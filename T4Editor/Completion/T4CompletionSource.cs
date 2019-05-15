using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using System;
using System.Collections.Generic;

namespace T4Editor.Intellisense
{
    internal class T4CompletionSource : ICompletionSource
    {
        private T4CompletionSourceProvider _sourceProvider;
        private ITextBuffer _textBuffer;
        private List<Completion2> _completionList;

        public T4CompletionSource(T4CompletionSourceProvider sourceProvider, ITextBuffer textBuffer)
        {
            _sourceProvider = sourceProvider;
            _textBuffer = textBuffer;
        }

        void ICompletionSource.AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            _completionList = new List<Completion2>
            {
                new Completion2("<# #>", "<#   #>", "Control Block", null, null),
                new Completion2("<#+ #>", "<#+   #>", "Class Feature Block", null, null),
                new Completion2("<#= #>", "<#=   #>", "Expression Block", null, null),
                new Completion2("<#@ import #>", "<#@ import namespace=\"\"#>", "Import Directive", null, null),
                new Completion2("<#@ include #>", "<#@ include file=\"\"#>", "Include Directive", null, null)
            };

            completionSets.Add(new CompletionSet(
                "Tokens",   
                "Tokens",   
                FindTokenSpanAtPosition(session.GetTriggerPoint(_textBuffer),
                    session),
                _completionList,
                null));
        }

        private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point, ICompletionSession session)
        {
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            ITextStructureNavigator navigator = _sourceProvider.NavigatorService.GetTextStructureNavigator(_textBuffer);
            TextExtent extent = navigator.GetExtentOfWord(currentPoint);
            return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
        }

        private bool _isDisposed;
        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }
    }
}
