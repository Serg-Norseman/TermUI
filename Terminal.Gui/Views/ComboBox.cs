//
// ComboBox.cs: ComboBox control
//
// Authors:
//   Ross Ferguson (ross.c.ferguson@btinternet.com)
//   Serg V. Zhdanovskikh
//

using System;
using System.Collections;
using System.Collections.Generic;

namespace Terminal.Gui {

	/// <summary>
	/// Specifies the ComboBox style.
	/// </summary>
	public enum ComboBoxStyle {
		/// <summary>
		/// Specifies that the list is displayed by clicking the down arrow and that the
		/// text portion is editable. The displayed elements correspond to the entered part of the string.
		/// </summary>
		Search,

		/// <summary>
		/// Specifies that the list is always visible and that the text portion is editable.
		/// This means that the user can enter a new value and is not limited to selecting
		/// an existing value in the list.
		/// </summary>
		Simple, // <- !HideDropdownListOnClick

		/// <summary>
		/// Specifies that the list is displayed by clicking the down arrow and that the
		/// text portion is editable. This means that the user can enter a new value and
		/// is not limited to selecting an existing value in the list.
		/// </summary>
		DropDown, // <- !ReadOnly

		/// <summary>
		/// Specifies that the list is displayed by clicking the down arrow and that the
		/// text portion is not editable. This means that the user cannot enter a new value.
		/// Only values already in the list can be selected.
		/// </summary>
		DropDownList // <- ReadOnly
	}


	/// <summary>
	/// Provides a drop-down list of items the user can select from.
	/// 
	/// To get the "classic" behavior, you need HideDropdownListOnClick = true and SearchMode = false.
	/// </summary>
	public class ComboBox : View {

		private class ComboListView : ListView, IPopover {
			private int highlighted = -1;
			private bool isFocusing;
			private ComboBox container;

			public ComboListView (ComboBox container)
			{
				this.container = container ?? throw new ArgumentNullException (nameof (container), "ComboBox container cannot be null.");

				LayoutStyle = LayoutStyle.Computed;
				ColorScheme = container.ColorScheme;
				CanFocus = true;
				TabStop = false;
				Visible = false;

				IgnoreBorderPropertyOnRedraw = true;
			}

			public override bool MouseEvent (MouseEvent me)
			{
				var res = false;
				var isMousePositionValid = (me.X >= 0 && me.X < Frame.Width && me.Y >= 0 && me.Y < Frame.Height);

				int offset = (container.DropDownBorderStyle == BorderStyle.None) ? 0 : 1;
				me.Y = me.Y - offset;

				if (isMousePositionValid) {
					res = base.MouseEvent (me);
				}

				if (container.HideDropdownListOnClick) {
					if (me.Flags == MouseFlags.Button1Clicked) {
						if (!isMousePositionValid && !isFocusing) {
							container.HideList ();
						} else if (isMousePositionValid) {
							OnOpenSelectedItem ();
						} else {
							isFocusing = false;
						}
						return true;
					} else if (me.Flags == MouseFlags.ReportMousePosition) {
						if (isMousePositionValid) {
							highlighted = Math.Min (TopItem + me.Y, Source.Count);
							SetNeedsDisplay ();
						}
						isFocusing = isFocusing && isMousePositionValid;
						return true;
					}
				}

				return res;
			}

