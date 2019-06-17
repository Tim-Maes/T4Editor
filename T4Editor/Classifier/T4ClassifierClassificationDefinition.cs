using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using T4Editor.Common;

namespace T4Editor
{
    internal static class T4ClassifierClassificationDefinition
    {
#pragma warning disable CS0649

        [Export]
        [Name(Constants.BaseContentType)]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition ContentDefinition;

        [Export]
        [FileExtension(Constants.TTFileExtension)]
        [ContentType(Constants.BaseContentType)]
        internal static FileExtensionToContentTypeDefinition ttFileExtensionDefinition;

        [Export]
        [FileExtension(Constants.TTIncludeFileExtension)]
        [ContentType(Constants.BaseContentType)]
        internal static FileExtensionToContentTypeDefinition ttincludeFileExtensionDefinition;

        [Export]
        [FileExtension(Constants.T4FileExtension)]
        [ContentType(Constants.BaseContentType)]
        internal static FileExtensionToContentTypeDefinition t4FileExtensionDefinition;

        [Export]
        [Name(Constants.BaseContentType)]
        internal static ClassificationTypeDefinition T4ClassificationDefinition;

        [Export]
        [Name(Constants.ClassFeatureBlock)]
        [BaseDefinition(Constants.BaseContentType)]
        internal static ClassificationTypeDefinition classFeatureBlockDefinition;

        [Export]
        [Name(Constants.StatementBlock)]
        [BaseDefinition(Constants.BaseContentType)]
        internal static ClassificationTypeDefinition statementBlockDefinition;

        [Export]
        [Name(Constants.DirectiveBlock)]
        [BaseDefinition(Constants.BaseContentType)]
        internal static ClassificationTypeDefinition directiveDefinition;

        [Export]
        [Name(Constants.OutputBlock)]
        [BaseDefinition(Constants.BaseContentType)]
        internal static ClassificationTypeDefinition outputDefinition;

        [Export]
        [Name(Constants.ExpressionBlock)]
        [BaseDefinition(Constants.BaseContentType)]
        internal static ClassificationTypeDefinition expressionDefinition;

#pragma warning restore 169
    }
}
