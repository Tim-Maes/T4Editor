using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace T4Editor
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("T4")]
    [FileExtension(".tt")]
    internal class T4ClassifierProvider : IClassifierProvider
    {
#pragma warning disable 649

        [Import]
        private IClassificationTypeRegistryService classificationRegistry;

#pragma warning restore 649

        static T4Classifier classifier;

        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            if (classifier == null)
                classifier = new T4Classifier(buffer, classificationRegistry);

            return classifier;
        }
    }
}