			public override void Redraw (Rect bounds)
			{
				var borderStyle = container.DropDownBorderStyle;
				int borders = (borderStyle == BorderStyle.None) ? 0 : 2;
				int offsetXY = (borders == 0) ? 0 : 1;

				Driver.SetAttribute (GetNormalColor ());
				DrawFrame (bounds, 0, true, borderStyle);

				var current = ColorScheme.Focus;
				Driver.SetAttribute (current);
				Move (0, 0);

				var frame = Frame;
				var frameHeight = frame.Height - borders;
				var frameWidth = frame.Width - borders;

				var item = TopItem;
				bool focused = HasFocus;
				int col = AllowsMarking ? 2 : 0;
				int start = LeftItem;
				var hideDropdownListOnClick = container.HideDropdownListOnClick;

				for (int row = 0; row < frameHeight; row++, item++) {
					bool isSelected = item == container.SelectedIndex;
					bool isHighlighted = hideDropdownListOnClick && item == highlighted;

					Attribute newcolor;
					if (isHighlighted || (isSelected && !hideDropdownListOnClick)) {
						newcolor = focused ? ColorScheme.Focus : ColorScheme.HotNormal;
					} else if (isSelected && hideDropdownListOnClick) {
						newcolor = focused ? ColorScheme.HotFocus : ColorScheme.HotNormal;
					} else {
						newcolor = focused ? GetNormalColor () : GetNormalColor ();
					}

					if (newcolor != current) {
						Driver.SetAttribute (newcolor);
						current = newcolor;
					}

					Move (0 + offsetXY, row + offsetXY);
					if (Source == null || item >= Source.Count) {
						for (int c = 0; c < frameWidth; c++)
							Driver.AddRune (' ');
					} else {
						var rowEventArgs = new ListViewRowEventArgs (item);
						OnRowRender (rowEventArgs);
						if (rowEventArgs.RowAttribute != null && current != rowEventArgs.RowAttribute) {
							current = (Attribute)rowEventArgs.RowAttribute;
							Driver.SetAttribute (current);
						}
						if (AllowsMarking) {
							Driver.AddRune (Source.IsMarked (item) ? (AllowsMultipleSelection ? Driver.Checked : Driver.Selected) : (AllowsMultipleSelection ? Driver.UnChecked : Driver.UnSelected));
							Driver.AddRune (' ');
						}
						Source.Render (this, Driver, isSelected, item, col + offsetXY, row + offsetXY, frameWidth - col, start);
					}
				}
			}

			public override bool OnEnter (View view)
			{
				if (container.HideDropdownListOnClick) {
					isFocusing = true;
					highlighted = container.SelectedIndex;
					Application.GrabMouse (this);
				}

				return base.OnEnter (view);
			}

			public override bool OnLeave (View view)
			{
				if (container.HideDropdownListOnClick) {
					isFocusing = false;
					highlighted = container.SelectedIndex;
					Application.UngrabMouse ();
				}

				return base.OnLeave (view);
			}

			public override bool OnSelectedChanged ()
			{
				var res = base.OnSelectedChanged ();

				highlighted = SelectedItem;

				return res;
			}

			///<inheritdoc/>
			public override bool ScrollDown (int items)
			{
				// The entire implementation needs to be moved to ListView - there are too many dependencies on Frame - borders.
				int borders = (container.DropDownBorderStyle == BorderStyle.None) ? 0 : 2;

				TopItem = Math.Max (Math.Min (TopItem + items, Source.Count - (Frame.Height - borders)), 0);
				SetNeedsDisplay ();
				return true;
			}
		}

		IListDataSource source;
		/// <summary>
		/// Gets or sets the <see cref="IListDataSource"/> backing this <see cref="ComboBox"/>, enabling custom rendering.
		/// </summary>
		/// <value>The source.</value>
		/// <remarks>
		///  Use <see cref="SetSource"/> to set a new <see cref="IList"/> source.
		/// </remarks>
		public IListDataSource Source {
			get => source;
			set {
				source = value;

				// Only need to refresh list if its been added to a container view
				if (SuperView != null && SuperView.Subviews.Contains (this)) {
					SelectedIndex = -1;
					search.Text = "";
					Search_Changed (this, "");
					SetNeedsDisplay ();
				}
			}
		}

		/// <summary>
		/// Sets the source of the <see cref="ComboBox"/> to an <see cref="IList"/>.
		/// </summary>
		/// <value>An object implementing the IList interface.</value>
		/// <remarks>
		///  Use the <see cref="Source"/> property to set a new <see cref="IListDataSource"/> source and use custome rendering.
		/// </remarks>
		public void SetSource (IList source)
		{
			if (source == null) {
				Source = null;
			} else {
				listview.SetSource (source);
				Source = listview.Source;
			}
		}

		/// <summary>
		/// This event is raised when the selected item in the <see cref="ComboBox"/> has changed.
		/// </summary>
		public event EventHandler<ListViewItemEventArgs> SelectedIndexChanged;

		/// <summary>
		/// This event is raised when the drop-down list is expanded.
		/// </summary>
		public event EventHandler Expanded;

		/// <summary>
		/// This event is raised when the drop-down list is collapsed.
		/// </summary>
		public event EventHandler Collapsed;

		/// <summary>
		/// This event is raised when the user Double Clicks on an item or presses ENTER to open the selected item.
		/// </summary>
		public event EventHandler<ListViewItemEventArgs> OpenSelectedItem;

