using System;
using Xwt;

namespace TpfModManager {
	public class NumberInputDialog : Dialog {
		VBox contentContainer = new VBox();
		TextEntry input = new TextEntry();

		public int Number { get; private set; }

		public NumberInputDialog(string title, string input) : this(title, input == "" ? 0 : int.Parse(input)) {}

		public NumberInputDialog(string title, int start) {
			Content = contentContainer;
			Number = start;

			contentContainer.PackStart(new Label(title));

			input.Text = Number > 0 ? Number.ToString() : "";
			input.Changed += delegate {
				int i;
				if (input.Text == "" || int.TryParse(input.Text, out i)) {
					// Nothing to do
				} else {
					input.Text = Number.ToString();
				}
			};
			input.Activated += Ok;
			contentContainer.PackStart(input);

			DialogButton ok = new DialogButton(Command.Ok);
			DialogButton cancel = new DialogButton(Command.Cancel);

			ok.Clicked += Ok;
			cancel.Clicked += delegate {
				Dispose();
			};

			Buttons.Add(ok);
			Buttons.Add(cancel);
			DefaultCommand = Command.Ok;
		}

		private void Ok(object sender, EventArgs args) {
			int i;
			if (int.TryParse(input.Text, out i)) {
				Number = i;
			}
			Dispose();
		}
	}
}