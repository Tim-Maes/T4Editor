using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

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
    /// Diagnostics are computed in the background whenever the projection is regenerated.
    /// Not IDisposable on purpose: one instance is shared by all views of the buffer,
    /// so a closing view must not tear it down.
    /// </summary>
    internal class T4CSharpErrorTagger : ITagger<IErrorTag>
    {
        private readonly ITextBuffer _buffer;
        private readonly object _resultsLock = new object();
        private T4ProjectionBufferManager _manager;
        private volatile DiagnosticResults _results;

        public T4CSharpErrorTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            EnsureSubscribed();

            var results = _results;
            if (results == null || spans.Count == 0)
                yield break;

            var targetSnapshot = spans[0].Snapshot;
            foreach (var tag in results.Tags)
            {
                var span = results.Snapshot == targetSnapshot
                    ? tag.Span
                    : tag.Span.TranslateTo(targetSnapshot, SpanTrackingMode.EdgeExclusive);

                if (span.Length > 0 && spans.IntersectsWith(span))
                    yield return new TagSpan<IErrorTag>(span, tag.Tag);
            }
        }

        /// <summary>
        /// Attaches to the buffer's projection manager, which is created by a view listener
        /// and replaced if all views close and the buffer is opened again. UI thread only.
        /// </summary>
        private void EnsureSubscribed()
        {
            _buffer.Properties.TryGetProperty(typeof(T4ProjectionBufferManager), out T4ProjectionBufferManager manager);
            if (manager == _manager)
                return;

            if (_manager != null)
                _manager.StateChanged -= OnStateChanged;

            _manager = manager;

            if (_manager != null)
            {
                _manager.StateChanged += OnStateChanged;
                OnStateChanged(_manager, EventArgs.Empty);
            }
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            var state = ((T4ProjectionBufferManager)sender).CurrentState;
            if (state == null || _results?.State == state)
                return;

            _ = ThreadHelper.JoinableTaskFactory.RunAsync(() => UpdateDiagnosticsAsync(state));
        }

        private async Task UpdateDiagnosticsAsync(T4ProjectionState state)
        {
            await TaskScheduler.Default;

            var newTags = new List<ITagSpan<IErrorTag>>();
            try
            {
                var diagnostics = await GetDiagnosticsAsync(state.Document, CancellationToken.None);
                var snapshot = state.Snapshot;

                foreach (var diag in diagnostics)
                {
                    if (diag.Severity < DiagnosticSeverity.Warning)
                        continue;

                    var genSpan = diag.Location.SourceSpan;

                    // Map generated position back to T4
                    int? t4Start = state.Generated.MapFromGenerated(genSpan.Start);
                    int? t4End = state.Generated.MapFromGenerated(genSpan.End);

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
            }
            catch (Exception)
            {
                // Don't crash on diagnostic failures
                return;
            }

            lock (_resultsLock)
            {
                // A newer state may have finished first; never go backwards
                var current = _results;
                if (current != null && current.Snapshot.Version.VersionNumber >= state.Snapshot.Version.VersionNumber)
                    return;

                _results = new DiagnosticResults(state, newTags);
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var currentSnapshot = _buffer.CurrentSnapshot;
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(
                new SnapshotSpan(currentSnapshot, 0, currentSnapshot.Length)));
        }

        private async Task<IEnumerable<Diagnostic>> GetDiagnosticsAsync(
            Document document, CancellationToken token)
        {
            var semanticModel = await document.GetSemanticModelAsync(token);
            if (semanticModel == null) return Enumerable.Empty<Diagnostic>();

            return semanticModel.GetDiagnostics(cancellationToken: token)
                .Where(d => d.Location.IsInSource);
        }

        private sealed class DiagnosticResults
        {
            public DiagnosticResults(T4ProjectionState state, List<ITagSpan<IErrorTag>> tags)
            {
                State = state;
                Tags = tags;
            }

            public T4ProjectionState State { get; }
            public ITextSnapshot Snapshot => State.Snapshot;
            public List<ITagSpan<IErrorTag>> Tags { get; }
        }
    }
}
