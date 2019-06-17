using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using T4Editor.Common;

namespace T4Editor.Intellisense
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("T4 completion handler")]
    [ContentType(Constants.BaseContentType)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class T4CompletionHandlerProvider : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService = null;
        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }
        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            Func<T4CompletionCommandHandler> createCommandHandler = delegate ()
            {
                return new T4CompletionCommandHandler(textViewAdapter, textView, this);
            };

            textView.Properties.GetOrCreateSingletonProperty(createCommandHandler);
        }
    }
}
