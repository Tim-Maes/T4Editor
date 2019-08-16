using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace T4Editor.Classifier
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("T4")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal class TextViewCreationListener : IWpfTextViewCreationListener
    {
#pragma warning disable 649

        [Import]
        private readonly TextViewColorizersManager _colorizersManager;

#pragma warning restore 649

        public void TextViewCreated(IWpfTextView textView)
        {
            TextViewColorizer colorizer = new TextViewColorizer(textView);
            if (_colorizersManager != null) _colorizersManager.AddColorizer(colorizer);
        }
    }
}
