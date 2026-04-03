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
		private int _minSize1 = 15;
		private int _minSize2 = 15;
		private readonly Orientation _orientation;
		private int _splitPosition;


		public int MinSize1
		{
			get { return _minSize1; }
			set { _minSize1 = value; }
		}

		public int MinSize2
		{
			get { return _minSize2; }
			set { _minSize2 = value; }
		}

		public Orientation Orientation => _orientation;
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

			_panel1 = new View ();
			_panel1.X = 0;
			_panel1.Y = 0;
			_panel1.CanFocus = false;

			_splitterBar = new SplitterBar (this);

			_panel2 = new FrameView ();
			_panel2.Width = Dim.Fill ();
			_panel2.Height = Dim.Fill ();
			_panel2.CanFocus = false;

			if (orientation == Orientation.Horizontal) {
				_panel1.Width = Dim.Fill ();

				_splitterBar.X = 0;
				_splitterBar.Y = Pos.Bottom (_panel1);
				_splitterBar.Width = Dim.Fill ();
				_splitterBar.Height = 1;

				_panel2.X = 0;
				_panel2.Y = Pos.Bottom (_splitterBar);
			} else {
				_panel1.Height = Dim.Fill ();

				_splitterBar.X = Pos.Right (_panel1);
				_splitterBar.Y = 0;
				_splitterBar.Width = 1;
				_splitterBar.Height = Dim.Fill ();

				_panel2.X = Pos.Right (_splitterBar);
				_panel2.Y = 0;
			}

			Add (_panel1, _splitterBar, _panel2);
		}

		protected internal override void OnLayoutComplete (LayoutEventArgs args)
		{
			if (_splitPosition == 0 && !Bounds.IsEmpty) {
				int newPos;
				if (_orientation == Orientation.Horizontal) {
					newPos = (Bounds.Height * _initialPercent) / 100;
					newPos = Math.Max (_minSize1, Math.Min (newPos, Bounds.Height - _minSize2 - 1));
				} else {
					newPos = (Bounds.Width * _initialPercent) / 100;
					newPos = Math.Max (_minSize1, Math.Min (newPos, Bounds.Width - _minSize2 - 1));
				}

				_splitPosition = newPos;
				UpdateLayout ();
			}

			base.OnLayoutComplete (args);
		}

		private void UpdateLayout ()
		{
			if (_orientation == Orientation.Horizontal) {
				_panel1.Height = _splitPosition;
			} else {
				_panel1.Width = _splitPosition;
			}

			LayoutSubviews ();
			SetNeedsDisplay ();
		}

		private void SetSplitPosition (int newPos)
		{
			if (newPos < _minSize1) newPos = _minSize1;

			if (_orientation == Orientation.Horizontal) {
				if (newPos > Bounds.Height - _minSize2 - 1) newPos = Bounds.Height - _minSize2 - 1;
			} else {
				if (newPos > Bounds.Width - _minSize2 - 1) newPos = Bounds.Width - _minSize2 - 1;
			}

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
