using Terminal.Gui;

namespace UICatalog.Scenarios {
	[ScenarioMetadata (Name: "NumericStepper", Description: "Demo for NumericStepper")]
	[ScenarioCategory ("Controls"), ScenarioCategory ("NumericStepper")]
	public class NumericStepperExample : Scenario {
		public override void Setup ()
		{
			var lblNum = new Label () {
				X = 10,
				Y = 10,
				Text = "NumericStepper"
			};

			var numStepper = new NumericStepper () {
				X = 10,
				Y = 11,
				Width = 20,
				Minimum = 0,
				Maximum = 70,
				Step = 1,
				Value = 7,
			};
			Win.Add (lblNum, numStepper);
		}
	}
}