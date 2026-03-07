//
// NumericStepper.cs: NumericStepper control
//
// Authors:
//   Serg V. Zhdanovskikh
//

using System;

namespace Terminal.Gui {
	public class NumericStepper : View {
		private readonly TextField _textField;
		private int _value;

		public int Maximum { get; set; } = int.MaxValue;

		public int Minimum { get; set; } = int.MinValue;

		public int Step { get; set; } = 1;

		public int Value {
			get { return _value; }
			set {
				_value = Math.Max (Minimum, Math.Min (Maximum, value));
				_textField.Text = _value.ToString ();
				ValueChanged?.Invoke (this, EventArgs.Empty);
			}
		}

		/// <summary>
		///If set to true its not allow any changes in the text.
		/// </summary>
		public bool ReadOnly {
			get => _textField.ReadOnly;
			set {
				_textField.ReadOnly = value;
				if (_textField.ReadOnly) {
					if (_textField.ColorScheme != null) {
						_textField.ColorScheme.Normal = _textField.ColorScheme.Focus;
					}
				}
			}
		}


		public event EventHandler ValueChanged;


		public NumericStepper () : base ()
		{
			Height = 1;
			//CanFocus = true;

			_textField = new TextField () {
				TextAlignment = TextAlignment.Right
			};
			_textField.Leave += (s, e) => {
				if (int.TryParse (_textField.Text.ToString (), out var v)) {
					Value = v;
				} else {
					_textField.Text = Value.ToString ();
				}
			};
			Add (_textField);

			// On resize
			LayoutComplete += (object s, LayoutEventArgs a) => {
				var bounds = Bounds;
				if (bounds.Width > 0) {
					_textField.X = 0;
					_textField.Y = 0;
					_textField.Width = bounds.Width - 2;
					//_textField.SetRelativeLayout (Bounds);
				}
			};
		}

		///<inheritdoc/>
		public override void Redraw (Rect bounds)
		{
			base.Redraw (bounds);

			Driver.SetAttribute (GetNormalColor ());
			//Driver.SetAttribute (ColorScheme.Focus);

			Move (bounds.Right - 2, 0);
			Driver.AddRune (Driver.DownArrow);

			Move (bounds.Right - 1, 0);
			Driver.AddRune (Driver.UpArrow);
		}

		///<inheritdoc/>
		public override bool MouseEvent (MouseEvent me)
		{
			if (!ReadOnly && me.Y == Bounds.Top && me.Flags == MouseFlags.Button1Pressed) {
				if (me.X == Bounds.Right - 2) {
					Value = Value - Step;
					return true;
				} else if (me.X == Bounds.Right - 1) {
					Value = Value + Step;
					return true;
				}
			} else if (me.Flags == MouseFlags.Button1Pressed) {
				if (!_textField.HasFocus) {
					_textField.SetFocus ();
				}
				return true;
			}
			return false;
		}

		public override bool ProcessKey (KeyEvent kb)
		{
			if (!ReadOnly) {
				if (kb.Key == Key.CursorUp) {
					Value += Step;
					return true;
				}
				if (kb.Key == Key.CursorDown) {
					Value -= Step;
					return true;
				}
			}
			return base.ProcessKey (kb);
		}
	}
}
