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
                // Use the new robust theme detection instead of hardcoded color comparison
                if (ThemeDetector.IsDarkTheme())
                {
                    SetDarkThemeColors();
                }
                else
                {
                    SetLightThemeColors();
                }

                Settings.Default.FirstInstall = false;
                Settings.Default.Save();
            }

            await OpenT4EditorSettingsCommand.InitializeAsync(this);
        }

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            // Use the new robust theme detection instead of hardcoded color comparison
            if (ThemeDetector.IsDarkTheme())
            {
                SetDarkThemeColors();
            }
            else
            {
                SetLightThemeColors();
            }
        }

        private void SetDarkThemeColors()
        {
            Settings.Default.ClassFeatureBlockColor = "#DEB887";
            Settings.Default.ControlBlockColor = "#FFEBCD";
            Settings.Default.DirectiveColor = "#C0C0C0";
            Settings.Default.OutputColor = "#FFFFFF";
            Settings.Default.InjectedColor = "#DAA520";
            Settings.Default.TagBackground = "#FFFAFAD2";
            Settings.Default.TagColor = "#FF000000";
            Settings.Default.Save();
            BatchUpdateColors();
        }

        private void SetLightThemeColors()
        {
            // Improved light theme colors with better contrast for readability
            Settings.Default.ClassFeatureBlockColor = "#8B4513";    // SaddleBrown - better contrast than gray
            Settings.Default.ControlBlockColor = "#4682B4";         // SteelBlue - better visibility
            Settings.Default.DirectiveColor = "#228B22";           // ForestGreen - better than muted olive
            Settings.Default.OutputColor = "#2F4F4F";              // DarkSlateGray - much better contrast
            Settings.Default.InjectedColor = "#B8860B";            // DarkGoldenrod - better than black
            Settings.Default.TagBackground = "#F5F5DC";            // Beige - softer background
            Settings.Default.TagColor = "#000080";                 // Navy - better than pure black
            Settings.Default.Save();
            BatchUpdateColors();
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
