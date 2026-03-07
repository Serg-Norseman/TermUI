//
// ColorDialog.cs: Dialog for color select
//
// Authors:
//   Serg V. Zhdanovskikh
//

namespace Terminal.Gui {
	public class ColorDialog : Dialog {
		Button prompt, cancel;
		ColorPicker colorPicker;


		internal bool canceled;


		/// <summary>
		/// Check if the dialog was or not canceled.
		/// </summary>
		public bool Canceled { get => canceled; }

		public Color Color {
			get => colorPicker.SelectedColor;
			set => colorPicker.SelectedColor = value;
		}

		public ColorDialog ()
		{
			this.colorPicker = new ColorPicker () {
				X = 1,
				Y = 1,
			};
			Add (this.colorPicker);

			Width = colorPicker.Width + 4;
			Height = colorPicker.Height + 7;

			this.prompt = new Button ("Ok") {
				IsDefault = true,
				Enabled = true
			};
			this.prompt.Clicked += (sender, e) => {
				canceled = false;
				Application.RequestStop ();
			};
			AddButton (this.prompt);

			this.cancel = new Button ("Cancel");
			this.cancel.Clicked += (sender, e) => {
				Cancel ();
			};
			AddButton (cancel);

			// On success, we will set this to false.
			canceled = true;

			KeyPress += (s, e) => {
				if (e.KeyEvent.Key == Key.Esc) {
					Cancel ();
					e.Handled = true;
				}
			};
			void Cancel ()
			{
				canceled = true;
				Application.RequestStop ();
			}
		}

		///<inheritdoc/>
		public override void WillPresent ()
		{
			base.WillPresent ();
			colorPicker.SetFocus ();
		}

		public Color Show(Color defaultValue)
		{
			Color = defaultValue;
			Application.Run(this);
			return (!Canceled) ? this.Color : defaultValue;
		}
	}
}