		readonly IList searchset = new List<object> ();
		readonly TextField search;
		readonly ComboListView listview;
		readonly int minimumHeight = 2;

		/// <summary>
		/// Overriding to properly handle event propagation.
		/// </summary>
		public override Rect Frame {
			get {
				var selfFrame = base.Frame;
				if (isShow) {
					selfFrame.Height += actualDropHeight;
				}
				return selfFrame;
			}
			set {
				base.Frame = value;
			}
		}

		/// <summary>
		/// Switch between the classic mode - ComboBox works without search filtering
		/// and the default mode of this framework - ComboBox works as a SearchBox.
		/// </summary>
		public bool SearchMode { get; set; }

		/// <summary>
		///   Changed event, raised when the text has changed.
		/// </summary>
		/// <remarks>
		///   This event is raised when the <see cref="Text"/> changes. 
		/// </remarks>
		/// <remarks>
		///   The passed <see cref="EventArgs"/> is a <see cref="string"/> containing the old value. 
		/// </remarks>
		public event EventHandler<string> TextChanged;

		/// <summary>
		/// Public constructor
		/// </summary>
		public ComboBox ()
		{
			search = new TextField ();
			listview = new ComboListView (this);
			Initialize ();
		}

		/// <summary>
		/// Public constructor
		/// </summary>
		/// <param name="text"></param>
		public ComboBox (string text) : this ()
		{
			Text = text;
		}

		/// <summary>
		/// Initialize with the source.
		/// </summary>
		/// <param name="source">The source.</param>
		public ComboBox (IList source) : this ()
		{
			SetSource (source);
		}

		private void Initialize ()
		{
			SearchMode = true;

			if (Bounds.Height < minimumHeight && (Height == null || Height is Dim.DimAbsolute)) {
				Height = minimumHeight;
			}

			search.MouseClick += (s, e) => {
				if (ReadOnly) {
					e.Handled = ExpandCollapse ();
				}
			};

			search.TextChanged += Search_Changed;

			listview.Y = Pos.Bottom (search);
			listview.OpenSelectedItem += (object sender, ListViewItemEventArgs a) => Selected ();

			this.Add (search, listview);

			// On resize
			LayoutComplete += (object s, LayoutEventArgs a) => {
				if (Bounds.Width > 0 && search.Frame.Width != Bounds.Width - 1) {
					search.Width = Bounds.Width - 1;

					listview.Width = Bounds.Width;
					listview.Height = CalculateHeight ();

					search.SetRelativeLayout (Bounds);
					listview.SetRelativeLayout (Bounds);
				}
			};

			listview.SelectedItemChanged += (object sender, ListViewItemEventArgs e) => {
				if (!HideDropdownListOnClick && searchset.Count > 0) {
					SetValue (searchset [listview.SelectedItem]);
				}
			};

			// ?!
			Added += (object sender, View v) => {
				SetNeedsLayout ();
				SetNeedsDisplay ();
				Search_Changed (this, Text.ToString ());
			};

			// Things this view knows how to do
			AddCommand (Command.Accept, () => ActivateSelected ());
			AddCommand (Command.ToggleExpandCollapse, () => ExpandCollapse ());
			AddCommand (Command.Expand, () => Expand ());
			AddCommand (Command.Collapse, () => Collapse ());
			AddCommand (Command.LineDown, () => MoveDown ());
			AddCommand (Command.LineUp, () => MoveUp ());
			AddCommand (Command.PageDown, () => PageDown ());
			AddCommand (Command.PageUp, () => PageUp ());
			AddCommand (Command.TopHome, () => MoveHome ());
			AddCommand (Command.BottomEnd, () => MoveEnd ());
			AddCommand (Command.Cancel, () => CancelSelected ());
			AddCommand (Command.UnixEmulation, () => UnixEmulation ());

			// Default keybindings for this view
			AddKeyBinding (Key.Enter, Command.Accept);
			AddKeyBinding (Key.F4, Command.ToggleExpandCollapse);
			AddKeyBinding (Key.CursorDown, Command.LineDown);
			AddKeyBinding (Key.CursorUp, Command.LineUp);
			AddKeyBinding (Key.PageDown, Command.PageDown);
			AddKeyBinding (Key.PageUp, Command.PageUp);
			AddKeyBinding (Key.Home, Command.TopHome);
			AddKeyBinding (Key.End, Command.BottomEnd);
			AddKeyBinding (Key.Esc, Command.Cancel);
			AddKeyBinding (Key.U | Key.CtrlMask, Command.UnixEmulation);
		}

