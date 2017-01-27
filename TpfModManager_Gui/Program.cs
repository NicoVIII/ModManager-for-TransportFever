using System;
using System.IO;
using TpfModManager;
using Xwt;
using Xwt.Drawing;

namespace TpfModManager.Gui {
	class Program {
		static ModManager modManager = new ModManager();
		static ModList modList;

		[STAThread]
		static void Main(string[] args) {
			string title = "TPF-ModManager v0.1.0-alpha.6";
			// Init Gui
			PlatformID id = Environment.OSVersion.Platform;
			switch (id) {
				// TODO Use WPF if XWT finally fixed all the issues with ListView and WPF
				/*case PlatformID.Win32NT:
					Application.Initialize(ToolkitType.Wpf);
					break;//*/
				case PlatformID.MacOSX:
				case PlatformID.Unix:
					try {
						Application.Initialize(ToolkitType.Cocoa);
					} catch {
						Application.Initialize(ToolkitType.Gtk);
					}
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
			modList = new ModList(modManager);
			mainWindow.MainMenu = new MainMenu(modManager, mainWindow, modList);

			// Set up Tpf mods Path
			if (modManager.Settings == null || modManager.Settings.TpfModPath == "") {
				var folderDialog = new SelectFolderDialog("Select TPF mods folder");
				folderDialog.Run(mainWindow);
				var folder = folderDialog.Folder;
				// TODO look into this
				if (folder != null && folder.EndsWith("mods")) {
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
				MessageDialog.ShowError(mainWindow, "Please set the path to Transport Fever's 'mods' folder!");
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