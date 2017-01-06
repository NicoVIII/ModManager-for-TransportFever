using System;
using Xwt;

namespace TpfModManager.Gui {
	public class MainMenu : Menu {
		public MainMenu() {
			MenuItem fileItem = new MenuItem("File");

			// Submenu "File"
			Menu fileMenu = new Menu();
			MenuItem exampleItem = new MenuItem("Example");
			fileMenu.Items.Add(exampleItem);
			fileItem.SubMenu = fileMenu;

			Items.Add(fileItem);
		}
	}
}