		private bool isShow = false;
		private int selectedIndex = -1;
		private int lastSelectedIndex = -1;
		private bool hideDropdownListOnClick;

		/// <summary>
		/// Gets the index of the currently selected item in the <see cref="Source"/>
		/// </summary>
		/// <value>The selected item or -1 none selected.</value>
		public int SelectedIndex {
			get => selectedIndex;
			set {
				if (selectedIndex != value && (value == -1
					|| (source != null && value > -1 && value < source.Count))) {

					selectedIndex = lastSelectedIndex = value;
					if (selectedIndex != -1) {
						SetValue (source.ToList () [selectedIndex].ToString (), true);
					} else {
						SetValue ("", true);
					}
					OnSelectedChanged ();
				}
			}
		}

		/// <summary>
		/// Gets the drop down list state, expanded or collapsed.
		/// </summary>
		public bool IsShow {
			get { return isShow; }
			private set {
				if (isShow != value) {
					isShow = value;

					// Necessary to restore the screen outside the current window
					// from which the menu was called with output over and beyond
					// the window border.
					if (!isShow && Application.Initialized) {
						Application.Refresh ();
					}
				}
			}
		}

		///<inheritdoc/>
		public new ColorScheme ColorScheme {
			get {
				return base.ColorScheme;
			}
			set {
				listview.ColorScheme = value;
				base.ColorScheme = value;
				SetNeedsDisplay ();
			}
		}

		/// <summary>
		///If set to true its not allow any changes in the text.
		/// </summary>
		public bool ReadOnly {
			get => search.ReadOnly;
			set => search.ReadOnly = value;
		}

		/// <summary>
		/// Gets or sets if the drop-down list can be hide with a button click event.
		/// </summary>
		public bool HideDropdownListOnClick {
			get => hideDropdownListOnClick;
			set => hideDropdownListOnClick = listview.WantContinuousButtonPressed = value;
		}

		///<inheritdoc/>
		public override bool MouseEvent (MouseEvent me)
		{
			if (me.X == Bounds.Right - 1 && me.Y == Bounds.Top && me.Flags == MouseFlags.Button1Pressed) {
				return ExpandCollapse ();
			} else if (me.Flags == MouseFlags.Button1Pressed) {
				if (!search.HasFocus) {
					search.SetFocus ();
				}

				return true;
			}

			return false;
		}

		private void FocusSelectedItem ()
		{
			// If no item is selected
			if (SelectedIndex > -1) {
				listview.SelectedItem = SelectedIndex > -1 && SelectedIndex < searchset.Count ? SelectedIndex : 0;
			}
			listview.TabStop = true;
			listview.SetFocus ();
			OnExpanded ();
		}

		/// <summary>
		/// Virtual method which invokes the <see cref="Expanded"/> event.
		/// </summary>
		public virtual void OnExpanded ()
		{
			Expanded?.Invoke (this, EventArgs.Empty);
		}

		/// <summary>
		/// Virtual method which invokes the <see cref="Collapsed"/> event.
		/// </summary>
		public virtual void OnCollapsed ()
		{
			Collapsed?.Invoke (this, EventArgs.Empty);
		}

		///<inheritdoc/>
		public override bool OnEnter (View view)
		{
			if (!search.HasFocus && !listview.HasFocus) {
				search.SetFocus ();
			}

			search.CursorPosition = search.Text.Length;

			if (ReadOnly)
				Application.Driver.SetCursorVisibility (CursorVisibility.Invisible);

			return base.OnEnter (view);
		}

		///<inheritdoc/>
		public override bool OnLeave (View view)
		{
			if (source?.Count > 0 && selectedIndex > -1 && selectedIndex < source.Count - 1
				&& search.Text != source.ToList () [selectedIndex].ToString ()) {

				SetValue (source.ToList () [selectedIndex].ToString ());
			}
			if (isShow && view != this && view != search && view != listview) {
				HideList ();
			} else if (listview.TabStop) {
				listview.TabStop = false;
			}

			return base.OnLeave (view);
		}

