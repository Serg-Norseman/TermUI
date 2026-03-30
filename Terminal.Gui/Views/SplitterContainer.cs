//
// SplitterContainer.cs: SplitterContainer control
//
// Authors:
//   Serg V. Zhdanovskikh
//

using System;

namespace Terminal.Gui
{
	/// <summary>
	/// A container with splitter that can be dragged with the mouse to resize panels above and below (or left and right).
	/// Renders as a line and fires <see cref="Dragged"/> when the user drags it.
	/// </summary>
	public class SplitterContainer : View
	{
		private View _panel1;
		private View _panel2;
		private View _splitterBar;

		private int _initialPercent;
		private int _minSize1 = 5;
		private int _minSize2 = 5;
		private readonly Orientation _orientation;
		private int _splitPosition;

		public View Panel1 => _panel1;
		public View Panel2 => _panel2;

		/// <summary>
		/// Fired when the user drags the splitter. The argument is the new position.
		/// </summary>
		public event EventHandler<int> Dragged;


		public SplitterContainer (Orientation orientation, int initialPercent = 50) : base ()
		{
			this.Width = Dim.Fill ();
			this.Height = Dim.Fill ();
			this.CanFocus = false;

			_orientation = orientation;
			_initialPercent = initialPercent;

			_panel1 = new FrameView () {
				X = 0,
				Y = 0,
				Height = Dim.Fill (),
				CanFocus = false
			};

			_splitterBar = new SplitterBar (this) {
				X = Pos.Right (_panel1),
				Y = 0,
				Width = 1,
				Height = Dim.Fill (),
			};

			_panel2 = new FrameView () {
				X = Pos.Right (_splitterBar),
				Y = 0,
				Width = Dim.Fill (),
				Height = Dim.Fill (),
				CanFocus = false
			};

			if (orientation == Orientation.Horizontal) {
				//Height = 1;
				//Width = Dim.Fill ();
			} else {
				//Height = Dim.Fill ();
				//Width = 1;
			}

			Add (_panel1, _splitterBar, _panel2);
		}

		protected internal override void OnLayoutComplete (LayoutEventArgs args)
		{
			if (_splitPosition == 0 && Bounds.Width > 0) {
				int newPos = (Bounds.Width * _initialPercent) / 100;
				newPos = Math.Max (_minSize1, Math.Min (newPos, Bounds.Width - _minSize2 - 1));

				_splitPosition = newPos;
				UpdateLayout ();
			}

			base.OnLayoutComplete (args);
		}

		private void UpdateLayout ()
		{
			_panel1.Width = _splitPosition;
			LayoutSubviews ();
			SetNeedsDisplay ();
		}

		private void SetSplitPosition (int newPos)
		{
			if (newPos < _minSize1) newPos = _minSize1;
			if (newPos > Bounds.Width - _minSize2 - 1) newPos = Bounds.Width - _minSize2 - 1;

			_splitPosition = newPos;
			UpdateLayout ();

			Dragged?.Invoke (this, newPos);
		}


		private sealed class SplitterBar : View
		{
			private int _dragStart;
			private SplitterContainer _host;
			private bool _isDragging;

			public SplitterBar (SplitterContainer host)
			{
				WantContinuousButtonPressed = true;
				CanFocus = false;
				_host = host;
			}

			/// <inheritdoc />
			public override void Redraw (Rect bounds)
			{
				Clear ();

				var driver = Application.Driver;
				var attr = _isDragging ? driver.MakeAttribute (Color.Black, Color.Cyan) : driver.MakeAttribute (Color.DarkGray, Color.Black);
				driver.SetAttribute (attr);

				if (_host._orientation == Orientation.Horizontal) {
					Move (0, 0);
					for (int i = 0; i < bounds.Width; i++) {
						driver.AddRune (Driver.HLine);
					}
				} else {
					for (int i = 0; i < bounds.Height; i++) {
						Move (0, 0 + i);
						driver.AddRune (Driver.VLine);
					}
				}
			}

			/// <inheritdoc />
			public override bool MouseEvent (MouseEvent mouseEvent)
			{
				int location = (_host._orientation == Orientation.Horizontal) ? mouseEvent.Y : mouseEvent.X;
				var mouseFlags = mouseEvent.Flags;

				if (mouseFlags.HasFlag (MouseFlags.Button1Pressed) && !_isDragging) {
					// Start drag - capture X/Y
					_isDragging = true;
					Application.GrabMouse (this);
					_dragStart = location;
					SetNeedsDisplay ();
					return true;
				}

				if (mouseFlags.HasFlag (MouseFlags.Button1Released) && _isDragging) {
					_isDragging = false;
					Application.UngrabMouse ();
					SetNeedsDisplay ();
					return true;
				}

				if (mouseFlags.HasFlag (MouseFlags.ReportMousePosition) && _isDragging) {
					// delta from drag start
					var delta = location - _dragStart;
					if (delta != 0) {
						int currPos = (_host._orientation == Orientation.Horizontal) ? this.Frame.Y : this.Frame.X;
						int newPos = location + currPos;
						_host.SetSplitPosition (newPos);
						// further movement is incremental
						_dragStart = location;
					}
					return true;
				}

				return base.MouseEvent (mouseEvent);
			}
		}
	}
}
