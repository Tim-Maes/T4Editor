using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using T4Editor.Common;
using T4Editor.Classifier;

namespace T4Editor.Controls
{
    public partial class ColorPickerControl : UserControl
    {
#pragma warning disable 649
        [Import]
        private TextViewColorizersManager _textViewsManager;

        public ColorPickerControl()
        {
            InitializeComponent();
            SetColorPickers();
            this.SatisfyImportsOnce();
        }
#pragma warning restore 649

        private void ClassFeatureBlockColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Settings.Default.ClassFeatureBlockColor = ClassFeatureBlockColorPicker.SelectedColor.ToString();
            Settings.Default.Save();
            BatchUpdateColors();
        }

        private void ControlBlockColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Settings.Default.ControlBlockColor = ControlBlockColorPicker.SelectedColor.ToString();
            Settings.Default.Save();
            BatchUpdateColors();
        }

        private void DirectiveColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Settings.Default.DirectiveColor = DirectiveColorPicker.SelectedColor.ToString();
            Settings.Default.Save();
            BatchUpdateColors();
        }

        private void OutputColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Settings.Default.OutputColor = OutputColorPicker.SelectedColor.ToString();
            Settings.Default.Save();
            BatchUpdateColors();
        }

        private void InjectedColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Settings.Default.InjectedColor = InjectedColorPicker.SelectedColor.ToString();
            Settings.Default.Save();
            BatchUpdateColors();
        }

        private void TagColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Settings.Default.TagColor = TagColorPicker.SelectedColor.ToString();
            Settings.Default.Save();
            BatchUpdateColors();
        }

        private void TagBackgroundColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Settings.Default.TagBackground = TagBackgroundColorPicker.SelectedColor.ToString();
            Settings.Default.Save();
            BatchUpdateColors();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BatchUpdateColors();
            var myWindow = Window.GetWindow(this);
            myWindow.Close();
        }

        public void BatchUpdateColors()
        {
            if(_textViewsManager == null) this.SatisfyImportsOnce();

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

        private void SetColorPickers()
        {
            ClassFeatureBlockColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(Settings.Default.ClassFeatureBlockColor);
            ControlBlockColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(Settings.Default.ControlBlockColor);
            DirectiveColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(Settings.Default.DirectiveColor);
            OutputColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(Settings.Default.OutputColor);
            InjectedColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(Settings.Default.InjectedColor);
            TagColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(Settings.Default.TagColor);
            TagBackgroundColorPicker.SelectedColor = (Color)ColorConverter.ConvertFromString(Settings.Default.TagBackground);
        }
    }
}
