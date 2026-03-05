using System.Collections.Generic;
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
				Height = 6 // 6 - for dropdown tests, 20 - for list tests
			};
			Win.Add (frameView);

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
				Y = 1,
				Width = Dim.Percent (40)
			};
			comboBox.SetSource (items);

			listview.SelectedItemChanged += (sender, e) => {
				lbListView.Text = items [e.Item];
				comboBox.SelectedItem = e.Item;
			};

			comboBox.SelectedItemChanged += (object sender, ListViewItemEventArgs text) => {
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

			var chkReadOnly = new CheckBox ("ReadOnly", false) { X = Pos.Right (frameView) + 3, Y = Pos.Bottom (btnThree) + 3 };
			chkReadOnly.Toggled += (previousState) => {
				if (comboBox != null) {
					comboBox.ReadOnly = chkReadOnly.Checked;
				}
			};
			Win.Add (chkReadOnly);
		}
	}
}