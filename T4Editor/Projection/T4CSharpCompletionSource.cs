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

            var document = manager.WorkspaceManager?.Document;
            if (document == null) return;

            // Get caret position in the T4 buffer
            var caretPoint = session.TextView.Caret.Position.BufferPosition;
            int t4Position = caretPoint.Position;

            // Map to generated C# position
            int? generatedPosition = manager.MapToGenerated(t4Position);
            if (!generatedPosition.HasValue)
                return; // Not inside a C# block

            // Query Roslyn completions (avoid deadlock by using JTF or ConfigureAwait false)
            IEnumerable<CompletionItem> completions;
            try
            {
                completions = Task.Run(() => GetRoslynCompletionsAsync(document, generatedPosition.Value)).Result;
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
            Document document, int position)
        {
            try
            {
                var completionService = CompletionService.GetService(document);
                if (completionService == null) return null;

                var results = await completionService.GetCompletionsAsync(document, position);
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
