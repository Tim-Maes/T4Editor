using Microsoft.VisualStudio.PlatformUI;

namespace T4Editor.Controls
{
    public class FirstInstallDialog : DialogWindow
    {
        public FirstInstallControl DialogControl { get; }

        public FirstInstallDialog()
        {
            this.DialogControl = new FirstInstallControl();
            this.Content = DialogControl;
            this.Height = 170;
            this.Width = 280;
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

        }

        public string ShowAndRetrieveSelection()
        {
            if (this.ShowDialog() == true)
            {
                return DialogControl.SelectedValue;
            }
            return null;
        }
    }
}
