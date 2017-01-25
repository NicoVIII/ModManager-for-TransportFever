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
			listView.Columns.Add("Name", name);
			listView.Columns.Add("Author(s)", authors);
			listView.Columns.Add("Version", version);
			listView.Columns.Add("RemoteVersion", remoteVersion);

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

			store.Clear();
			for (int i = 0; i < modManager.ModList.Length; i++) {
				var r = store.AddRow();
				var m = modManager.ModList[i];
				if (m.Image != "") {
					var imagePath = Path.Combine(modManager.Settings.TpfModPath, m.Folder, m.Image);
					var pngPath = Path.ChangeExtension(imagePath, "png");
					store.SetValue(r, icon, Image.FromFile(pngPath).WithBoxSize(80));
				}
				store.SetValue(r, name, m.Name);
				store.SetValue(r, authors, string.Join(", ", m.Authors));
				store.SetValue(r, version, m.Version.Major + "." + m.Version.Minor);
				store.SetValue(r, remoteVersion, "-");
			}
		}
	}
}