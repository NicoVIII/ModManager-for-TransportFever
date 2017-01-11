using System;
using System.IO;
using Xwt;
using Xwt.Drawing;

namespace TpfModManager.Gui {
	public class ModList : ListView {
		ModManager modManager;
		ListStore store;
		DataField<Image> icon = new DataField<Image>();
		DataField<string> name = new DataField<string>();
		DataField<string> version = new DataField<string>();
		DataField<string> remoteVersion = new DataField<string>();

		public ModList(ModManager modManager) {
			this.modManager = modManager;
			store = new ListStore(/*icon, */name, version, remoteVersion);

			SelectionMode = SelectionMode.Multiple;
			DataSource = store;
			//Columns.Add("", icon);
			Columns.Add("Name", name);
			Columns.Add("Version", version);
			Columns.Add("RemoteVersion", remoteVersion);
			Update();
		}

		public void Update() {
			for (int i = 0; i < modManager.ModList.Length; i++) {
				var r = store.AddRow();
				var m = modManager.ModList[i];
				if (m.Image != "") {
					//store.SetValue(r, icon, Image.FromFile(Path.Combine(modManager.Settings.TpfModPath, m.Folder, m.Image)));
				}
				store.SetValue(r, name, m.Name);
				store.SetValue(r, version, m.Version.Major + "." + m.Version.Minor);
				store.SetValue(r, remoteVersion, "-");
			}
		}
	}
}