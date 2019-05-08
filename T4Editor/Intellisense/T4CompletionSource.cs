using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using System;
using System.Collections.Generic;

namespace T4Editor.Intellisense
{
    internal class T4CompletionSource : ICompletionSource
    {
        private T4CompletionSourceProvider m_sourceProvider;
        private ITextBuffer m_textBuffer;
        private List<Completion> m_compList;

        public T4CompletionSource(T4CompletionSourceProvider sourceProvider, ITextBuffer textBuffer)
        {
            m_sourceProvider = sourceProvider;
            m_textBuffer = textBuffer;
        }
        void ICompletionSource.AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            List<string> strList = new List<string>();
            m_compList = new List<Completion>();

            m_compList.Add(new Completion("Control Block", "<#  #>", "Control Block", null, null));
            m_compList.Add(new Completion("Class Feature Block", "<#+  #>", "Class Feature Block", null, null));
            m_compList.Add(new Completion("Expression Block", "<#=  #>", "Expression Block", null, null));
            m_compList.Add(new Completion("Import Directive", "<#@ import  #>", "Import Directive", null, null));
            m_compList.Add(new Completion("Include Directive", "<#@ include  #>", "Include Directive", null, null));

            completionSets.Add(new CompletionSet(
                "Tokens",   
                "Tokens",   
                FindTokenSpanAtPosition(session.GetTriggerPoint(m_textBuffer),
                    session),
                m_compList,
                null));
        }

        private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point, ICompletionSession session)
        {
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            ITextStructureNavigator navigator = m_sourceProvider.NavigatorService.GetTextStructureNavigator(m_textBuffer);
            TextExtent extent = navigator.GetExtentOfWord(currentPoint);
            return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
        }

        private bool m_isDisposed;
        public void Dispose()
        {
            if (!m_isDisposed)
            {
                GC.SuppressFinalize(this);
                m_isDisposed = true;
            }
        }
    }
}
