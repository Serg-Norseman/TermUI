using Terminal.Gui;

namespace UICatalog.Scenarios {
	[ScenarioMetadata (Name: "RadioButton", Description: "Demo for RadioButton")]
	[ScenarioCategory ("Controls"), ScenarioCategory ("RadioButton")]
	public class RadioButtonExample : Scenario {
		public override void Setup ()
		{
			// === Group 1: Gender ===
			var genderGroup = "gender";

			var rbMale = new RadioButton () { Location = new Point (2, 2), Text = "Male", Group = genderGroup };
			var rbFemale = new RadioButton () { Location = new Point (2, 3), Text = "Female", Group = genderGroup };
			var rbOther = new RadioButton () { Location = new Point (2, 4), Text = "Other", Group = genderGroup };

			// === Group 2: Access Level ===
			var accessGroup = "access";

			var accessFrame = new FrameView (accessGroup) { Location = new Point (2, 6), Size = new Size (30, 7) };
			var rbGuest = new RadioButton () { Location = new Point (2, 1), Text = "Guest", Group = accessGroup };
			var rbUser = new RadioButton () { Location = new Point (2, 2), Text = "User", Group = accessGroup, Checked = true };
			var rbAdmin = new RadioButton () { Location = new Point (2, 3), Text = "Administrator", Group = accessGroup };
			accessFrame.Add (rbGuest, rbUser, rbAdmin);

			// === Group 3: No group (standalone) ===
			var rbStandalone = new RadioButton () { Location = new Point (2, 14), Text = "Standalone", Group = "" };

			var lblToggle = new Label () { Location = new Point (2, 20), Text = "debug line" };

			// Subscribe to events
			rbMale.CheckedChanged += (s, e) => {
				if (e.NewValue) lblToggle.Text = "Selected: Male";
			};

			rbAdmin.CheckedChanged += (s, e) => {
				if (e.NewValue) lblToggle.Text = "Selected: Administrator";
			};

			Win.Add (rbMale, rbFemale, rbOther, accessFrame, rbStandalone, lblToggle);
		}
	}
}
