using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Media;
using T4Editor.Classifier;
using T4Editor.Common;
using Constants = T4Editor.Common.Constants;
using Task = System.Threading.Tasks.Task;


namespace T4Editor.Commands
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] 
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuidString)]
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class OpenT4EditorSettingsCommandPackage : AsyncPackage
    {
        [Import]
        private TextViewColorizersManager _textViewsManager;

        public const string PackageGuidString = Constants.PackageGuid;

        public OpenT4EditorSettingsCommandPackage()
        {
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;

            if (Settings.Default.FirstInstall)
            {
                ApplyThemeColors(ThemeDetector.IsDarkTheme(), overwriteCustomColors: true);

                Settings.Default.FirstInstall = false;
                Settings.Default.Save();
            }

            await OpenT4EditorSettingsCommand.InitializeAsync(this);
        }

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            // Keep colors the user picked; only move the ones still at the other theme's defaults
            ApplyThemeColors(ThemeDetector.IsDarkTheme(), overwriteCustomColors: false);
        }

        private static readonly Dictionary<string, string> DarkThemeColors = new Dictionary<string, string>
        {
            [nameof(Settings.ClassFeatureBlockColor)] = "#DEB887",
            [nameof(Settings.ControlBlockColor)] = "#FFEBCD",
            [nameof(Settings.DirectiveColor)] = "#C0C0C0",
            [nameof(Settings.OutputColor)] = "#FFFFFF",
            [nameof(Settings.InjectedColor)] = "#DAA520",
            [nameof(Settings.TagBackground)] = "#FFFAFAD2",
            [nameof(Settings.TagColor)] = "#FF000000",
        };

        // Improved light theme colors with better contrast for readability
        private static readonly Dictionary<string, string> LightThemeColors = new Dictionary<string, string>
        {
            [nameof(Settings.ClassFeatureBlockColor)] = "#8B4513",  // SaddleBrown - better contrast than gray
            [nameof(Settings.ControlBlockColor)] = "#4682B4",       // SteelBlue - better visibility
            [nameof(Settings.DirectiveColor)] = "#228B22",          // ForestGreen - better than muted olive
            [nameof(Settings.OutputColor)] = "#2F4F4F",             // DarkSlateGray - much better contrast
            [nameof(Settings.InjectedColor)] = "#B8860B",           // DarkGoldenrod - better than black
            [nameof(Settings.TagBackground)] = "#F5F5DC",           // Beige - softer background
            [nameof(Settings.TagColor)] = "#000080",                // Navy - better than pure black
        };

        private void ApplyThemeColors(bool isDarkTheme, bool overwriteCustomColors)
        {
            var newDefaults = isDarkTheme ? DarkThemeColors : LightThemeColors;
            var oldDefaults = isDarkTheme ? LightThemeColors : DarkThemeColors;
            bool changed = false;

            foreach (var entry in newDefaults)
            {
                var current = (string)Settings.Default[entry.Key];
                if (overwriteCustomColors || IsSameColor(current, oldDefaults[entry.Key]))
                {
                    Settings.Default[entry.Key] = entry.Value;
                    changed = true;
                }
            }

            if (changed)
            {
                Settings.Default.Save();
                BatchUpdateColors();
            }
        }

        /// <summary>
        /// Compares colors by value: the color picker stores "#AARRGGBB" while defaults use "#RRGGBB".
        /// </summary>
        private static bool IsSameColor(string a, string b)
        {
            try
            {
                return (Color)ColorConverter.ConvertFromString(a) == (Color)ColorConverter.ConvertFromString(b);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void BatchUpdateColors()
        {
            if (_textViewsManager == null) this.SatisfyImportsOnce();

            var colors = GetColorsFromSettings();

            List<CategoryItemDecorationSettings> settings = new List<CategoryItemDecorationSettings>();

            for (var i = 0; i < colors.Count(); i++)
            {
                settings.Add(new CategoryItemDecorationSettings
                {
                    ForegroundColor = colors[i],
                    DisplayName = Constants.Types[i]
                });
            }

            settings.Add(new CategoryItemDecorationSettings
            {
                ForegroundColor = (Color)ColorConverter.ConvertFromString(Settings.Default.TagColor),
                BackgroundColor = (Color)ColorConverter.ConvertFromString(Settings.Default.TagBackground),
                DisplayName = Constants.Tag,
            });

            foreach (TextViewColorizer colorizer in _textViewsManager.GetColorizers())
            {
                colorizer.UpdateColors(settings);
            }
        }

        private Color[] GetColorsFromSettings()
        {
            return new Color[] {
             (Color)ColorConverter.ConvertFromString(Settings.Default.ClassFeatureBlockColor),
                (Color)ColorConverter.ConvertFromString(Settings.Default.ControlBlockColor),
                (Color)ColorConverter.ConvertFromString(Settings.Default.DirectiveColor),
                (Color)ColorConverter.ConvertFromString(Settings.Default.OutputColor),
                (Color)ColorConverter.ConvertFromString(Settings.Default.InjectedColor),
             };
        }
    }
}
