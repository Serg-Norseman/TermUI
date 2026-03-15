//
// RadioButton.cs: Radio button with grouping for Terminal.Gui v1
//
// Authors:
//   Serg V. Zhdanovskikh
//

using System;
using System.Collections.Generic;

namespace Terminal.Gui {
	/// <summary>
	/// RadioButton with grouping support.
	/// Only one RadioButton in a group can be selected at a time.
	/// Group is determined by the Group string property.
	/// </summary>
	public class RadioButton : View {
		private string _group;
		private bool _isChanging;
		private bool _checked;

		/// <summary>
		/// Group name. RadioButtons with the same Group are mutually exclusive.
		/// </summary>
		public string Group {
			get => _group;
			set {
				if (_group != value) {
					_group = value;
					// When changing group, check if this breaks exclusivity
					if (Checked) {
						UncheckOthersInGroup ();
					}
				}
			}
		}

		/// <summary>
		/// State change event (after group processing)
		/// </summary>
		public event EventHandler<ToggledEventArgs> CheckedChanged;

		public event EventHandler Clicked;

		public RadioButton () : base ()
		{
			Initialize ();
		}

		private void Initialize ()
		{
			CanFocus = true;
			AutoSize = true;

			_checked = false;
			_group = string.Empty;
			_isChanging = false;

			// Subscribe to base CheckBox state change
			//Toggled += OnRadioButtonToggled;

			UpdateTextFormatterText ();
			ProcessResizeView ();
		}

		/// <summary>
		/// Checked property override for group logic handling
		/// </summary>
		public bool Checked {
			get => _checked;
			set {
				if (_checked != value) {
					_isChanging = true;
					try {
						// If setting to true, uncheck others in the group
						if (value) {
							UncheckOthersInGroup ();
						}

						_checked = value;
						OnToggled (!value, value);
					} finally {
						_isChanging = false;
					}
				}
			}
		}

		/// <summary>
		/// Find and uncheck other RadioButtons in the same group
		/// </summary>
		private void UncheckOthersInGroup ()
		{
			if (string.IsNullOrEmpty (_group)/* || _isChanging*/)
				return;

			var siblings = GetRadioButtonsInGroup ();
			foreach (RadioButton rb in siblings) {
				if (rb != this && rb.Checked) {
					rb._isChanging = true;
					try {
						rb._checked = false;
						rb.OnToggled (true, false);
					} finally {
						rb._isChanging = false;
					}
				}
			}
		}

		/// <summary>
		/// Find all RadioButtons with the same Group in SuperView
		/// </summary>
		private RadioButton [] GetRadioButtonsInGroup ()
		{
			if (SuperView == null)
				return new RadioButton [] { this };

			// Recursive search in nested containers
			var result = new List<RadioButton> ();
			GetRadioButtonsInContainer (SuperView, result);
			return result.ToArray ();
		}

		private void GetRadioButtonsInContainer (View container, List<RadioButton> list)
		{
			foreach (var view in container.Subviews) {
				if (view is RadioButton rb && !string.IsNullOrEmpty (rb._group) && rb._group == _group) {
					list.Add (rb);
				} else {
					GetRadioButtonsInContainer (view, list);
				}
			}
		}

		protected virtual void OnToggled (bool oldValue, bool newValue)
		{
			UpdateTextFormatterText ();
			ProcessResizeView ();

			CheckedChanged?.Invoke (this, new ToggledEventArgs { OldValue = oldValue, NewValue = newValue });
		}

		///<inheritdoc/>
		public override void PositionCursor ()
		{
			Move (0, 0);
		}

		///<inheritdoc/>
		public override bool OnEnter (View view)
		{
			Application.Driver.SetCursorVisibility (CursorVisibility.Invisible);

			return base.OnEnter (view);
		}

		///<inheritdoc/>
		public override bool ProcessKey (KeyEvent kb)
		{
			if (kb.Key == Key.Space || kb.Key == Key.Enter) {
				if (!Checked) {
					Checked = true;
				}
				return true;
			}

			return base.ProcessKey (kb);
		}

		///<inheritdoc/>
		public override bool MouseEvent (MouseEvent me)
		{
			if (!me.Flags.HasFlag (MouseFlags.Button1Clicked) || !CanFocus)
				return false;

			SetFocus ();

			Clicked?.Invoke (this, EventArgs.Empty);

			if (!Checked) {
				Checked = true;
			}

			SetNeedsDisplay ();

			return true;
		}

		///<inheritdoc/>
		/*public override void Redraw (Rect bounds)
		{
			var current = ColorScheme.Focus;
			Driver.SetAttribute (current);
			Move (0, 0);

			// Radio button symbol instead of CheckBox square
			Driver.AddRune ('(');
			Driver.AddRune (Checked ? (Rune)'\u25CF' : (Rune)' ');
			Driver.AddStr (") ");

			// the font does not have these symbols when testing (Windows \ Consolas)
			//Driver.AddRune (Checked ? (Rune)'◉' : (Rune)'○');
			//Driver.AddRune (Checked ? (Rune)'\u25C9' : (Rune)'\u25EF'); 

			var tf = TextFormatter;
			tf.Text = Text;
			tf.Draw (bounds, current, HasFocus ? ColorScheme.HotFocus : ColorScheme.HotNormal);
		}*/

		/// <inheritdoc/>
		protected override void UpdateTextFormatterText ()
		{
			// Windows \ Lucida Console - symbol \u25CF missing
			// Windows \ Consolas - symbol \a missing

			string radioMark = Checked ? "(\u25CF)" : "( )";

			switch (TextAlignment) {
			case TextAlignment.Left:
			case TextAlignment.Centered:
			case TextAlignment.Justified:
				TextFormatter.Text = radioMark + " " + Text;
				break;
			case TextAlignment.Right:
				TextFormatter.Text = Text + " " + radioMark;
				break;
			}
		}
	}

	/// <summary>
	/// State change event arguments
	/// </summary>
	public class ToggledEventArgs : EventArgs {
		public bool OldValue { get; set; }
		public bool NewValue { get; set; }
	}
}
