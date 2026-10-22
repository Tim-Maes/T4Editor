using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace T4Editor.Projection
{
    /// <summary>
    /// Provides C# IntelliSense completions inside T4 code blocks by querying
    /// the Roslyn workspace's CompletionService with the mapped position.
    /// </summary>
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("T4")]
    [Name("T4 C# Completion")]
    [Order(Before = "default")]
    internal class T4CSharpCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(
                typeof(T4CSharpCompletionSource),
                () => new T4CSharpCompletionSource(textBuffer, NavigatorService));
        }
    }

    internal class T4CSharpCompletionSource : ICompletionSource
    {
        private readonly ITextBuffer _textBuffer;
        private readonly ITextStructureNavigatorSelectorService _navigatorService;
        private bool _disposed;

        private const int CompletionTimeoutMs = 500;

        public T4CSharpCompletionSource(
            ITextBuffer textBuffer,
            ITextStructureNavigatorSelectorService navigatorService)
        {
            _textBuffer = textBuffer;
            _navigatorService = navigatorService;
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            if (_disposed) return;

            // Get the projection manager from buffer properties
            if (!_textBuffer.Properties.TryGetProperty(typeof(T4ProjectionBufferManager), out T4ProjectionBufferManager manager))
                return;

            // Get caret position in the T4 buffer
            var caretPoint = session.TextView.Caret.Position.BufferPosition;
            int t4Position = caretPoint.Position;

            // Completion usually triggers on the keystroke that just happened, before the
            // debounced regeneration: use a projection that includes it
            T4ProjectionState state;
            try
            {
                state = manager.GetStateForSnapshot(caretPoint.Snapshot);
            }
            catch (Exception)
            {
                return;
            }
            if (state == null) return;

            // Map to generated C# position
            int? generatedPosition = state.Generated.MapToGenerated(t4Position);
            if (!generatedPosition.HasValue)
                return; // Not inside a C# block

            // This legacy completion API is synchronous, so Roslyn runs on the thread pool
            // with a bounded wait: a slow query (e.g. the first one) yields no results
            // instead of freezing the UI thread
            IEnumerable<CompletionItem> completions;
            var cts = new CancellationTokenSource();
            try
            {
                var completionTask = Task.Run(() => GetRoslynCompletionsAsync(state.Document, generatedPosition.Value, cts.Token));
#pragma warning disable VSTHRD002 // Bounded wait; the task never needs the UI thread
                if (!completionTask.Wait(CompletionTimeoutMs))
                {
                    cts.Cancel();
                    return;
                }
                completions = completionTask.Result;
#pragma warning restore VSTHRD002
            }
            catch (Exception)
            {
                return;
            }
            if (completions == null || !completions.Any())
                return;

            // Build VS completion items
            var vsCompletions = completions.Select(item =>
                new Microsoft.VisualStudio.Language.Intellisense.Completion(
                    item.DisplayText,
                    item.DisplayText,
                    item.DisplayText,
                    null,
                    null))
                .ToList();

            // Find the tracking span for replacement
            var trackingSpan = FindTokenSpanAtPosition(session);

            completionSets.Add(new CompletionSet(
                "T4CSharp",
                "C#",
                trackingSpan,
                vsCompletions,
                null));
        }

        private async Task<IEnumerable<CompletionItem>> GetRoslynCompletionsAsync(
            Document document, int position, CancellationToken token)
        {
            try
            {
                var completionService = CompletionService.GetService(document);
                if (completionService == null) return null;

                var results = await completionService.GetCompletionsAsync(document, position, cancellationToken: token);
                return results?.Items;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private ITrackingSpan FindTokenSpanAtPosition(ICompletionSession session)
        {
            var caretPoint = session.TextView.Caret.Position.BufferPosition;
            var navigator = _navigatorService.GetTextStructureNavigator(_textBuffer);
            var extent = navigator.GetExtentOfWord(caretPoint);

            // If caret is at end of word, GetExtentOfWord may return the next word.
            // Try one position back to get the word being typed.
            if (extent.Span.Start == caretPoint && caretPoint.Position > 0)
            {
                extent = navigator.GetExtentOfWord(caretPoint - 1);
            }

            return caretPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
