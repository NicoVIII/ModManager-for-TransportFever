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
				modList.Update();
				MessageDialog.ShowMessage(mainWindow, "Search complete.");
			};
			fileMenu.Items.Add(checkItem);

			// Submenu "File/Install"
			MenuItem installItem = new MenuItem("Install");
			Menu installMenu = new Menu();
			installItem.SubMenu = installMenu;

			MenuItem installFromFileItem = new MenuItem("From file...");
			installFromFileItem.Clicked += (sender, e) => {
				var dialog = new OpenFileDialog("Choose mod archive");
				dialog.Run(mainWindow);
				var fileName = dialog.FileName;
				if (fileName != null) {
					switch (manager.Install(fileName)) {
						case InstallationResult.Success:
							modList.Update();
							MessageDialog.ShowMessage("Installation complete.");
							break;
						case InstallationResult.AlreadyInstalled:
							MessageDialog.ShowMessage("Mod is already installed.");
							break;
						case InstallationResult.ModInvalid:
							MessageDialog.ShowMessage("Something is wrong with this mod :(");
							break;
						case InstallationResult.NotSupported:
							MessageDialog.ShowMessage("Sadly this mod is not supported yet.");
							break;
					}
				}
			};
			installMenu.Items.Add(installFromFileItem);

			MenuItem installFromUrlItem = new MenuItem("From url...");
			installFromUrlItem.Sensitive = false;
			installMenu.Items.Add(installFromUrlItem);

			fileMenu.Items.Add(installItem);
			fileMenu.Items.Add(new SeparatorMenuItem());

			MenuItem exitItem = new MenuItem("Exit");
			exitItem.Clicked += (sender, e) => {
				Application.Exit();
			};
			fileMenu.Items.Add(exitItem);

			fileItem.SubMenu = fileMenu;

			Items.Add(fileItem);
		}
	}
}