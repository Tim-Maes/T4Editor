using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace T4Editor
{
    /// <summary>
    /// Classification type definition export for T4Classifier
    /// </summary>
    internal static class T4ClassifierClassificationDefinition
    {
        // This disables "The field is never used" compiler's warning. Justification: the field is used by MEF.
#pragma warning disable 169

        [Export]
        [Name("T4")]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition ContentDefinition;

        [Export]
        [FileExtension(".tt")]
        [ContentType("T4")]
        internal static FileExtensionToContentTypeDefinition t4FileExtensionDefinition;

        [Export]
        [FileExtension(".ttinclude")]
        [ContentType("T4")]
        internal static FileExtensionToContentTypeDefinition ttincludeFileExtensionDefinition;

        #region Classification Type Definitions

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

        #endregion


#pragma warning restore 169
    }
}
