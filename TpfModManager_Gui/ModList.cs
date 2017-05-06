using System;
using System.IO;
using Xwt;
using Xwt.Drawing;

namespace TpfModManager.Gui {
	public class ModList : VBox {
		ModManager modManager;

		ListView listView;
		ListStore store;
		Window mainWindow;

		DataField<Image> icon = new DataField<Image>();
		DataField<string> name = new DataField<string>();
		DataField<string> authors = new DataField<string>();
		DataField<string> version = new DataField<string>();
		DataField<string> updateAvailable = new DataField<string>();
		DataField<string> remoteVersion = new DataField<string>();
		DataField<string> tpfnetId = new DataField<string>();
		DataField<string> folder = new DataField<string>();

		public ModList(ModManager modManager, Window mainWindow) {
			this.modManager = modManager;
			this.mainWindow = mainWindow;

			store = new ListStore(icon, name, authors, tpfnetId, version, updateAvailable, remoteVersion, folder);

			initialiseListView();
		}

		void initialiseListView() {
			if (listView != null)
				Remove(listView);

			listView = new ListView();
			listView.GridLinesVisible = GridLines.Horizontal;

			listView.SelectionMode = SelectionMode.Multiple;
			listView.DataSource = store;
			listView.Columns.Add("", icon);

			// Name
			var nameColumn = new ListViewColumn(Resources.Localisation.List_Name, new TextCellView(name));
			nameColumn.CanResize = true;
			nameColumn.SortDataField = name;
			listView.Columns.Add(nameColumn);

			// Author(s)
			var authorsColumn = new ListViewColumn(Resources.Localisation.List_Authors, new TextCellView(authors));
			authorsColumn.CanResize = true;
			authorsColumn.SortDataField = authors;
			listView.Columns.Add(authorsColumn);

			// TpfnetId
			var tpfNetIdColumn = new ListViewColumn(Resources.Localisation.List_TpfNetId, new TextCellView(tpfnetId));
			tpfNetIdColumn.CanResize = true;
			tpfNetIdColumn.SortDataField = tpfnetId;
			listView.Columns.Add(tpfNetIdColumn);

			// Version
			var versionColumn = new ListViewColumn(Resources.Localisation.List_Version, new TextCellView(version));
			versionColumn.CanResize = true;
			versionColumn.SortDataField = version;
			listView.Columns.Add(versionColumn);

			// Update available
			var updateColumn = new ListViewColumn(Resources.Localisation.List_Update, new TextCellView(updateAvailable));
			updateColumn.CanResize = true;
			updateColumn.SortDataField = updateAvailable;
			listView.Columns.Add(updateColumn);

			// Remote version
			var remoteVersionColumn = new ListViewColumn(Resources.Localisation.List_RemoteVersion, new TextCellView(remoteVersion));
			remoteVersionColumn.CanResize = true;
			remoteVersionColumn.SortDataField = remoteVersion;
			listView.Columns.Add(remoteVersionColumn);

			// Remote version
			var folderColumn = new ListViewColumn(Resources.Localisation.List_Folder, new TextCellView(folder));
			folderColumn.CanResize = true;
			folderColumn.SortDataField = folder;
			listView.Columns.Add(folderColumn);

			PackStart(listView, true);

			// Add menu handler
			Menu contextMenu = new Menu();
			MenuItem openModUrlItem = new MenuItem(Resources.Localisation.List_ContextMenu_OpenUrl);
			contextMenu.Items.Add(openModUrlItem);
			MenuItem openModFolderItem = new MenuItem(Resources.Localisation.List_ContextMenu_OpenFolder);
			contextMenu.Items.Add(openModFolderItem);
			contextMenu.Items.Add(new SeparatorMenuItem());

			MenuItem changeTpfNetIdItem = new MenuItem(Resources.Localisation.List_ContextMenu_ChangeTpfNetId);
			contextMenu.Items.Add(changeTpfNetIdItem);
			contextMenu.Items.Add(new SeparatorMenuItem());

			MenuItem uninstallItem = new MenuItem(Resources.Localisation.List_ContextMenu_Uninstall);
			contextMenu.Items.Add(uninstallItem);

			EventHandler changeTpfNetIdHandler = null;
			EventHandler modUrlHandler = null;
			EventHandler modFolderHandler = null;
			EventHandler uninstallHandler = null;

			// Menu handling
			listView.ButtonPressed += delegate (object sender, ButtonEventArgs e) {
				int row = listView.GetRowAtPosition(new Point(e.X, e.Y));
				if (e.Button == PointerButton.Right && row >= 0) {
					// Set actual row to selected
					listView.SelectRow(row);
					// HACK fix position of popup
					contextMenu.Popup(listView, e.X + 20, e.Y + 40);

					// Open mod url
					if (store.GetValue(row, tpfnetId) != "" && int.Parse(store.GetValue(row, tpfnetId)) > 0) {
						openModUrlItem.Sensitive = true;

						// Remove previous handler
						removeClickedHandler(openModUrlItem, modUrlHandler);
						modUrlHandler = delegate {
							System.Diagnostics.Process.Start("https://transportfever.net/filebase/index.php/Entry/" + store.GetValue(row, tpfnetId));
						};
						openModUrlItem.Clicked += modUrlHandler;
					} else {
						openModUrlItem.Sensitive = false;
					}

					// Open folder
					removeClickedHandler(openModFolderItem, modFolderHandler);
					modFolderHandler = delegate {
						System.Diagnostics.Process.Start(Path.Combine(modManager.Settings.TpfModPath, store.GetValue(row, folder)));
					};
					openModFolderItem.Clicked += modFolderHandler;

					// Change tpfNetId
					removeClickedHandler(changeTpfNetIdItem, changeTpfNetIdHandler);
					changeTpfNetIdHandler = delegate {
						var dialog = new NumberInputDialog(Resources.Localisation.List_ChangeTpfNetId_Input, store.GetValue(row, tpfnetId));
						dialog.Title = store.GetValue(row, folder);
						if (dialog.Run(mainWindow) == Command.Ok) {
							modManager.ChangeTpfNetId(dialog.Number, store.GetValue(row, folder));
							Update();
						}
					};
					changeTpfNetIdItem.Clicked += changeTpfNetIdHandler;

					// Uninstall
					removeClickedHandler(uninstallItem, uninstallHandler);
					uninstallHandler = delegate {
						if (MessageDialog.Confirm(Resources.Localisation.Uninstall_Confirm, Command.Yes, false)) {
							modManager.Uninstall(store.GetValue(row, folder));
							Update();
							MessageDialog.ShowMessage(Resources.Localisation.Uninstall_Complete);
						}
					};
					uninstallItem.Clicked += uninstallHandler;
				}
			};
		}

