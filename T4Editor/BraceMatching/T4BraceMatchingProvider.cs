using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using T4Editor.Common;

namespace T4Editor.Completion
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType(Constants.BaseContentType)]
    [TagType(typeof(TextMarkerTag))]
    internal class T4BraceMatchingProvider : IViewTaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (textView == null)
                return null;

            if (textView.TextBuffer != buffer)
                return null;

            return new T4BraceMatchingTagger(textView, buffer) as ITagger<T>;
        }
    }
}
