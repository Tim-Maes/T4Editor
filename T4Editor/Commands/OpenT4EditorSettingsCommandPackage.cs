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
                var themedColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey).ToString();

                if (themedColor == Constants.LightTheme) SetLightThemeColors();
                else SetDarkThemeColors();

                Settings.Default.FirstInstall = false;
                Settings.Default.Save();
            }

            await OpenT4EditorSettingsCommand.InitializeAsync(this);
        }

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            var themedColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey).ToString();

            if (themedColor == Constants.LightTheme) SetLightThemeColors();
            else SetDarkThemeColors();
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
            Settings.Default.ClassFeatureBlockColor = "#A9A9A9";
            Settings.Default.ControlBlockColor = "#A9A9A9";
            Settings.Default.DirectiveColor = "#8A9A5B";
            Settings.Default.OutputColor = "#7393B3";
            Settings.Default.InjectedColor = "#FF000000";
            Settings.Default.TagBackground = "#FFFAFAD2";
            Settings.Default.TagColor = "#FF000000";
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
