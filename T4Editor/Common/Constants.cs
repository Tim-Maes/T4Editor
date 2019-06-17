namespace T4Editor.Common
{
    internal static class Constants
    {
        #region RegularExpressions

        internal const string StatementBlockRegex = "(?<!\")<#((?!\"|=|@|\\+))((?!(?!)<#(?!\\+|\\=)|#>)[\\s|\\w|\\d|\n|().,<>\\-:;@#$%^&=*\\[\\]\"'+\\/\\\\®°⁰!?{}|`~])*(?=\\s?|\\w?|\n?)(?<!\")#>(?!\")";
        internal const string OutputBlockRegex = "(?<=#>)(((?!<#(?!\\+|\\=|\")|#>(?!\"))[\\s|\\w|\\d|\n|().,<>\\-:;@#$%^&=*\\[\\]\"'+\\/\\\\®°⁰!?{}|`~])*(?=\\s|\\w|\\n?))(?=<#)";
        internal const string ClassFeatureBlockRegex = "<#\\+((?!<#(?!\\+|\\=)|#>)[\\s|\\w|\\d|\n|().,<>\\-:;@#$%^&=*\\[\\]\"'+\\/\\\\®°⁰!?{}|`~])*(?=\\s|\\w|\\n?)#>";
        internal const string DirectiveRegex = "(?<=<#@)((?!<#(?!\\+|\\=)|#>)[\\s|\\w|\\d|\n|().,<>\\-:;@#$%^&=*\\[\\]\"'+\\/\\\\®°⁰!?{}|`~])*(?=\\s|\\w|\\n?)(?=#>)";
        internal const string ExpressionBlockRegex = "<#=((?!<#(?!\\+|\\=)|#>)[\\s|\\w|\\d|\n|().,<>\\-:;@#$%^&=*\\[\\]\"'+\\/\\\\®°⁰!?{}|`~])*(\\s?)#>";

        #endregion

        #region ContentTypes

        internal const string BaseContentType = "T4";
        internal const string TTFileExtension = ".tt";
        internal const string T4FileExtension = ".t4";
        internal const string TTIncludeFileExtension = ".ttinclude";

        #endregion

        #region ClassificationTypes

        internal const string StatementBlock = "T4.StatementBlock";
        internal const string ClassFeatureBlock = "T4.ClassFeatureBlock";
        internal const string DirectiveBlock = "T4.Directive";
        internal const string OutputBlock = "T4.Output";
        internal const string ExpressionBlock = "T4.Expression";

        #endregion

        #region Guids

        internal const string CommandSetGuid = "bfb75b6d-fe6d-4894-9cde-a1714bc797c2";
        internal const string PackageGuid = "c556d31f-824b-450e-879a-39e102ca1ae4";

        #endregion
    }
}
