using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace T4Editor.Projection
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("T4")]
    [TagType(typeof(IErrorTag))]
    internal class T4CSharpErrorTaggerProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (textView.TextBuffer != buffer)
                return null;

            return buffer.Properties.GetOrCreateSingletonProperty(
                typeof(T4CSharpErrorTagger),
                () => new T4CSharpErrorTagger(buffer)) as ITagger<T>;
        }
    }

    /// <summary>
    /// Provides error squiggles in T4 code blocks by querying Roslyn diagnostics
    /// from the generated C# document and mapping them back to T4 positions.
    /// </summary>
    internal class T4CSharpErrorTagger : ITagger<IErrorTag>, IDisposable
    {
        private readonly ITextBuffer _buffer;
        private List<ITagSpan<IErrorTag>> _currentTags = new List<ITagSpan<IErrorTag>>();
        private CancellationTokenSource _cts;
        private Timer _timer;
        private bool _disposed;

        private const int UpdateDelayMs = 500;

        public T4CSharpErrorTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _buffer.Changed += OnBufferChanged;

            // Initial diagnostic fetch (delayed to let projection manager initialize)
            _timer = new Timer(_ => UpdateDiagnostics(), null, 1000, Timeout.Infinite);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            return _currentTags;
        }

        private void OnBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            // Debounce diagnostic updates
            _cts?.Cancel();
            _timer?.Change(UpdateDelayMs, Timeout.Infinite);
        }

        private void UpdateDiagnostics()
        {
            if (_disposed) return;

            if (!_buffer.Properties.TryGetProperty(typeof(T4ProjectionBufferManager), out T4ProjectionBufferManager manager))
                return;

            var document = manager.WorkspaceManager?.Document;
            if (document == null) return;

            try
            {
                _cts = new CancellationTokenSource();
                var diagnostics = GetDiagnosticsAsync(document, _cts.Token).Result;
                var snapshot = _buffer.CurrentSnapshot;
                var newTags = new List<ITagSpan<IErrorTag>>();

                foreach (var diag in diagnostics)
                {
                    if (diag.Severity < DiagnosticSeverity.Warning)
                        continue;

                    var genSpan = diag.Location.SourceSpan;
                    
                    // Map generated position back to T4
                    int? t4Start = manager.MapFromGenerated(genSpan.Start);
                    int? t4End = manager.MapFromGenerated(genSpan.End);

                    if (!t4Start.HasValue || !t4End.HasValue)
                        continue;

                    int start = t4Start.Value;
                    int length = t4End.Value - start;

                    if (start < 0 || start + length > snapshot.Length || length <= 0)
                        continue;

                    var errorType = diag.Severity == DiagnosticSeverity.Error
                        ? PredefinedErrorTypeNames.SyntaxError
                        : PredefinedErrorTypeNames.Warning;

                    var span = new SnapshotSpan(snapshot, start, length);
                    var tag = new ErrorTag(errorType, diag.GetMessage());
                    newTags.Add(new TagSpan<IErrorTag>(span, tag));
                }

                _currentTags = newTags;
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(
                    new SnapshotSpan(snapshot, 0, snapshot.Length)));
            }
            catch (Exception)
            {
                // Don't crash on diagnostic failures
            }
        }

        private async Task<IEnumerable<Diagnostic>> GetDiagnosticsAsync(
            Document document, CancellationToken token)
        {
            var semanticModel = await document.GetSemanticModelAsync(token);
            if (semanticModel == null) return Enumerable.Empty<Diagnostic>();

            return semanticModel.GetDiagnostics(cancellationToken: token)
                .Where(d => d.Location.IsInSource);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _buffer.Changed -= OnBufferChanged;
                _cts?.Cancel();
                _cts?.Dispose();
                _timer?.Dispose();
            }
        }
    }
}
