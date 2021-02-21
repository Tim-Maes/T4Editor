using System;
using System.ComponentModel.Design;
using T4Editor.Controls;
using Shell = Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using Window = System.Windows;

namespace T4Editor.Commands
{
    internal sealed class OpenT4EditorSettingsCommand
    {
        public const int CommandId = 0x0100;

        public static readonly Guid CommandSet = new Guid(Common.Constants.CommandSetGuid);

        private readonly Shell.AsyncPackage package;

        private OpenT4EditorSettingsCommand(Shell.AsyncPackage package, Shell.OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static OpenT4EditorSettingsCommand Instance
        {
            get;
            private set;
        }

        public static async Task InitializeAsync(Shell.AsyncPackage package)
        {
            await Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            Shell.OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as Shell.OleMenuCommandService;
            Instance = new OpenT4EditorSettingsCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            var window = new Window.Window
            {
                Width = 350,
                Height = 180,
                Content = new ColorPickerControl()
            };

            window.ShowDialog();
        }
    }
}
