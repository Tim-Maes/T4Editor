using Microsoft.VisualStudio.PlatformUI;
using System.Drawing;

namespace T4Editor.Common
{
    /// <summary>
    /// Utility class for detecting Visual Studio theme (Dark/Light) based on color luminance
    /// instead of hardcoded color values
    /// </summary>
    internal static class ThemeDetector
    {
        /// <summary>
        /// Determines if the current Visual Studio theme is dark by analyzing the luminance
        /// of the tool window background color
        /// </summary>
        /// <returns>True if dark theme, false if light theme</returns>
        public static bool IsDarkTheme()
        {
            var themeColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
            return IsDarkColor(themeColor);
        }

        /// <summary>
        /// Determines if a color is dark based on its perceived luminance
        /// Uses the standard luminance formula: 0.299*R + 0.587*G + 0.114*B
        /// </summary>
        /// <param name="color">The color to analyze</param>
        /// <returns>True if the color is dark (luminance < 128), false otherwise</returns>
        private static bool IsDarkColor(Color color)
        {
            // Calculate perceived luminance using standard formula
            double luminance = (0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B);
            
            // Consider anything below 128 (50% of 255) as dark
            return luminance < 128;
        }
    }
}