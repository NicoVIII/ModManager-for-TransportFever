using System;
using Xwt;
using Xwt.Drawing;

namespace TPFMM_Gui {
	class Program {
		[STAThread]
		static void Main(string[] args) {
			string title = "TPF-ModManager v0.1-alpha.5";
			// Init Gui
			PlatformID id = Environment.OSVersion.Platform;
			switch (id) {
				case PlatformID.Win32NT:
					Application.Initialize(ToolkitType.Wpf);
					break;
				//case PlatformID.MacOSX:
				//case PlatformID.Unix:
				default:
					Application.Initialize(ToolkitType.Gtk);
					break;
			}
			var mainWindow = new Window() {
				Title = title,

				Decorated = true,
				Width = 800,
				Height = 600
			};

			Box container = new VBox();
			container.Margin = new WidgetSpacing(5, 5, 5, 5);
			container.BackgroundColor = Colors.Black;
			mainWindow.Content = container;

			mainWindow.MainMenu = new MainMenu();

			// Start Application
			mainWindow.Show();
			Application.Run();
			mainWindow.Dispose();
		}
	}
}
