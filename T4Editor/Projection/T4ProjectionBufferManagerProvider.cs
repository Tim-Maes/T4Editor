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

            // Only create one manager per buffer (multiple views may share a buffer)
            if (buffer.Properties.ContainsProperty(typeof(T4ProjectionBufferManager)))
                return;

            var manager = new T4ProjectionBufferManager(buffer);
            buffer.Properties.AddProperty(typeof(T4ProjectionBufferManager), manager);

            // Clean up when the view is closed
            textView.Closed += (s, e) =>
            {
                if (buffer.Properties.TryGetProperty(typeof(T4ProjectionBufferManager), out T4ProjectionBufferManager mgr))
                {
                    buffer.Properties.RemoveProperty(typeof(T4ProjectionBufferManager));
                    mgr.Dispose();
                }
            };
        }
    }
}
