using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace T4Editor.Intellisense
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("T4")]
    [Name("T4 completion")]
    internal class T4CompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new T4CompletionSource(this, textBuffer);
        }
    }
}
