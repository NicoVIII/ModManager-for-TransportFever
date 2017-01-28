using System;
using System.IO;
using Xwt;
using Xwt.Drawing;

namespace TpfModManager.Gui {
	public class ModList : VBox {
		ModManager modManager;

		ListView listView;
		ListStore store;

		DataField<Image> icon = new DataField<Image>();
		DataField<string> name = new DataField<string>();
		DataField<string> authors = new DataField<string>();
		DataField<string> version = new DataField<string>();
		DataField<string> updateAvailable = new DataField<string>();
		DataField<string> remoteVersion = new DataField<string>();
		DataField<int> tpfNetId = new DataField<int>();

		public ModList(ModManager modManager) {
			this.modManager = modManager;

			listView = new ListView();
			listView.GridLinesVisible = GridLines.Horizontal;
			store = new ListStore(icon, name, authors, version, updateAvailable, remoteVersion, tpfNetId);

			listView.SelectionMode = SelectionMode.Multiple;
			listView.DataSource = store;
			listView.Columns.Add("", icon);

			// Name
			var nameColumn = new ListViewColumn("Name", new TextCellView(name));
			nameColumn.CanResize = true;
			nameColumn.SortDataField = name;
			listView.Columns.Add(nameColumn);

			// Author(s)
			var authorsColumn = new ListViewColumn("Author(s)", new TextCellView(authors));
			authorsColumn.CanResize = true;
			authorsColumn.SortDataField = authors;
			listView.Columns.Add(authorsColumn);

			// Version
			var versionColumn = new ListViewColumn("Version", new TextCellView(version));
			versionColumn.CanResize = true;
			versionColumn.SortDataField = version;
			listView.Columns.Add(versionColumn);

			// Update available
			var updateColumn = new ListViewColumn("Update\navailable", new TextCellView(updateAvailable));
			updateColumn.CanResize = true;
			updateColumn.SortDataField = updateAvailable;
			listView.Columns.Add(updateColumn);

			// Remote version
			var remoteVersionColumn = new ListViewColumn("RemoteVersion", new TextCellView(remoteVersion));
			remoteVersionColumn.CanResize = true;
			remoteVersionColumn.SortDataField = remoteVersion;
			listView.Columns.Add(remoteVersionColumn);

			PackStart(listView, true);

			// Add menu handler
			Menu contextMenu = new Menu();
			MenuItem openModUrlItem = new MenuItem("Open mod url");
			contextMenu.Items.Add(openModUrlItem);
			EventHandler lastHandler = null;
			listView.ButtonPressed += delegate (object sender, ButtonEventArgs e) {
				int row = listView.GetRowAtPosition(new Point(e.X, e.Y));
				if (e.Button == PointerButton.Right && row >= 0) {
					// Set actual row to selected
					listView.SelectRow(row);
					contextMenu.Popup(listView, e.X, e.Y);
					if (store.GetValue(row, tpfNetId) > 0) {
						openModUrlItem.Sensitive = true;
						// Remove previous handler
						if (lastHandler != null) {
							openModUrlItem.Clicked -= lastHandler;
						}
						lastHandler = delegate {
							System.Diagnostics.Process.Start("https://transportfever.net/filebase/index.php/Entry/"+store.GetValue(row, tpfNetId));
						};
						openModUrlItem.Clicked += lastHandler;
					} else {
						openModUrlItem.Sensitive = false;
						// Remove previous handler
						if (lastHandler != null) {
							openModUrlItem.Clicked -= lastHandler;
							lastHandler = null;
						}
					}
				}
			};
		}

		private void GenerateModImagePng() {
			try {
				// Generate png if necessary
				DevILSharp.IL.Init();
				// TODO remove png on update
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
			// HACK library is available, but not found :(
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
					updateAvailableString = "X";
				} else {
					updateAvailableString = "";
				}
				store.SetValue(r, updateAvailable, updateAvailableString);

				// TpfNetId (invisible)
				store.SetValue(r, tpfNetId, m.TpfNetId);
			}
		}
	}
}