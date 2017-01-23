using System;
using System.IO;
using TpfModManager;
using Xwt;

namespace TpfModManager.Gui {
	public class MainMenu : Menu {
		public MainMenu(ModManager manager, Window mainWindow, ModList modList) {
			MenuItem fileItem = new MenuItem("File");

			// Submenu "File"
			Menu fileMenu = new Menu();

			MenuItem checkItem = new MenuItem("Search for installed mods");
			checkItem.Clicked += (sender, e) => {
				manager.Check();
				modList.GenerateModImagePng();
				modList.Update();
				MessageDialog.ShowMessage(mainWindow, "Search complete.");
			};
			fileMenu.Items.Add(checkItem);

			// TODO for test purposes add "is mod installed"
			MenuItem testItem = new MenuItem("Is mod already installed?");
			testItem.Clicked += (sender, e) => {
				var dialog = new OpenFileDialog("Choose mod archive");
				dialog.Run(mainWindow);
				MessageDialog.ShowMessage(manager.IsModInstalled(dialog.FileName).ToString());
			};
			fileMenu.Items.Add(testItem);
			// TODO add Exit

			fileItem.SubMenu = fileMenu;

			Items.Add(fileItem);
		}
	}
}
