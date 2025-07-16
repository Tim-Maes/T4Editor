using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using T4Editor.Common;

namespace T4Editor
{
    [Export(typeof(IClassifierProvider))]
    [ContentType(Constants.BaseContentType)]
    internal class T4ClassifierProvider : IClassifierProvider
    {
#pragma warning disable 649

        [Import]
        private IClassificationTypeRegistryService classificationRegistry;

#pragma warning restore 649

        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            // Performance improvement: Create separate classifier instances per buffer
            // instead of sharing a single static instance
            return buffer.Properties.GetOrCreateSingletonProperty(
                typeof(T4Classifier),
                () => new T4Classifier(buffer, classificationRegistry));
        }
    }
}
