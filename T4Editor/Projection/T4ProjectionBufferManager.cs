using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace T4Editor.Projection
{
    /// <summary>
    /// Manages the lifecycle of the hidden C# buffer and Roslyn workspace
    /// for a single T4 document. Listens for edits and re-generates the
    /// C# document with debouncing for performance.
    /// All generation work runs on a background thread; results are published
    /// as an immutable <see cref="T4ProjectionState"/>.
    /// </summary>
    internal class T4ProjectionBufferManager : IDisposable
    {
        private readonly ITextBuffer _t4Buffer;
        private readonly T4CSharpDocumentGenerator _generator;
        private readonly T4CSharpWorkspaceManager _workspaceManager;
        private readonly object _regenerateLock = new object();
        private volatile T4ProjectionState _currentState;
        private CancellationTokenSource _debounceCts;
        private bool _workspaceInitialized;
        private int _viewCount;
        private volatile bool _disposed;

        private const int DebounceDelayMs = 300;

        public T4ProjectionBufferManager(ITextBuffer t4Buffer)
        {
            _t4Buffer = t4Buffer ?? throw new ArgumentNullException(nameof(t4Buffer));
            _generator = new T4CSharpDocumentGenerator();
            _workspaceManager = new T4CSharpWorkspaceManager();

            _t4Buffer.Changed += OnBufferChanged;

            // Creating the workspace loads Roslyn's MEF host, which is slow: do it off the UI thread
            ScheduleRegeneration(0);
        }

        /// <summary>
        /// The latest published projection, or null until the first generation completes.
        /// It may lag behind the buffer's current snapshot while an edit is being debounced.
        /// </summary>
        public T4ProjectionState CurrentState => _currentState;

        /// <summary>
        /// Raised on a background thread whenever a new <see cref="CurrentState"/> is published.
        /// </summary>
        public event EventHandler StateChanged;

        /// <summary>Registers a text view using this manager. UI thread only.</summary>
        public void AddView() => _viewCount++;

        /// <summary>Unregisters a text view; returns true when no views are left. UI thread only.</summary>
        public bool RemoveView() => --_viewCount <= 0;

        /// <summary>
        /// Returns a projection matching <paramref name="snapshot"/> exactly. When the published state
        /// is older, a fresh one is generated on the spot without touching the workspace, which is
        /// cheap: generation is plain string processing and forking a Roslyn document is incremental.
        /// Returns null if no state has been published yet.
        /// </summary>
        public T4ProjectionState GetStateForSnapshot(ITextSnapshot snapshot)
        {
            var state = _currentState;
            if (state == null || state.Snapshot == snapshot)
                return state;

            var generated = _generator.Generate(snapshot.GetText());
            var document = state.Document.WithText(SourceText.From(generated.CSharpCode));
            return new T4ProjectionState(snapshot, generated, document);
        }

        private void OnBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            ScheduleRegeneration(DebounceDelayMs);
        }

        private void ScheduleRegeneration(int delayMs)
        {
            // Debounce: cancel previous pending regeneration
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            _ = Task.Delay(delayMs, token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    RegenerateDocument(token);
                }
            }, TaskScheduler.Default);
        }

        private void RegenerateDocument(CancellationToken token)
        {
            // Serialize regenerations so the workspace and published state always move forward together
            lock (_regenerateLock)
            {
                if (_disposed || token.IsCancellationRequested)
                    return;

                try
                {
                    var snapshot = _t4Buffer.CurrentSnapshot;
                    if (_currentState?.Snapshot == snapshot)
                        return;

                    if (!_workspaceInitialized)
                    {
                        _workspaceManager.Initialize();
                        _workspaceInitialized = true;
                    }

                    var generated = _generator.Generate(snapshot.GetText());
                    var document = _workspaceManager.UpdateDocument(generated.CSharpCode);
                    if (document == null)
                        return;

                    _currentState = new T4ProjectionState(snapshot, generated, document);
                }
                catch (Exception)
                {
                    // Don't crash the editor on parse errors
                    return;
                }
            }

            if (!_disposed)
            {
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _t4Buffer.Changed -= OnBufferChanged;
                _debounceCts?.Cancel();
                StateChanged = null;

                // A regeneration may still be running (e.g. the initial workspace load);
                // wait for it on a background thread instead of blocking the UI thread
                _ = Task.Run(() =>
                {
                    lock (_regenerateLock)
                    {
                        _workspaceManager.Dispose();
                    }
                });
            }
        }
    }
}
