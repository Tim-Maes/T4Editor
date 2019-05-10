using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Windows;
using T4Editor.Controls;
using Task = System.Threading.Tasks.Task;

namespace T4Editor.Commands
{
    internal sealed class OpenT4EditorSettingsCommand
    {
        public const int CommandId = 0x0100;

        public static readonly Guid CommandSet = new Guid("bfb75b6d-fe6d-4894-9cde-a1714bc797c2");

        private readonly AsyncPackage package;

        private OpenT4EditorSettingsCommand(AsyncPackage package, OleMenuCommandService commandService)
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

        private IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new OpenT4EditorSettingsCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            var window = new Window
            {
                Width = 350,
                Height = 180,
                Content = new ColorPickerControl()
            };

            window.ShowDialog();
            MessageBox.Show("Restart Visual Studio to apply changes.");
        }
    }
}
