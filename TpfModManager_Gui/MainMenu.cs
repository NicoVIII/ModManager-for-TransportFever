using System;
using System.IO;
using Xwt;

namespace TpfModManager.Gui {
	public class MainMenu : Menu {
		ConfirmDelegate upgradeCallback = (folder) => MessageDialog.Confirm("Do you really want to upgrade the mod in the folder '" + folder + "'?", Command.Yes);

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

			MenuItem installFromFolderItem = new MenuItem("From folder...");
			installFromFolderItem.Clicked += (sender, e) => {
				var dialog = new SelectFolderDialog("Choose folder with mod archives");
				dialog.Run(mainWindow);
				var folderName = dialog.Folder;
				if (folderName != null && folderName != "") {
					foreach (var file in Directory.GetFiles(folderName)) {
						// TODO write function for this
						// TODO move messages to the end!
						switch (manager.Install(Path.Combine(folderName, file), upgradeCallback)) {
							case InstallationResult.Success:
								modList.Update();
								//MessageDialog.ShowMessage(file + ":\nInstallation complete.");
								break;
							case InstallationResult.AlreadyInstalled:
								MessageDialog.ShowMessage(file + ":\nMod is already installed.");
								break;
							case InstallationResult.ModInvalid:
								MessageDialog.ShowMessage(file + ":\nSomething is wrong with this mod :(");
								break;
							case InstallationResult.NotSupported:
								MessageDialog.ShowMessage(file + "\nSadly this mod is not supported yet.");
								break;
							case InstallationResult.NotAnArchive:
								// Nothing to do
								break;
						}
					}
					MessageDialog.ShowMessage("Bulk installation complete.");
				}
			};
			installMenu.Items.Add(installFromFolderItem);

			MenuItem installFromFileItem = new MenuItem("From file...");
			installFromFileItem.Clicked += (sender, e) => {
				var dialog = new OpenFileDialog("Choose mod archive");
				dialog.Run(mainWindow);
				var fileName = dialog.FileName;
				if (fileName != null) {
					switch (manager.Install(fileName, upgradeCallback)) {
						case InstallationResult.Success:
							modList.Update();
							MessageDialog.ShowMessage("Installation complete.");
							break;
						case InstallationResult.Upgrade:
							modList.Update();
							MessageDialog.ShowMessage("Mod was successfully upgraded.");
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
						case InstallationResult.NotAnArchive:
							MessageDialog.ShowMessage("This file is not a valid archive.");
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