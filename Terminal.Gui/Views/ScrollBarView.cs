//
// ScrollBarView.cs: ScrollBarView view.
//
// Authors:
//   Miguel de Icaza (miguel@gnome.org)
//

using System;

namespace Terminal.Gui
{
	public class ScrollBarView : ScrollBar
	{
		private ScrollBarView otherScrollBarView;
		private View contentBottomRightCorner;
		private bool showBothScrollIndicator => OtherScrollBarView != null && OtherScrollBarView.showScrollIndicator && showScrollIndicator;

		public ScrollBarView () : base (0, 0, false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Gui.ScrollBarView"/> class using <see cref="LayoutStyle.Computed"/> layout.
		/// </summary>
		/// <param name="size">The size that this scrollbar represents.</param>
		/// <param name="position">The position within this scrollbar.</param>
		/// <param name="isVertical">If set to <c>true</c> this is a vertical scrollbar, otherwise, the scrollbar is horizontal.</param>
		public ScrollBarView (int size, int position, bool isVertical) : base (size, position, isVertical)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Gui.ScrollBarView"/> class using <see cref="LayoutStyle.Computed"/> layout.
		/// </summary>
		/// <param name="host">The view that will host this scrollbar.</param>
		/// <param name="isVertical">If set to <c>true</c> this is a vertical scrollbar, otherwise, the scrollbar is horizontal.</param>
		/// <param name="showBothScrollIndicator">If set to <c>true (default)</c> will have the other scrollbar, otherwise will have only one.</param>
		public ScrollBarView (View host, bool isVertical, bool showBothScrollIndicator = true) : base (host, isVertical)
		{
			if (showBothScrollIndicator) {
				OtherScrollBarView = new ScrollBarView (0, 0, !isVertical) {
					ColorScheme = host.ColorScheme,
					Host = host,
					CanFocus = false,
					Enabled = host.Enabled,
					Visible = host.Visible,
					OtherScrollBarView = this
				};
				OtherScrollBarView.hosted = true;
				OtherScrollBarView.X = OtherScrollBarView.IsVertical ? Pos.Right (host) - 1 : Pos.Left (host);
				OtherScrollBarView.Y = OtherScrollBarView.IsVertical ? Pos.Top (host) : Pos.Bottom (host) - 1;
				OtherScrollBarView.Host.SuperView.Add (OtherScrollBarView);
				OtherScrollBarView.ShowScrollIndicator = true;
			}

			contentBottomRightCorner = new View (" ") { Visible = host.Visible, ColorScheme = host.ColorScheme };
			Host.SuperView.Add (contentBottomRightCorner);
			contentBottomRightCorner.X = Pos.Right (host) - 1;
			contentBottomRightCorner.Y = Pos.Bottom (host) - 1;
			contentBottomRightCorner.Width = 1;
			contentBottomRightCorner.Height = 1;
			contentBottomRightCorner.DrawContent += ContentBottomRightCorner_DrawContent;
		}

		protected override void Host_VisibleChanged (object sender, EventArgs e)
		{
			if (!Host.Visible) {
				Visible = Host.Visible;
				if (otherScrollBarView != null) {
					otherScrollBarView.Visible = Visible;
				}
				contentBottomRightCorner.Visible = Visible;
			} else {
				ShowHideScrollBars ();
			}
		}

		protected override void Host_EnabledChanged (object sender, EventArgs e)
		{
			Enabled = Host.Enabled;
			if (otherScrollBarView != null) {
				otherScrollBarView.Enabled = Enabled;
			}
			contentBottomRightCorner.Enabled = Enabled;
		}

		private void ContentBottomRightCorner_DrawContent (object sender, Rect obj)
		{
			Driver.SetAttribute (Host.HasFocus ? GetFocusColor () : GetNormalColor ());
			Host.SuperView.AddRune (contentBottomRightCorner.Frame.X, contentBottomRightCorner.Frame.Y, ' ');
		}

		/// <summary>
		/// Represent a vertical or horizontal ScrollBarView other than this.
		/// </summary>
		public ScrollBarView OtherScrollBarView
		{
			get => otherScrollBarView;
			set {
				if (value != null && (value.IsVertical && vertical || !value.IsVertical && !vertical)) {
					throw new ArgumentException ($"There is already a {(vertical ? "vertical" : "horizontal")} ScrollBarView.");
				}
				otherScrollBarView = value;
			}
		}

		protected override void ShowHideScrollBars (bool redraw = true)
		{
			if (!hosted || (hosted && !autoHideScrollBars)) {
				if (contentBottomRightCorner != null && contentBottomRightCorner.Visible) {
					contentBottomRightCorner.Visible = false;
				} else if (otherScrollBarView != null && otherScrollBarView.contentBottomRightCorner != null && otherScrollBarView.contentBottomRightCorner.Visible) {
					otherScrollBarView.contentBottomRightCorner.Visible = false;
				}
				return;
			}

			var pending = CheckBothScrollBars (this);
			if (otherScrollBarView != null) {
				CheckBothScrollBars (otherScrollBarView, pending);
			}

			SetWidthHeight ();
			SetRelativeLayout (Bounds);
			if (otherScrollBarView != null) {
				OtherScrollBarView.SetRelativeLayout (OtherScrollBarView.Bounds);
			}

			if (showBothScrollIndicator) {
				if (contentBottomRightCorner != null && !contentBottomRightCorner.Visible) {
					contentBottomRightCorner.Visible = true;
				} else if (otherScrollBarView != null && otherScrollBarView.contentBottomRightCorner != null && !otherScrollBarView.contentBottomRightCorner.Visible) {
					otherScrollBarView.contentBottomRightCorner.Visible = true;
				}
			} else if (!showScrollIndicator) {
				if (contentBottomRightCorner != null && contentBottomRightCorner.Visible) {
					contentBottomRightCorner.Visible = false;
				} else if (otherScrollBarView != null && otherScrollBarView.contentBottomRightCorner != null && otherScrollBarView.contentBottomRightCorner.Visible) {
					otherScrollBarView.contentBottomRightCorner.Visible = false;
				}
				if (Application.MouseGrabView != null && Application.MouseGrabView == this) {
					Application.UngrabMouse ();
				}
			} else if (contentBottomRightCorner != null && contentBottomRightCorner.Visible) {
				contentBottomRightCorner.Visible = false;
			} else if (otherScrollBarView != null && otherScrollBarView.contentBottomRightCorner != null && otherScrollBarView.contentBottomRightCorner.Visible) {
				otherScrollBarView.contentBottomRightCorner.Visible = false;
			}
			if (Host?.Visible == true && showScrollIndicator && !Visible) {
				Visible = true;
			}
			if (Host?.Visible == true && otherScrollBarView?.showScrollIndicator == true && !otherScrollBarView.Visible) {
				otherScrollBarView.Visible = true;
			}

			if (!redraw) {
				return;
			}

			if (showScrollIndicator) {
				Redraw (Bounds);
			}
			if (otherScrollBarView != null && otherScrollBarView.showScrollIndicator) {
				otherScrollBarView.Redraw (otherScrollBarView.Bounds);
			}
		}

		private bool CheckBothScrollBars (ScrollBarView scrollBarView, bool pending = false)
		{
			int barsize = scrollBarView.vertical ? scrollBarView.Bounds.Height : scrollBarView.Bounds.Width;

			if (barsize == 0 || barsize >= scrollBarView.contentSize) {
				if (scrollBarView.showScrollIndicator) {
					scrollBarView.ShowScrollIndicator = false;
				}
				if (scrollBarView.Visible) {
					scrollBarView.Visible = false;
				}
			} else if (barsize > 0 && barsize == scrollBarView.contentSize && scrollBarView.OtherScrollBarView != null && pending) {
				if (scrollBarView.showScrollIndicator) {
					scrollBarView.ShowScrollIndicator = false;
				}
				if (scrollBarView.Visible) {
					scrollBarView.Visible = false;
				}
				if (scrollBarView.OtherScrollBarView != null && scrollBarView.showBothScrollIndicator) {
					scrollBarView.OtherScrollBarView.ShowScrollIndicator = false;
				}
				if (scrollBarView.OtherScrollBarView.Visible) {
					scrollBarView.OtherScrollBarView.Visible = false;
				}
			} else if (barsize > 0 && barsize == contentSize && scrollBarView.OtherScrollBarView != null && !pending) {
				pending = true;
			} else {
				if (scrollBarView.OtherScrollBarView != null && pending) {
					if (!scrollBarView.showBothScrollIndicator) {
						scrollBarView.OtherScrollBarView.ShowScrollIndicator = true;
					}
					if (!scrollBarView.OtherScrollBarView.Visible) {
						scrollBarView.OtherScrollBarView.Visible = true;
					}
				}
				if (!scrollBarView.showScrollIndicator) {
					scrollBarView.ShowScrollIndicator = true;
				}
				if (!scrollBarView.Visible) {
					scrollBarView.Visible = true;
				}
			}

			return pending;
		}

		protected override void SetWidthHeight ()
		{
			if (showBothScrollIndicator) {
				Width = vertical ? 1 : Dim.Width (Host) - 1;
				Height = vertical ? Dim.Height (Host) - 1 : 1;

				otherScrollBarView.Width = otherScrollBarView.vertical ? 1 : Dim.Width (Host) - 1;
				otherScrollBarView.Height = otherScrollBarView.vertical ? Dim.Height (Host) - 1 : 1;
			} else if (showScrollIndicator) {
				base.SetWidthHeight ();
			} else if (otherScrollBarView?.showScrollIndicator == true) {
				otherScrollBarView.Width = otherScrollBarView.vertical ? 1 : Dim.Width (Host) - 0;
				otherScrollBarView.Height = otherScrollBarView.vertical ? Dim.Height (Host) - 0 : 1;
			}
		}
	}
}
