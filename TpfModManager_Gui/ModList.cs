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
		DataField<string> remoteVersion = new DataField<string>();

		public ModList(ModManager modManager) {
			this.modManager = modManager;

			listView = new ListView();
			store = new ListStore(icon, name, authors, version, remoteVersion);

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

			// Remote version
			var remoteVersionColumn = new ListViewColumn("RemoteVersion", new TextCellView(remoteVersion));
			remoteVersionColumn.CanResize = true;
			remoteVersionColumn.SortDataField = remoteVersion;
			listView.Columns.Add(remoteVersionColumn);

			PackStart(listView, true);
		}

		private void GenerateModImagePng() {
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
					store.SetValue(r, icon, Image.FromFile(pngPath).WithBoxSize(80));
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
			}
		}
	}
}