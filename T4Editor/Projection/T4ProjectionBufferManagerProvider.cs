using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace T4Editor.Projection
{
    /// <summary>
    /// MEF-exported listener that bootstraps the projection buffer manager
    /// when a T4 file is opened in the editor. Creates the hidden C# workspace
    /// and attaches it to the text buffer's property bag for other components to use.
    /// </summary>
    [Export(typeof(ITextViewCreationListener))]
    [ContentType("T4")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal class T4ProjectionBufferManagerProvider : ITextViewCreationListener
    {
        public void TextViewCreated(ITextView textView)
        {
            var buffer = textView.TextBuffer;

            // One manager per buffer, shared by all its views (split window, New Window);
            // it lives until the last of those views closes
            var manager = buffer.Properties.GetOrCreateSingletonProperty(
                typeof(T4ProjectionBufferManager),
                () => new T4ProjectionBufferManager(buffer));
            manager.AddView();

            textView.Closed += (s, e) =>
            {
                if (manager.RemoveView())
                {
                    buffer.Properties.RemoveProperty(typeof(T4ProjectionBufferManager));
                    manager.Dispose();
                }
            };
        }
    }
}
