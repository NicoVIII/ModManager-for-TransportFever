using System;
using TpfModManager;
using Xwt;

namespace TpfModManager.Gui {
	public class MainMenu : Menu {
		public MainMenu(ModManager manager, Window mainWindow) {
			MenuItem fileItem = new MenuItem("File");

			// Submenu "File"
			Menu fileMenu = new Menu();

			MenuItem checkItem = new MenuItem("Search for installed mods");
			checkItem.Clicked += (sender, e) => { manager.Check(); MessageDialog.ShowMessage(mainWindow, "Search complete."); };
			fileMenu.Items.Add(checkItem);

			fileItem.SubMenu = fileMenu;

			Items.Add(fileItem);
		}
	}
}
