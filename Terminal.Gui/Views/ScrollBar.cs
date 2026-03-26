//
// ScrollBar.cs: ScrollBar view.
//
// Authors:
//   Miguel de Icaza (miguel@gnome.org)
//   Serg V. Zhdanovskikh
//

using System;

namespace Terminal.Gui
{
	/// <summary>
	/// ScrollBarViews are views that display a 1-character scrollbar, either horizontal or vertical
	/// </summary>
	/// <remarks>
	/// <para>
	///   The scrollbar is drawn to be a representation of the Size, assuming that the 
	///   scroll position is set at Position.
	/// </para>
	/// <para>
	///   If the region to display the scrollbar is larger than three characters, 
	///   arrow indicators are drawn.
	/// </para>
	/// </remarks>
	public class ScrollBar : View
	{
		protected bool autoHideScrollBars = true;
		protected bool vertical;
		protected int contentSize, position;
		protected bool showScrollIndicator;
		protected bool hosted;


		public ScrollBar () : this (0, 0, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Gui.ScrollBarView"/> class using <see cref="LayoutStyle.Computed"/> layout.
		/// </summary>
		/// <param name="size">The size that this scrollbar represents.</param>
		/// <param name="position">The position within this scrollbar.</param>
		/// <param name="isVertical">If set to <c>true</c> this is a vertical scrollbar, otherwise, the scrollbar is horizontal.</param>
		public ScrollBar (int size, int position, bool isVertical) : base ()
		{
			base.WantContinuousButtonPressed = true;

			vertical = isVertical;
			this.position = position;
			this.contentSize = size;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Gui.ScrollBarView"/> class using <see cref="LayoutStyle.Computed"/> layout.
		/// </summary>
		/// <param name="host">The view that will host this scrollbar.</param>
		/// <param name="isVertical">If set to <c>true</c> this is a vertical scrollbar, otherwise, the scrollbar is horizontal.</param>
		/// <param name="showBothScrollIndicator">If set to <c>true (default)</c> will have the other scrollbar, otherwise will have only one.</param>
		public ScrollBar (View host, bool isVertical) : this (0, 0, isVertical)
		{
			if (host == null) {
				throw new ArgumentNullException ("The host parameter can't be null.");
			} else if (host.SuperView == null) {
				throw new ArgumentNullException ("The host SuperView parameter can't be null.");
			}

			hosted = true;
			ColorScheme = host.ColorScheme;
			X = isVertical ? Pos.Right (host) - 1 : Pos.Left (host);
			Y = isVertical ? Pos.Top (host) : Pos.Bottom (host) - 1;
			Host = host;
			CanFocus = false;
			Enabled = host.Enabled;
			Visible = host.Visible;
			//Host.CanFocusChanged += Host_CanFocusChanged;
			Host.EnabledChanged += Host_EnabledChanged;
			Host.VisibleChanged += Host_VisibleChanged;
			Host.SuperView.Add (this);
			AutoHideScrollBars = true;
			ShowScrollIndicator = true;
			ClearOnVisibleFalse = false;
		}

		/// <summary>
		/// Get or sets the view that host this <see cref="View"/>
		/// </summary>
		public View Host { get; internal set; }

		protected virtual void Host_VisibleChanged (object sender, EventArgs e)
		{
			if (!Host.Visible) {
				Visible = Host.Visible;
			} else {
				ShowHideScrollBars ();
			}
		}

		protected virtual void Host_EnabledChanged (object sender, EventArgs e)
		{
			Enabled = Host.Enabled;
		}

		/// <summary>
		/// If set to <c>true</c> this is a vertical scrollbar, otherwise, the scrollbar is horizontal.
		/// </summary>
		public bool IsVertical
		{
			get => vertical;
			set {
				vertical = value;
				SetNeedsDisplay ();
			}
		}

		/// <summary>
		/// The size of content the scrollbar represents.
		/// </summary>
		/// <value>The size.</value>
		/// <remarks>The <see cref="Size"/> is typically the size of the virtual content. E.g. when a Scrollbar is
		/// part of a <see cref="View"/> the Size is set to the appropriate dimension of <see cref="Host"/>.</remarks>
		public new int Size
		{
			get => contentSize;
			set {
				contentSize = value;
				SetRelativeLayout (Bounds);
				ShowHideScrollBars (false);
				OnResize ();
				SetNeedsDisplay ();
			}
		}

		/// <summary>
		/// This event is raised when the position on the scrollbar has changed.
		/// </summary>
		public event EventHandler ChangedPosition;

		/// <summary>
		/// The position, relative to <see cref="Size"/>, to set the scrollbar at.
		/// </summary>
		/// <value>The position.</value>
		public int Position
		{
			get => position;
			set {
				if (position != value && CanScroll (value)) {
					position = value;
					OnChangedPosition ();
					SetNeedsDisplay ();
				}
			}
		}

		/// <summary>
		/// Gets or sets the visibility for the vertical or horizontal scroll indicator.
		/// </summary>
		/// <value><c>true</c> if show vertical or horizontal scroll indicator; otherwise, <c>false</c>.</value>
		public bool ShowScrollIndicator
		{
			get => showScrollIndicator;
			set {
				if (value == showScrollIndicator) {
					return;
				}

				showScrollIndicator = value;
				SetNeedsLayout ();
				if (value) {
					Visible = true;
				} else {
					Visible = false;
					Position = 0;
				}
				SetWidthHeight ();
			}
		}

		/// <summary>
		/// If true the vertical/horizontal scroll bars won't be showed if it's not needed.
		/// </summary>
		public bool AutoHideScrollBars
		{
			get => autoHideScrollBars;
			set {
				if (autoHideScrollBars != value) {
					autoHideScrollBars = value;
					SetNeedsDisplay ();
				}
			}
		}

		/// <summary>
		/// Virtual method to invoke the <see cref="ChangedPosition"/> action event.
		/// </summary>
		public virtual void OnChangedPosition ()
		{
			ChangedPosition?.Invoke (this, EventArgs.Empty);
			RecalcVars ();
		}

		protected override void OnResize ()
		{
			base.OnResize ();
			RecalcVars ();
		}

		/// <summary>
		/// Only used for a hosted view that will update and redraw the scrollbars.
		/// </summary>
		public virtual void Refresh ()
		{
			ShowHideScrollBars ();
		}

		protected virtual void ShowHideScrollBars (bool redraw = true)
		{
		}

		///<inheritdoc/>
		public override void Redraw (Rect region)
		{
			if (ColorScheme == null || ((!showScrollIndicator || Size == 0) && AutoHideScrollBars && Visible)) {
				if ((!showScrollIndicator || Size == 0) && AutoHideScrollBars && Visible) {
					ShowHideScrollBars (false);
				}
				return;
			}

			Driver.SetAttribute (Host.HasFocus ? ColorScheme.Focus : GetNormalColor ());

			RecalcVars ();
			bool fixedThumb = (thumb2 == thumb1);

			if (vertical) {
				if ((Bounds.Height == 0) || (region.Right < Bounds.Width - 1)) {
					return;
				}

				var col = Bounds.Width - 1;
				var bh = Bounds.Height;
				if (bh > 1) {
					Move (col, 0);
					Driver.AddRune (Driver.UpArrow);
					if (bh == 3) {
						Move (col, 1);
						Driver.AddRune (Driver.Square);
					}
					Move (col, bh - 1);
					Driver.AddRune (Driver.DownArrow);
				}

				if (bh > 3) {
					for (int y = 1; y < bh - 1; y++) {
						Move (col, y);
						Rune special;
						if (y < thumb1 || y > thumb2) {
							special = ConsoleDriver.Stipple;
						} else if (y == thumb1 && fixedThumb) {
							special = Driver.Square;
						} else if (y == thumb1) {
							special = Driver.TopTee;
						} else if (y == thumb2) {
							special = Driver.BottomTee;
						} else {
							special = Driver.VLine;
						}
						Driver.AddRune (special);
					}
				}
			} else {
				if ((Bounds.Width == 0) || (region.Bottom < Bounds.Height - 1)) {
					return;
				}

				var row = Bounds.Height - 1;
				var bw = Bounds.Width;
				if (bw > 1) {
					Move (0, row);
					Driver.AddRune (Driver.LeftArrow);
					if (bw == 3) {
						Move (1, row);
						Driver.AddRune (Driver.Square);
					}
					Move (bw - 1, row);
					Driver.AddRune (Driver.RightArrow);
				}

				if (bw > 3) {
					Move (1, row);
					for (int x = 1; x < bw - 1; x++) {
						Rune special;
						if (x < thumb1 || x > thumb2) {
							special = ConsoleDriver.Stipple;
						} else if (x == thumb1 && fixedThumb) {
							special = Driver.Square;
						} else if (x == thumb1) {
							special = Driver.LeftTee;
						} else if (x == thumb2) {
							special = Driver.RightTee;
						} else {
							special = Driver.HLine;
						}
						Driver.AddRune (special);
					}
				}
			}
		}

		int lastLocation = -1;
		int posBarOffset;

		///<inheritdoc/>
		public override bool MouseEvent (MouseEvent mouseEvent)
		{
			var mouseFlags = mouseEvent.Flags;

			if (mouseFlags != MouseFlags.Button1Pressed && mouseFlags != MouseFlags.Button1DoubleClicked &&
				!mouseFlags.HasFlag (MouseFlags.Button1Pressed | MouseFlags.ReportMousePosition) &&
				mouseFlags != MouseFlags.Button1Released && mouseFlags != MouseFlags.WheeledDown &&
				mouseFlags != MouseFlags.WheeledUp && mouseFlags != MouseFlags.WheeledRight &&
				mouseFlags != MouseFlags.WheeledLeft && mouseFlags != MouseFlags.Button1TripleClicked) {
				return false;
			}

			if (!Host.CanFocus) {
				return true;
			}
			if (Host?.HasFocus == false) {
				Host.SetFocus ();
			}

			RecalcVars ();

			int location = vertical ? mouseEvent.Y : mouseEvent.X;

			if (mouseFlags == MouseFlags.Button1Pressed && (Application.MouseGrabView == null || Application.MouseGrabView != this)) {
				Application.GrabMouse (this);
			} else if (mouseFlags == MouseFlags.Button1Released && Application.MouseGrabView != null && Application.MouseGrabView == this) {
				lastLocation = -1;
				skipDirection = 0;
				Application.UngrabMouse ();
				return true;
			}

			if (showScrollIndicator && (mouseFlags == MouseFlags.WheeledDown || mouseFlags == MouseFlags.WheeledUp ||
				mouseFlags == MouseFlags.WheeledRight || mouseFlags == MouseFlags.WheeledLeft)) {
				return Host.MouseEvent (mouseEvent);
			}

			bool fixedThumb = (Application.Style == TUIStyle.Classic);

			if (mouseFlags.HasFlag (MouseFlags.ReportMousePosition | MouseFlags.Button1Pressed) && lastLocation > -1) {
				if (fixedThumb) {

					var newPos = (int)((location - 1) / fixFactor);
					TryScroll (newPos - position);

				} else {

					var deltaLoc = location - posBarOffset;
					var trackRatio = contentSize / (float)trackSize;

					int newPos = 0;
					if (location > lastLocation) {
						newPos = (int)Math.Round ((deltaLoc * trackRatio) + trackRatio);
					} else if (location < lastLocation) {
						newPos = (int)Math.Round ((deltaLoc * trackRatio) - trackRatio);
					}

					if (newPos != 0) TryScroll ((int)newPos - position);

				}
				return true;
			} else {
				if (mouseFlags.HasFlag (MouseFlags.Button1Pressed) && lastLocation < 0) {
					if ((location >= thumb1 && location <= thumb2)) {
						lastLocation = location;
						posBarOffset = Math.Max (location - thumb1, 1);
					} else if (location == 0) {
						TryScroll (-1);
					} else if (location == pageSize - 1) {
						TryScroll (+1);
					} else if (location > thumb2 && skipDirection >= 0) {
						TryScroll (+pageSize);
						skipDirection += 1;
					} else if (location < thumb1 && skipDirection <= 0) {
						TryScroll (-pageSize);
						skipDirection -= 1;
					}
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// To solve the bug with position jerking when WantContinuousButtonPressed is enabled:
		/// after a click to the right of the slider, the position changes,
		/// but then a new click event arrives with the same cursor coordinates,
		/// and the position is calculated as a click on the left.
		/// </summary>
		private int skipDirection = 0;

		internal bool TryScroll (int n)
		{
			var newPos = Math.Max (Math.Min (positionMax, position + n), 0);
			if (newPos >= 0 && newPos <= positionMax) {
				Position = newPos;
				return true;
			}
			return false;
		}

		internal bool CanScroll (int newPos)
		{
			if (!Bounds.IsEmpty && newPos >= 0 && newPos <= positionMax) {
				return true;
			}
			return false;
		}

		void RecalcVars ()
		{
			if (Host?.Bounds.IsEmpty != false) {
				pageSize = 0;
				trackSize = 0;
				positionMax = 0;
				thumb1 = thumb2 = 0;
				return;
			}

			bool fixedThumb = (Application.Style == TUIStyle.Classic);

			pageSize = vertical ? Bounds.Height : Bounds.Width;
			trackSize = pageSize - 2;
			positionMax = contentSize - pageSize;
			float posRatio = position / (float)positionMax;

			//bool fixedThumb = (Application.Style == TUIStyle.Classic);
			if (contentSize != 0) {
				if (!fixedThumb) {
					float pageRatio = pageSize / (float)contentSize;
					thumbSize = Math.Max (1, (int)(trackSize * pageRatio));
					thumb1 = 1 + (int)((trackSize - thumbSize) * posRatio);
					thumb2 = Math.Min (trackSize, thumb1 + thumbSize);
					if (thumb2 - thumb1 < thumbSize) thumb1 = thumb2 - thumbSize;
				} else {
					thumbSize = 1;
					thumb1 = 1 + (int)Math.Round ((trackSize - thumbSize) * posRatio);
					thumb2 = thumb1;

					fixFactor = (trackSize - thumbSize) / (float)positionMax;
				}
			}
		}

		int pageSize, trackSize, positionMax;
		int thumbSize, thumb1, thumb2;
		float fixFactor;

		///<inheritdoc/>
		public override bool OnEnter (View view)
		{
			Application.Driver.SetCursorVisibility (CursorVisibility.Invisible);

			return base.OnEnter (view);
		}

		protected virtual void SetWidthHeight ()
		{
			if (showScrollIndicator) {
				Width = vertical ? 1 : Dim.Width (Host) - 0;
				Height = vertical ? Dim.Height (Host) - 0 : 1;
			}
		}
	}
}
