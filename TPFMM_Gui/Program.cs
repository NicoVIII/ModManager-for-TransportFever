using System;
using TpfModManager;
using Xwt;
using Xwt.Drawing;

namespace TpfModManager.Gui {
	class Program {
		static ModManager modManager = new ModManager();

		[STAThread]
		static void Main(string[] args) {
			string title = "TPF-ModManager v0.1-alpha.5";
			// Init Gui
			PlatformID id = Environment.OSVersion.Platform;
			switch (id) {
				case PlatformID.Win32NT:
					Application.Initialize(ToolkitType.Wpf);
					break;
				//case PlatformID.MacOSX:
				//case PlatformID.Unix:
				default:
					Application.Initialize(ToolkitType.Gtk);
					break;
			}
			var mainWindow = new Window() {
				Title = title,

				Decorated = true,
				Width = 800,
				Height = 600
			};

			// Set up Tpf mods Path
			if (modManager.Settings == null) {
				var folderDialog = new SelectFolderDialog("Select TPF mods folder");
				folderDialog.Run();
				Settings settings = new Settings();
				settings.TpfModPath = folderDialog.Folder;
				settings.Save();
				modManager.Settings = settings;
			}

			// Init basic layout
			Box container = new VBox();
			container.Margin = new WidgetSpacing(5, 5, 5, 5);
			container.PackStart(new ModList(), true);
			mainWindow.Content = container;

			mainWindow.MainMenu = new MainMenu();

			// Start Application
			mainWindow.Show();
			Application.Run();
			mainWindow.Dispose();
		}
	}
}
