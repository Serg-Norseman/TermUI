//
// StackLayout.cs
//
// Authors:
//   Serg V. Zhdanovskikh
//

using System;
using System.Linq;

namespace Terminal.Gui
{
	public class StackLayout : View
	{
		readonly Orientation orientation;
		readonly int padding;
		readonly int spacing;

		public StackLayout (params View? [] views) : this (Orientation.Vertical, 0, 0, views) { }

		public StackLayout (Orientation orientation, int padding, int spacing, params View [] views)
		{
			this.orientation = orientation;
			this.padding = padding;
			this.spacing = Math.Max (0, spacing);

			AddRange (views);
		}

		public override void Add (View view)
		{
			if (Subviews.Count > 0) {
				var prevView = Subviews [Subviews.Count - 1];

				switch (orientation) {
					case Orientation.Vertical:
						view.X = padding;
						view.Y = (spacing == 0) ? Pos.Bottom (prevView) : Pos.Bottom (prevView) + spacing;
						break;

					case Orientation.Horizontal:
						view.X = (spacing == 0) ? Pos.Right (prevView) : Pos.Right (prevView) + spacing;
						view.Y = padding;
						break;
				}
			} else {
				view.X = padding;
				view.Y = padding;
			}

			base.Add (view);
		}

		public void AddRange (params View [] views)
		{
			switch (orientation) {
				case Orientation.Vertical:
					int maxWidth = views.Max (vw => vw.Bounds.Width);
					this.Width = maxWidth;
					this.Height = Dim.Fill ();
					break;

				case Orientation.Horizontal:
					// FIXME: dirty hack for ComboBox that don't work unless height is 2 or more
					int maxHeight = views.Max (vh => (vh is ComboBox) ? 1 : vh.Bounds.Height);
					this.Height = maxHeight;
					this.Width = Dim.Fill ();
					break;
			}

			foreach (var view in views) {
				if (view != null)
					Add (view);
			}
		}
	}
}
