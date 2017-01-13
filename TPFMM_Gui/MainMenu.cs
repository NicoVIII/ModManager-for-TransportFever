using System;
using TpfModManager;
using Xwt;

namespace TpfModManager.Gui {
	public class MainMenu : Menu {
		public MainMenu(ModManager manager, Window mainWindow, ModList modList) {
			MenuItem fileItem = new MenuItem("File");

			// Submenu "File"
			Menu fileMenu = new Menu();

			MenuItem checkItem = new MenuItem("Search for installed mods");
			checkItem.Clicked += (sender, e) => { manager.Check(); modList.Update(); MessageDialog.ShowMessage(mainWindow, "Search complete."); };
			fileMenu.Items.Add(checkItem);
			// TODO for test purposes add "is mod installed" <----- NEXT TIME

			// TODO add Exit

			fileItem.SubMenu = fileMenu;

			Items.Add(fileItem);
		}
	}
}