		void removeClickedHandler(MenuItem item, EventHandler handler) {
			if (handler != null)
				item.Clicked -= handler;
		}

		void GenerateModImagePng() {
			try {
				// Generate png if necessary
				DevILSharp.IL.Init();
				foreach (Mod m in modManager.ModList) {
					if (m.Image != "") {
						var imagePath = Path.Combine(modManager.Settings.TpfModPath, m.Folder, m.Image);
						var pngPath = Path.ChangeExtension(imagePath, "png");

						if (!File.Exists(pngPath)) {
							var image = DevILSharp.Image.Load(imagePath);
							image.Save(pngPath, DevILSharp.ImageType.Png);
						}
					}
				}
			}
			catch (FileNotFoundException) {
				// Nothing to do
			}
			// HACK macOS: library is available, but not found :(
			catch (DllNotFoundException) {
				// Nothing to do
			}
		}

		public void Update() {
			GenerateModImagePng();

			// Load remote versions
			modManager.LookUpRemoteVersions();

			store.Clear();
			for (int i = 0; i < modManager.ModList.Length; i++) {
				var r = store.AddRow();
				var m = modManager.ModList[i];

				// Image
				if (m.Image != "") {
					var imagePath = Path.Combine(modManager.Settings.TpfModPath, m.Folder, m.Image);
					var pngPath = Path.ChangeExtension(imagePath, "png");
					if (File.Exists(pngPath)) {
						store.SetValue(r, icon, Image.FromFile(pngPath).WithBoxSize(80));
					}
				}

				// Name
				store.SetValue(r, name, m.Name);

				// Authors
				var authorNameArray = new string[m.Authors.Length];
				for (int j = 0; j < m.Authors.Length; j++) {
					authorNameArray[j] = m.Authors[j].Name;
				}
				store.SetValue(r, authors, string.Join(", ", authorNameArray));

				// Version
				store.SetValue(r, version, m.Version.Major + "." + m.Version.Minor);

				// Remote version
				string remoteVersionString;
				if (m.RemoteVersion == null)
					remoteVersionString = "-";
				else
					remoteVersionString = m.RemoteVersion.Major + "." + m.RemoteVersion.Minor;
				
				store.SetValue(r, remoteVersion, remoteVersionString);

				// Update available
				string updateAvailableString;
				if (m.RemoteVersion != null && (m.RemoteVersion.Major > m.Version.Major || m.RemoteVersion.Major == m.Version.Major && m.RemoteVersion.Minor > m.Version.Minor)) {
					updateAvailableString = Resources.Localisation.List_Update_Yes;
				} else {
					updateAvailableString = "";
				}
				store.SetValue(r, updateAvailable, updateAvailableString);

				// TpfNetId
				if (m.TpfNetId > 0)
					store.SetValue(r, tpfnetId, m.TpfNetId.ToString());
				else
					store.SetValue(r, tpfnetId, "");

				// Folder
				Console.WriteLine("+ " + m.Folder);
				store.SetValue(r, folder, m.Folder);
			}
		}
	}
}