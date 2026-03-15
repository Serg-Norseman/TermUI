using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace UICatalog.Scenarios {
	[ScenarioMetadata (Name: "ComboBoxIteration", Description: "ComboBox iteration.")]
	[ScenarioCategory ("Controls"), ScenarioCategory ("ComboBox")]
	public class ComboBoxIteration : Scenario {
		public override void Setup ()
		{
			var items = new List<string> () { "one", "two", "three", "four", "five", "six", "seven" };

			var lbListView = new Label () {
				AutoSize = false,
				Width = 10,
				Height = 1
			};
			Win.Add (lbListView);

			var listview = new ListView (items) {
				Y = Pos.Bottom (lbListView) + 1,
				Width = 10,
				Height = Dim.Fill (2)
			};
			Win.Add (listview);

			var frameView = new FrameView () {
				X = Pos.Right (lbListView) + 3,
				Width = Dim.Percent (40),
				Height = 7 // 6 - for dropdown tests, 20 - for list tests
			};
			Win.Add (frameView);

			var txtValue = new TextField ();
			txtValue.X = Pos.Left (frameView);
			txtValue.Y = Pos.Bottom (frameView) + 20;
			txtValue.Width = 30;
			txtValue.Text = "text from combo";
			Win.Add (txtValue);

			var lbComboBox = new Label () {
				ColorScheme = Colors.TopLevel,
				X = 1,
				Y = 0,
				Width = Dim.Percent (40)
			};

			var comboBox = new ComboBox () {
				//DropDownBorderStyle = BorderStyle.Single,
				MaxDropDownItems = 5,
				X = 1,
				Y = 2,
				Width = Dim.Percent (40)
			};
			comboBox.SetSource (items);

			comboBox.HideDropdownListOnClick = true;
			comboBox.SearchMode = false;

			listview.SelectedItemChanged += (sender, e) => {
				lbListView.Text = items [e.Item];
				comboBox.SelectedIndex = e.Item;
			};

			comboBox.TextChanged += (s, e) => {
				txtValue.Text = comboBox.Text;
			};

			comboBox.SelectedIndexChanged += (object sender, ListViewItemEventArgs text) => {
				if (text.Item != -1) {
					lbComboBox.Text = text.Value.ToString ();
					listview.SelectedItem = text.Item;
				}
			};
			frameView.Add (lbComboBox, comboBox);

			// Debugging clicks on a drop-down list (falls into the component below it)!
			frameView.Add (new TextField { X = 1, Y = Pos.Top (comboBox) + 2, Height = 1, Width = 20 });

			var btnTwo = new Button ("Two") {
				X = Pos.Right (frameView) + 3,
			};
			btnTwo.Clicked += (sender, e) => {
				items = new List<string> () { "one", "two" };
				comboBox.SetSource (items);
				listview.SetSource (items);
				listview.SelectedItem = 0;
			};
			Win.Add (btnTwo);

			var btnThree = new Button ("Three") {
				X = Pos.Right (frameView) + 3,
				Y = Pos.Top (comboBox)
			};
			btnThree.Clicked += (sender, e) => {
				items = new List<string> () { "one", "two", "three" };
				comboBox.SetSource (items);
				listview.SetSource (items);
				listview.SelectedItem = 0;
			};
			Win.Add (btnThree);

			var chkSearchMode = new CheckBox ("SearchMode", false) { X = Pos.Right (frameView) + 3, Y = Pos.Bottom (btnThree) + 2 };
			Win.Add (chkSearchMode);

			var chkReadOnly = new CheckBox ("ReadOnly", false) { X = Pos.Right (frameView) + 3, Y = Pos.Bottom (btnThree) + 3 };
			Win.Add (chkReadOnly);

			var chkHideDropdownListOnClick = new CheckBox ("HideDropdownListOnClick", false) { X = Pos.Right (frameView) + 3, Y = Pos.Bottom (btnThree) + 4 };
			Win.Add (chkHideDropdownListOnClick);

			chkReadOnly.Checked = comboBox.ReadOnly;
			chkSearchMode.Checked = comboBox.SearchMode;
			chkHideDropdownListOnClick.Checked = comboBox.HideDropdownListOnClick;

			chkSearchMode.CheckedChanged += (s, previousState) => {
				if (comboBox != null) {
					comboBox.SearchMode = chkSearchMode.Checked;
				}
			};
			chkReadOnly.CheckedChanged += (s, previousState) => {
				if (comboBox != null) {
					comboBox.ReadOnly = chkReadOnly.Checked;
				}
			};
			chkHideDropdownListOnClick.CheckedChanged += (s, previousState) => {
				if (comboBox != null) {
					comboBox.HideDropdownListOnClick = chkHideDropdownListOnClick.Checked;
					if (chkHideDropdownListOnClick.Checked) {
						frameView.Height = 7;
					} else {
						frameView.Height = 15;
					}
				}
			};

			var borderStyleEnum = Enum.GetValues (typeof (BorderStyle)).Cast<BorderStyle> ().ToList ();
			var rbBorderStyle = new RadioGroup (borderStyleEnum.Select (e => NStack.ustring.Make (e.ToString ())).ToArray ()) {
				X = Pos.Left (chkReadOnly),
				Y = Pos.Bottom (btnThree) + 7,
				SelectedItem = (int)comboBox.DropDownBorderStyle
			};
			Win.Add (rbBorderStyle);
			rbBorderStyle.SelectedItemChanged += (s, e) => {
				comboBox.DropDownBorderStyle = (BorderStyle)e.SelectedItem;
				comboBox.SetNeedsDisplay ();
			};


		}
	}
}