		/// <summary>
		/// Invokes the SelectedChanged event if it is defined.
		/// </summary>
		/// <returns></returns>
		public virtual bool OnSelectedChanged ()
		{
			// Note: Cannot rely on "listview.SelectedItem != lastSelectedItem" because the list is dynamic. 
			// So we cannot optimize. Ie: Don't call if not changed
			SelectedIndexChanged?.Invoke (this, new ListViewItemEventArgs (SelectedIndex, search.Text));

			return true;
		}

		/// <summary>
		/// Invokes the OnOpenSelectedItem event if it is defined.
		/// </summary>
		/// <returns></returns>
		public virtual bool OnOpenSelectedItem ()
		{
			var value = search.Text;
			lastSelectedIndex = SelectedIndex;
			OpenSelectedItem?.Invoke (this, new ListViewItemEventArgs (SelectedIndex, value));

			return true;
		}

		///<inheritdoc/>
		public override void Redraw (Rect bounds)
		{
			base.Redraw (bounds);

			Driver.SetAttribute (ColorScheme.Focus);
			Move (Bounds.Right - 1, 0);
			Driver.AddRune (Driver.DownArrow);
		}

		///<inheritdoc/>
		public override bool ProcessKey (KeyEvent e)
		{
			var result = InvokeKeybindings (e);
			if (result != null)
				return (bool)result;

			return base.ProcessKey (e);
		}

		bool UnixEmulation ()
		{
			// Unix emulation
			Reset ();
			return true;
		}

		bool CancelSelected ()
		{
			search.SetFocus ();
			if (ReadOnly || HideDropdownListOnClick) {
				SelectedIndex = lastSelectedIndex;
				if (SelectedIndex > -1 && SelectedIndex < listview.Source?.Count) {
					search.Text = listview.Source.ToList () [SelectedIndex].ToString ();
				}
			} else if (!ReadOnly) {
				search.Text = "";
				selectedIndex = lastSelectedIndex;
				OnSelectedChanged ();
			}
			Collapse ();
			return true;
		}

		bool? MoveEnd ()
		{
			if (!isShow && search.HasFocus) {
				return null;
			}
			if (HasItems ()) {
				listview.MoveEnd ();
			}
			return true;
		}

		bool? MoveHome ()
		{
			if (!isShow && search.HasFocus) {
				return null;
			}
			if (HasItems ()) {
				listview.MoveHome ();
			}
			return true;
		}

		bool PageUp ()
		{
			if (HasItems ()) {
				listview.MovePageUp ();
			}
			return true;
		}

		bool PageDown ()
		{
			if (HasItems ()) {
				listview.MovePageDown ();
			}
			return true;
		}

		bool? MoveUp ()
		{
			if (search.HasFocus) { // stop odd behavior on KeyUp when search has focus
				return true;
			}

			if (listview.HasFocus && listview.SelectedItem == 0 && searchset.Count > 0) // jump back to search
			{
				search.CursorPosition = search.Text.Length;
				search.SetFocus ();
				return true;
			}
			return null;
		}

		bool? MoveDown ()
		{
			if (search.HasFocus) { // jump to list
				if (searchset.Count > 0) {
					listview.TabStop = true;
					listview.SetFocus ();
					SetValue (searchset [listview.SelectedItem]);
				} else {
					listview.TabStop = false;
					SuperView?.FocusNext ();
				}
				return true;
			}
			return null;
		}

		/// <summary>
		/// Toggles the expand/collapse state of the sublist in the combo box
		/// </summary>
		/// <returns></returns>
		bool ExpandCollapse ()
		{
			if (search.HasFocus || listview.HasFocus) {
				if (!isShow) {
					return Expand ();
				} else {
					return Collapse ();
				}
			}
			return false;
		}

		bool ActivateSelected ()
		{
			if (HasItems ()) {
				Selected ();
				return true;
			}
			return false;
		}

		bool HasItems ()
		{
			return Source?.Count > 0;
		}

		/// <summary>
		/// Collapses the drop down list.  Returns true if the state chagned or false
		/// if it was already collapsed and no action was taken
		/// </summary>
		public virtual bool Collapse ()
		{
			if (!isShow) {
				return false;
			}

			HideList ();

			return true;
		}

		/// <summary>
		/// Expands the drop down list.  Returns true if the state chagned or false
		/// if it was already expanded and no action was taken
		/// </summary>
		public virtual bool Expand ()
		{
			if (isShow) {
				return false;
			}

			UpdateSearchSet ();
			if (searchset.Count < 1) {
				return false;
			}

			ShowList ();
			FocusSelectedItem ();

			return true;
		}

