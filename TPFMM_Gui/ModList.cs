using System;
using System.IO;
using Xwt;
using Xwt.Drawing;

namespace TpfModManager.Gui {
	public class ModList : ScrollView {
		ModManager modManager;
		Table table = new Table();

		public ModList(ModManager modManager) {
			this.modManager = modManager;
			this.Content = table;

			AddHeader();
			Update();
		}

		private void AddHeader() {
			table.Add(new Label(""), 0, 0);
			table.Add(new Label("Name"), 1, 0, hexpand: true);
			table.Add(new Label("Version"), 2, 0);
			table.Add(new Label("Remote Version"), 3, 0);
		}

		public void Update() {
			var list = modManager.ModList;
			for (int i = 0; i < list.Length; i++) {
				var m = list[i];
				if (m.Image != "") {
					var imagePath = Path.Combine(modManager.Settings.TpfModPath, m.Folder, m.Image);
					//table.Add(new ImageView(Image.FromFile(imagePath).WithSize(20,20)), i, 0);
				}
				table.Add(new Label(m.Name), 1, i + 1);
				table.Add(new Label(m.Version.Major+"."+m.Version.Minor), 2, i+1);
				table.Add(new Label("-"), 3, i+1);
			}
		}
	}
}