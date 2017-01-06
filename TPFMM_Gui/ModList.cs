using System;
using Xwt;

namespace TpfModManager.Gui {
	public class ModList : Table {
		public ModList() {
			AddHeader();
		}

		private void AddHeader() {
			Add(new Label(""), 0, 0);
			Add(new Label("Name"), 1, 0, hexpand: true);
			Add(new Label("Version"), 2, 0);
			Add(new Label("Remote Version"), 3, 0);
		}
	}
}