		/// <summary>
		/// The currently selected list item or entered text
		/// </summary>
		public new string Text {
			get {
				return search.Text;
			}
			set {
				search.Text = value;
			}
		}

		private void SetValue (object text, bool isFromSelectedItem = false)
		{
			search.TextChanged -= Search_Changed;
			search.Text = text.ToString ();
			search.CursorPosition = 0;
			search.TextChanged += Search_Changed;
			if (!isFromSelectedItem) {
				selectedIndex = GetSelectedItemFromSource (search.Text.ToString ());
				OnSelectedChanged ();
			}
		}

		private void Selected ()
		{
			listview.TabStop = false;

			if (listview.Source.Count == 0 || searchset.Count == 0) {
				HideList ();
				return;
			}

			SetValue (searchset [listview.SelectedItem]);
			search.CursorPosition = search.Text.Length;
			Search_Changed (this, search.Text.ToString ());
			OnOpenSelectedItem ();
			Reset (keepSearchText: true);

			HideList ();
		}

		private int GetSelectedItemFromSource (string value)
		{
			if (source == null) {
				return -1;
			}
			var itemsList = source.ToList ();
			for (int i = 0; i < itemsList.Count; i++) {
				if (itemsList [i].ToString () == value) {
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Reset to full original list
		/// </summary>
		private void Reset (bool keepSearchText = false)
		{
			if (!keepSearchText) {
				search.Text = string.Empty;
			}

			UpdateSearchSet ();

			listview.SetSource (searchset);
			listview.Height = CalculateHeight ();

			if (HasFocus && Subviews.Count > 0) {
				search.SetFocus ();
			}
		}

		private void UpdateSearchSet ()
		{
			searchset.Clear ();

			if (Source == null)
				return;

			var itemsList = Source.ToList ();

			if (!SearchMode) {
				foreach (var item in itemsList) {
					searchset.Add (item);
				}
			} else {
				string searchVal = search.Text.ToString ();

				foreach (var item in itemsList) {
					if (item.ToString ().StartsWith (searchVal, StringComparison.CurrentCultureIgnoreCase)) {
						searchset.Add (item);
					}
				}
			}
		}

		private void Search_Changed (object sender, string text)
		{
			TextChanged?.Invoke (this, text);

			if (source == null) { // Object initialization		
				return;
			}

			if (search.Text != text && SearchMode && HasFocus) {
				isShow = false; // for strong Expand
				Expand ();
			}
		}

		/// <summary>
		/// Show the search list
		/// </summary>
		private void ShowList ()
		{
			listview.SetSource (searchset);
			listview.Clear (); // Ensure list shrinks in Dialog as you type
			listview.Height = CalculateHeight ();
			listview.Visible = true;
			SuperView?.BringSubviewToFront (this);

			IsShow = true;
		}

		/// <summary>
		/// Hide the search list
		/// </summary>
		private void HideList ()
		{
			if (lastSelectedIndex != selectedIndex) {
				OnOpenSelectedItem ();
			}
			var rect = listview.ViewToScreen (listview.Bounds);
			Reset (keepSearchText: true);
			listview.Clear (rect);
			listview.TabStop = false;
			listview.Visible = false;
			SuperView?.SendSubviewToBack (this);
			SuperView?.SetNeedsDisplay (rect);
			OnCollapsed ();

			IsShow = false;
		}

		/// <summary>
		/// Internal height of dynamic search list
		/// </summary>
		/// <returns></returns>
		private int CalculateHeight ()
		{
			actualDropHeight = Math.Min (maxDropDownItems, searchset.Count);

			int borders = (DropDownBorderStyle == BorderStyle.None) ? 0 : 2;
			actualDropHeight += borders;

			return actualDropHeight;
		}

		private int actualDropHeight;
		private int maxDropDownItems = 10;

		/// <summary>
		/// Maximum number of items in the drop-down list.
		/// </summary>
		public int MaxDropDownItems {
			get { return maxDropDownItems; }
			set { maxDropDownItems = value; }
		}

		public BorderStyle DropDownBorderStyle {
			get {
				if (listview.Border == null) {
					listview.Border = new Border () { BorderStyle = BorderStyle.None };
				}
				return listview.Border.BorderStyle;
			}
			set { listview.Border = new Border () { BorderStyle = value }; }
		}
	}
}
