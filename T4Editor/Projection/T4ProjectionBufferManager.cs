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
    /// </summary>
    internal class T4ProjectionBufferManager : IDisposable
    {
        private readonly ITextBuffer _t4Buffer;
        private readonly T4CSharpDocumentGenerator _generator;
        private readonly T4CSharpWorkspaceManager _workspaceManager;
        private GeneratedDocument _currentDocument;
        private CancellationTokenSource _debounceCts;
        private bool _disposed;

        private const int DebounceDelayMs = 300;

        public T4ProjectionBufferManager(ITextBuffer t4Buffer)
        {
            _t4Buffer = t4Buffer ?? throw new ArgumentNullException(nameof(t4Buffer));
            _generator = new T4CSharpDocumentGenerator();
            _workspaceManager = new T4CSharpWorkspaceManager();

            _workspaceManager.Initialize();
            RegenerateDocument();

            _t4Buffer.Changed += OnBufferChanged;
        }

        /// <summary>The current generated document with span mappings</summary>
        public GeneratedDocument CurrentDocument => _currentDocument;

        /// <summary>The Roslyn workspace manager for querying language services</summary>
        public T4CSharpWorkspaceManager WorkspaceManager => _workspaceManager;

        /// <summary>
        /// Maps a caret position in the T4 buffer to the generated C# document.
        /// Returns null if the position is not within a C# code block.
        /// </summary>
        public int? MapToGenerated(int t4Position)
        {
            return _currentDocument?.MapToGenerated(t4Position);
        }

        /// <summary>
        /// Maps a position in the generated C# back to the T4 buffer.
        /// </summary>
        public int? MapFromGenerated(int generatedPosition)
        {
            return _currentDocument?.MapFromGenerated(generatedPosition);
        }

        private void OnBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            // Debounce: cancel previous pending regeneration
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            Task.Delay(DebounceDelayMs, token).ContinueWith(t =>
            {
                if (!t.IsCanceled && !_disposed)
                {
                    RegenerateDocument();
                }
            }, TaskScheduler.Default);
        }

        private void RegenerateDocument()
        {
            try
            {
                var snapshot = _t4Buffer.CurrentSnapshot;
                var text = snapshot.GetText();
                _currentDocument = _generator.Generate(text);
                _workspaceManager.UpdateDocument(_currentDocument.CSharpCode);
            }
            catch (Exception)
            {
                // Don't crash the editor on parse errors
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _t4Buffer.Changed -= OnBufferChanged;
                _debounceCts?.Cancel();
                _debounceCts?.Dispose();
                _workspaceManager.Dispose();
            }
        }
    }
}
