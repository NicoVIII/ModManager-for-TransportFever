using System;
using System.IO;
using System.Globalization;
using System.Resources;
using System.Threading;
using Xwt;
using Xwt.Drawing;

namespace TpfModManager.Gui {
	class Program {
		static ModManager modManager;
		static ModList modList;

		[STAThread]
		static void Main(string[] args) {
			// TODO use of console
			Console.WriteLine("In this alpha version this console is shown for better debugging. In a release version this console will be gone!");

			string title = "TPF-ModManager v0.1.0-alpha.7";
			// Init Gui
			PlatformID id = Environment.OSVersion.Platform;
			switch (id) {
				// TODO Use WPF if XWT finally fixed all the issues with ListView and WPF
				/*case PlatformID.Win32NT:
					Application.Initialize(ToolkitType.Wpf);
					break;//*/
				case PlatformID.MacOSX:
				case PlatformID.Unix:
					/*try {
						Application.Initialize(ToolkitType.Cocoa);
					} catch {//*/
						Application.Initialize(ToolkitType.Gtk);
					//}
				break;
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
			mainWindow.Show();

			// Initialise components
			modManager = new ModManager();
			modList = new ModList(modManager, mainWindow);

			// Initialise menu
			mainWindow.MainMenu = new MainMenu(modManager, mainWindow, modList);

			// Set up Tpf mods Path
			if (modManager.Settings == null || modManager.Settings.TpfModPath == "") {
				var folderDialog = new SelectFolderDialog(Resources.Localisation.Setup_ChooseModFolder);
				folderDialog.Run(mainWindow);
				var folder = folderDialog.Folder;
				// TODO look into this
				if (folder != null && Path.GetFileName(folder) == "mods") {
					// Update TpfModPath
					Settings settings = new Settings();
					settings.TpfModPath = folderDialog.Folder;
					settings.Save();
					modManager.Settings = settings;
					modManager.Check();
				}
			}

			// Load UI
			if (modManager.Settings == null || modManager.Settings.TpfModPath == "") {
				MessageDialog.ShowError(mainWindow, Resources.Localisation.Setup_PleaseSetPath);
			} else {
				modList.Update();

				// Init basic layout
				Box container = new VBox();
				container.Margin = new WidgetSpacing(5, 5, 5, 5);
				container.PackStart(modList, true);
				mainWindow.Content = container;

				// Start application
				Application.Run();
			}
			mainWindow.Dispose();
		}
	}
}