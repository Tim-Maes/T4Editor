using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace T4Editor
{
    internal static class T4ClassifierClassificationDefinition
    {
#pragma warning disable CS0649

        [Export]
        [Name("T4")]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition ContentDefinition;

        [Export]
        [FileExtension(".tt")]
        [ContentType("T4")]
        internal static FileExtensionToContentTypeDefinition t4FileExtensionDefinition;

        [Export]
        [FileExtension(".ttinclude")]
        [ContentType("T4")]
        internal static FileExtensionToContentTypeDefinition ttincludeFileExtensionDefinition;

        [Export]
        [Name("T4")]
        internal static ClassificationTypeDefinition T4ClassificationDefinition;

        [Export]
        [Name("T4.ClassFeatureBlock")]
        [BaseDefinition("T4")]
        internal static ClassificationTypeDefinition classFeatureBlockDefinition;

        [Export]
        [Name("T4.StatementBlock")]
        [BaseDefinition("T4")]
        internal static ClassificationTypeDefinition statementBlockDefinition;

        [Export]
        [Name("T4.Directive")]
        [BaseDefinition("T4")]
        internal static ClassificationTypeDefinition directiveDefinition;

        [Export]
        [Name("T4.Output")]
        [BaseDefinition("T4")]
        internal static ClassificationTypeDefinition outputDefinition;

        [Export]
        [Name("T4.Injected")]
        [BaseDefinition("T4")]
        internal static ClassificationTypeDefinition injectDefinition;

#pragma warning restore 169
    }
}
