using System;
using System.Text.RegularExpressions;

namespace T4Editor.Common
{
    internal static class Constants
    {
        #region RegularExpressions

        private const string ControlBlockRegexPattern = /* language=regex */ "(?<openingtag>(?<!\")<#((?!\"|=|@|\\+)))(?<code>((?!(?!)<#(?!\\+|=)|#>)[\\s|\\w|\\d|\\n|().,<>\\-:;@#$%^&=*\\[\\]\"'+\\/\\®°⁰!?{}|`~\\\\u2000-\\\\u206F\\\\u2E00-\\\\u2E7F])*)(?<closingtag>(?=\\s?|\\w?|\\n?)(?<!\")#>(?!\"))";
        private const string ClassFeatureBlockRegexPattern = /* language=regex */ "(?<openingtag>(?<!\\\")<#\\+((?!\"|=|@|\\+)))(?<code>((?!(?!)<#(?!\\\\+|=)|#>)[\\s|\\w|\\d|\\\\n|().,<>\\-:;@#$%^&=*\\[\\]\\\"'+\\/\\®°⁰!?{}|`~\\\\u2000-\\\\u206F\\\\u2E00-\\\\u2E7F])*)(?<closingtag>(?=\\s?|\\w?|\\n?)(?<!\")#>(?!\"))";
        private const string OutputBlockRegexPattern = /* language=regex */ "(?<=#>)(((?!<#(?!\\+|\\=|\")|#>)[\\s|\\w|\\d|\n|().,<>\\-:;@#$%^&=*\\[\\]\"'+\\/\\\\®°⁰!?{}|`~\\u2000-\\u206F\\u2E00-\\u2E7F])*(?=\\s|\\w|\\n?))(?=(<#)|$(?![\\r\\n]))";
        private const string DirectiveRegexPattern = /* language=regex */ "(?<openingtag>^(<#@))(?<code>((?!<#(?!\\+|\\=)|#>)[\\s|\\w|\\d|\\n|().,<>\\-:;@#$%^&=*\\[\\]\"'+\\/\\\\®°⁰!?{}|`~\\\\u2000-\\\\u206F\\\\u2E00-\\\\u2E7F])*)(?<closingtag>(?=\\s?|\\w?|\\n?)(?<!\")#>(?!\"))";
        private const string ExpressionBlockRegexPattern = /* language=regex */ "(?<openingtag><#=)(?<code>((?!<#(?!\\+|\\=)|#>)[\\s|\\w|\\d|\\n|().,<>\\-:;@#$%^&=*\\[\\]\\\"'+\\/\\\\®°⁰!?{}|`~\\\\u2000-\\\\u206F\\\\u2E00-\\\\u2E7F])*)(\\s?)(?<closingtag>#>)";

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

        #region VSColorThemes
        internal const string DarkTheme = "Color [A=255, R=31, G=31, B=31]";
        internal const string LightTheme = "Color [A=255, R=245, G=245, B=245]";
        #endregion

        #region Performance Constants
        
        // Performance thresholds for switching between parsing strategies
        internal const int LARGE_FILE_THRESHOLD = 50000; // 50KB
        internal const int MAX_INCREMENTAL_PARSE_SIZE = 10000; // 10KB chunks
        internal const int REGEX_TIMEOUT_MS = 100; // Timeout for regex operations
        
        #endregion

        internal static class Regexen
        {
            // Performance optimization: Add timeout to prevent regex catastrophic backtracking
            private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(REGEX_TIMEOUT_MS);
            
            public static Regex ControlBlock { get; } = new Regex(Constants.ControlBlockRegexPattern, 
                RegexOptions.Compiled | RegexOptions.ExplicitCapture, RegexTimeout);
            public static Regex ClassFeatureBlock { get; } = new Regex(Constants.ClassFeatureBlockRegexPattern, 
                RegexOptions.Compiled | RegexOptions.ExplicitCapture, RegexTimeout);
            public static Regex OutputBlock { get; } = new Regex(Constants.OutputBlockRegexPattern, 
                RegexOptions.Compiled | RegexOptions.ExplicitCapture, RegexTimeout);
            public static Regex Directive { get; } = new Regex(Constants.DirectiveRegexPattern, 
                RegexOptions.Compiled | RegexOptions.ExplicitCapture, RegexTimeout);
            public static Regex ExpressionBlock { get; } = new Regex(Constants.ExpressionBlockRegexPattern, 
                RegexOptions.Compiled | RegexOptions.ExplicitCapture, RegexTimeout);
        }
    }
}
