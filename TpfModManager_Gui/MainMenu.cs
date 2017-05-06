using System;
using System.IO;
using Xwt;

namespace TpfModManager.Gui {
	public class MainMenu : Menu {
		ConfirmDelegate upgradeCallback = (folder) => MessageDialog.Confirm(string.Format(Resources.Localisation.Install_UpgradeConfirm, folder), Command.Yes);

		public MainMenu(ModManager manager, Window mainWindow, ModList modList) {
			MenuItem fileItem = new MenuItem(Resources.Localisation.Menu_File);

			// Submenu "File"
			Menu fileMenu = new Menu();

			MenuItem checkItem = new MenuItem(Resources.Localisation.Menu_Search);
			checkItem.Clicked += (sender, e) => {
				manager.Check();
				modList.Update();
				MessageDialog.ShowMessage(mainWindow, Resources.Localisation.Search_Complete);
			};
			fileMenu.Items.Add(checkItem);

			// Submenu "File/Install"
			MenuItem installItem = new MenuItem(Resources.Localisation.Menu_Install);
			Menu installMenu = new Menu();
			installItem.SubMenu = installMenu;

			MenuItem installFromFolderItem = new MenuItem(Resources.Localisation.Menu_Install_FromFolder);
			installFromFolderItem.Clicked += (sender, e) => {
				var dialog = new SelectFolderDialog(Resources.Localisation.Install_ChooseFolder);
				dialog.Run(mainWindow);
				var folderName = dialog.Folder;
				if (folderName != null && folderName != "") {
					foreach (var file in Directory.GetFiles(folderName)) {
						// TODO write function for this
						// TODO move messages to the end!
						switch (manager.Install(Path.Combine(folderName, file), upgradeCallback)) {
							case InstallationResult.Success:
								modList.Update();
								//MessageDialog.ShowMessage(file + ":\n" + Resources.Localisation.Install_Complete);
								break;
							case InstallationResult.AlreadyInstalled:
								MessageDialog.ShowMessage(file + ":\n" + Resources.Localisation.Install_AlreadyInstalled);
								break;
							case InstallationResult.ModInvalid:
								MessageDialog.ShowMessage(file + ":\n" + Resources.Localisation.Install_ModInvalid);
								break;
							case InstallationResult.NotSupported:
								MessageDialog.ShowMessage(file + "\n" + Resources.Localisation.Install_NotSupported);
								break;
							case InstallationResult.NotAnArchive:
								// Nothing to do
								break;
						}
					}
					MessageDialog.ShowMessage(Resources.Localisation.Install_BulkComplete);
				}
			};
			installMenu.Items.Add(installFromFolderItem);

			MenuItem installFromFileItem = new MenuItem(Resources.Localisation.Menu_Install_FromFile);
			installFromFileItem.Clicked += (sender, e) => {
				var dialog = new OpenFileDialog(Resources.Localisation.Install_ChooseArchive);
				dialog.Run(mainWindow);
				var fileName = dialog.FileName;
				if (fileName != null) {
					switch (manager.Install(fileName, upgradeCallback)) {
						case InstallationResult.Success:
							modList.Update();
							MessageDialog.ShowMessage(Resources.Localisation.Install_Complete);
							break;
						case InstallationResult.Upgrade:
							modList.Update();
							MessageDialog.ShowMessage(Resources.Localisation.Install_UpgradeComplete);
							break;
						case InstallationResult.AlreadyInstalled:
							MessageDialog.ShowMessage(Resources.Localisation.Install_AlreadyInstalled);
							break;
						case InstallationResult.ModInvalid:
							MessageDialog.ShowMessage(Resources.Localisation.Install_ModInvalid);
							break;
						case InstallationResult.NotSupported:
							MessageDialog.ShowMessage(Resources.Localisation.Install_NotSupported);
							break;
						case InstallationResult.NotAnArchive:
							MessageDialog.ShowMessage(Resources.Localisation.Install_NotAnArchive);
							break;
					}
				}
			};
			installMenu.Items.Add(installFromFileItem);

			fileMenu.Items.Add(installItem);
			fileMenu.Items.Add(new SeparatorMenuItem());

			MenuItem exitItem = new MenuItem(Resources.Localisation.Menu_Exit);
			exitItem.Clicked += (sender, e) => {
				Application.Exit();
			};
			fileMenu.Items.Add(exitItem);

			fileItem.SubMenu = fileMenu;

			Items.Add(fileItem);
		}
	}
}