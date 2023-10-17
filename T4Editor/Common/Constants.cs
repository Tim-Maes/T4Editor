using System.Text.RegularExpressions;

namespace T4Editor.Common
{
    internal static class Constants
    {
        #region RegularExpressions

        private const string VSOfficialPatternCleaner = /* language=regex */ @"(?<boilerplate>^(\\)+)(?=<\#)|
                                                                               (?<=([^\\]|^)(\\)*)<\#@(?<directive>.*?)(?<=[^\\](\\)*)\#>|
                                                                               (?<=([^\\]|^)(\\)*)<\#\+(?<classfeature>.*?)(?<=[^\\](\\)*)\#>|
                                                                               (?<=([^\\]|^)(\\)*)<\#=(?<expression>.*?)(?<=[^\\](\\)*)\#>|
                                                                               (?<=([^\\]|^)(\\)*)<\#(?<statement>.*?)(?<=[^\\](\\)*)\#>|
                                                                               (?<boilerplate>.+?)(?=((?<=[^\\](\\)*)<\#))|
                                                                               (?<boilerplate>.+)(?=$)";
        #endregion

        #region ContentTypes

        internal const string BaseContentType = "T4";
        internal const string TTFileExtension = ".tt";
        internal const string T4FileExtension = ".t4";
        internal const string TTIncludeFileExtension = ".ttinclude";

        #endregion

        #region ClassificationTypes

        internal static readonly string[] Types = { "T4.ClassFeature", "T4.Control", "T4.Directive", "T4.Output", "T4.Expression", "T4.Tag" };
        internal const string ClassFeatureBlock = "T4.ClassFeature";
        internal const string ControlBlock = "T4.Control";
        internal const string DirectiveBlock = "T4.Directive";
        internal const string OutputBlock = "T4.Output";
        internal const string ExpressionBlock = "T4.Expression";
        internal const string Tag = "T4.Tag";

        #endregion

        #region OutliningRegion

        internal const string BlockStartHide = "<#";
        internal const string BlockEndHide = "#>";
        internal const string BlockEllipsis = "<#..#>";
        internal const string BlockHoverText = "hidden block";

        #endregion

        #region Guids

        internal const string CommandSetGuid = "bfb75b6d-fe6d-4894-9cde-a1714bc797c2";
        internal const string PackageGuid = "c556d31f-824b-450e-879a-39e102ca1ae4";

        #endregion

        internal static class Regexen
        {
            public static Regex VSOfficialPattern { get; } = new Regex(Constants.VSOfficialPatternCleaner, RegexOptions.Compiled);
        }
    }
}
