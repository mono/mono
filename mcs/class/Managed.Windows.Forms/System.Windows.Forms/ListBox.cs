// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//
// TODO:
//	- Keyboard navigation
//	- Horizontal item scroll
//	- Performance testing
//
//

// NOT COMPLETE

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace System.Windows.Forms
{

	public class ListBox : ListControl
	{
		internal class ListBoxInfo
		{
			internal int item_height; 		/* Item's height */
			internal int top_item;			/* First item that we show the in the current page */
			internal int last_item;			/* Last visible item */
			internal int page_size;			/* Number of listbox items per page. In MultiColumn listbox indicates items per column */
			internal Rectangle textdrawing_rect;	/* Displayable Client Rectangle minus the scrollbars and with IntegralHeight calculated*/
			internal bool show_verticalsb;		/* Is Vertical scrollbar show it? */
			internal bool show_horizontalsb;	/* Is Horizontal scrollbar show it? */
			internal Rectangle client_rect;		/* Client Rectangle. Usually = ClientRectangle except when IntegralHeight has been applied*/
			internal int max_itemwidth;		/* Maxium item width within the listbox */

			public ListBoxInfo ()
			{
				last_item = 0;
				item_height = 0;
				top_item = 0;
				page_size = 0;
				max_itemwidth = 0;
				show_verticalsb = false;
				show_horizontalsb = false;
			}
		}

		internal class ListBoxItem
		{
			internal int Index;
			internal bool Selected;
			internal CheckState State;

			public ListBoxItem (int index)
			{
				Index = index;
				Selected = false;
				State = CheckState.Unchecked;
			}
		}

		private BorderStyle border_style;
		private int column_width;
		private DrawMode draw_mode;
		private int horizontal_extent;
		private bool horizontal_scrollbar;
		private bool integral_height;
		private bool multicolumn;
		private bool scroll_always_visible;
		private int selected_index;
		private SelectedIndexCollection selected_indices;
		private object selected_item;
		private SelectedObjectCollection selected_items;
		private SelectionMode selection_mode;
		private bool sorted;
		private bool use_tabstops;
		private int preferred_height;
		private int top_index;
		private int column_width_internal;
		private VScrollBar vscrollbar_ctrl;
		private HScrollBar hscrollbar_ctrl;
		private bool suspend_ctrlupdate;

		internal StringFormat string_format;
		internal ListBoxInfo listbox_info;
		internal ObjectCollection items;

		public ListBox ()
		{
			border_style = BorderStyle.Fixed3D;			
			draw_mode = DrawMode.Normal;
			horizontal_extent = 0;
			horizontal_scrollbar = false;
			integral_height = true;
			multicolumn = false;
			preferred_height = 7;
			scroll_always_visible = false;
			selected_index = -1;
			selected_item = null;
			selection_mode = SelectionMode.One;
			sorted = false;
			top_index = 0;
			use_tabstops = true;
			BackColor = ThemeEngine.Current.ColorWindow;
			ColumnWidth = 0;
			suspend_ctrlupdate = false;

			items = new ObjectCollection (this);
			selected_indices = new SelectedIndexCollection (this);
			selected_items = new SelectedObjectCollection (this);
			listbox_info = new ListBoxInfo ();
			string_format = new StringFormat ();
			listbox_info.item_height = FontHeight;

			/* Vertical scrollbar */
			vscrollbar_ctrl = new VScrollBar ();
			vscrollbar_ctrl.Minimum = 0;
			vscrollbar_ctrl.SmallChange = 1;
			vscrollbar_ctrl.LargeChange = 1;
			vscrollbar_ctrl.Maximum = 0;
			vscrollbar_ctrl.ValueChanged += new EventHandler (VerticalScrollEvent);
			vscrollbar_ctrl.Visible = false;

			/* Horizontal scrollbar */
			hscrollbar_ctrl = new HScrollBar ();
			hscrollbar_ctrl.Minimum = 0;
			hscrollbar_ctrl.SmallChange = 1;
			hscrollbar_ctrl.LargeChange = 1;
			hscrollbar_ctrl.Maximum = 0;
			hscrollbar_ctrl.Visible = false;
			hscrollbar_ctrl.ValueChanged += new EventHandler (HorizontalScrollEvent);

			/* Events */
			MouseDown += new MouseEventHandler (OnMouseDownLB);

			UpdateFormatString ();
		}

		#region Events
		public new event EventHandler BackgroundImageChanged;
		public new event EventHandler Click;
		public event DrawItemEventHandler DrawItem;
		public event MeasureItemEventHandler MeasureItem;
		public new event PaintEventHandler Paint;
		public event EventHandler SelectedIndexChanged;
		public new event EventHandler TextChanged;
		#endregion // Events

		#region Public Properties
		public override Color BackColor {
			get { return base.BackColor; }
			set {
				if (base.BackColor == value)
					return;

    				base.BackColor = value;
				Refresh ();
			}
		}

		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set {
				if (base.BackgroundImage == value)
					return;

    				base.BackgroundImage = value;

    				if (BackgroundImageChanged != null)
					BackgroundImageChanged (this, EventArgs.Empty);

				Refresh ();
			}
		}

		public BorderStyle BorderStyle {
			get { return border_style; }

    			set {
				if (!Enum.IsDefined (typeof (BorderStyle), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for BorderStyle", value));

				if (border_style == value)
					return;

    				border_style = value;
				Refresh ();
    			}
		}

		public int ColumnWidth {
			get { return column_width; }
			set {
				if (column_width < 0)
					throw new ArgumentException ("A value less than zero is assigned to the property.");

    				column_width = value;

    				if (value == 0)
    					ColumnWidthInternal = 120;
    				else
    					ColumnWidthInternal = value;

				Refresh ();
			}
		}

		protected override CreateParams CreateParams {
			get { return base.CreateParams;}
		}

		protected override Size DefaultSize {
			get { return new Size (120, 96); }
		}

		public virtual DrawMode DrawMode {
			get { return draw_mode; }

    			set {
				if (!Enum.IsDefined (typeof (DrawMode), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for DrawMode", value));

				if (draw_mode == value)
					return;

    				draw_mode = value;
				Refresh ();
    			}
		}

		public override Color ForeColor {
			get { return base.ForeColor; }
			set {

				if (base.ForeColor == value)
					return;

    				base.ForeColor = value;
				Refresh ();
			}
		}

		public int HorizontalExtent {
			get { return horizontal_extent; }
			set {
				if (horizontal_extent == value)
					return;

    				horizontal_extent = value;
				Refresh ();
			}
		}

		public bool HorizontalScrollbar {
			get { return horizontal_scrollbar; }
			set {
				if (horizontal_scrollbar == value)
					return;

    				horizontal_scrollbar = value;
    				UpdateShowHorizontalScrollBar ();
				Refresh ();
			}
		}

		public bool IntegralHeight {
			get { return integral_height; }
			set {
				if (integral_height == value)
					return;

    				integral_height = value;
				CalcClientArea ();
			}
		}

		public virtual int ItemHeight {
			get { return listbox_info.item_height; }
			set {
				if (value > 255)
					throw new ArgumentOutOfRangeException ("The ItemHeight property was set beyond 255 pixels");

				listbox_info.item_height = value;
				CalcClientArea ();
			}
		}

		public ObjectCollection Items {
			get { return items; }
		}

		public bool MultiColumn {
			get { return multicolumn; }
			set {
				if (multicolumn == value)
					return;

				if (value == true && DrawMode == DrawMode.OwnerDrawVariable)
					throw new ArgumentException ("A multicolumn ListBox cannot have a variable-sized height.");

    				multicolumn = value;
    				UpdateShowVerticalScrollBar (); /* the needs for scrollbars may change */
				UpdateShowHorizontalScrollBar ();
				Refresh ();
			}
		}

		public int PreferredHeight {
			get { return preferred_height;}
		}

		public override RightToLeft RightToLeft {
			get { return base.RightToLeft; }
			set {
				if (base.RightToLeft == value)
					return;

    				base.RightToLeft = value;
    				UpdateFormatString ();
				Refresh ();
			}
		}

		// Only afects the Vertical ScrollBar
		public bool ScrollAlwaysVisible {
			get { return scroll_always_visible; }
			set {
				if (scroll_always_visible == value)
					return;

    				scroll_always_visible = value;
				UpdateShowVerticalScrollBar ();
				UpdateShowHorizontalScrollBar ();
			}
		}

		public override int SelectedIndex {
			get { return selected_index;}
			set {
				if (value < -1 || value >= Items.Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				if (SelectionMode == SelectionMode.None)
					throw new ArgumentException ("cannot call this method if SelectionMode is SelectionMode.None");

				if (selected_index == value)
					return;

				if (SelectionMode == SelectionMode.One)
					UnSelectItem (selected_index, true);

    				SelectItem (value);
    				selected_index = value;
    				OnSelectedIndexChanged  (new EventArgs ());
			}
		}

		public SelectedIndexCollection SelectedIndices {
			get { return selected_indices; }
		}

		public object SelectedItem {
			get {
				if (SelectedItems.Count > 0)
					return SelectedItems[0];
				else
					return null;
			}
			set {
				if (selected_item == value)
					return;

				int index = Items.IndexOf (value);

				if (index == -1)
					return;

				SelectedIndex = index;
			}
		}

		public SelectedObjectCollection SelectedItems {
			get {return selected_items;}
		}

		public virtual SelectionMode SelectionMode {
			get { return selection_mode; }
    			set {
				if (!Enum.IsDefined (typeof (SelectionMode), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for SelectionMode", value));

				if (selection_mode == value)
					return;

    				selection_mode = value;
				Refresh ();
    			}
		}

		public bool Sorted {
			get { return sorted; }

    			set {
				if (sorted == value)
					return;

    				sorted = value;
    				Sort ();
    			}
		}

		public override string Text {
			get {
				if (SelectionMode != SelectionMode.None && SelectedIndex != -1)
					return Items[SelectedIndex].ToString ();

				return base.Text;
			}
			set {

				base.Text = value;

				if (SelectionMode == SelectionMode.None)
					return;

				int index;

				index = FindStringExact (value);

				if (index == -1)
					return;

				SelectedIndex = index;
			}
		}

		public int TopIndex {
			get { return top_index;}
			set {
				if (value == top_index)
					return;

				if (value < 0 || value >= Items.Count)
					return;

				value = top_index;
				Refresh ();
			}
		}

		public bool UseTabStops {
			get { return use_tabstops; }

    			set {
				if (use_tabstops == value)
					return;

    				use_tabstops = value;
    				UpdateFormatString ();
				Refresh ();
    			}
		}

		#endregion Public Properties

		#region Private Properties

		internal ListBoxInfo LBoxInfo {
			get { return listbox_info; }
		}

		private int ColumnWidthInternal {
			get { return column_width_internal; }
			set { column_width_internal = value; }
		}

		#endregion Private Properties

		#region Public Methods
		protected virtual void AddItemsCore (object[] value)
		{
			Items.AddRange (value);
		}

		public void BeginUpdate ()
		{
			suspend_ctrlupdate = true;
		}

		public void ClearSelected ()
		{
			foreach (int i in selected_indices) {
				UnSelectItem (i, false);
			}

			selected_indices.ClearIndices ();
			selected_items.ClearObjects ();
		}

		protected virtual ObjectCollection CreateItemCollection ()
		{
			return new ObjectCollection (this);
		}

		public void EndUpdate ()
		{
			suspend_ctrlupdate = false;
			UpdateItemInfo (false, -1, -1);
			Refresh ();
		}

		public int FindString (String s)
		{
			return FindString (s, 0);
		}

		public int FindString (string s,  int startIndex)
		{
			for (int i = startIndex; i < Items.Count; i++) {
				if ((Items[i].ToString ()).StartsWith (s))
					return i;
			}

			return -1;
		}

		public int FindStringExact (string s)
		{
			return FindStringExact (s, 0);
		}

		public int FindStringExact (string s,  int startIndex)
		{
			for (int i = startIndex; i < Items.Count; i++) {
				if ((Items[i].ToString ()).Equals (s))
					return i;
			}

			return -1;
		}

		public int GetItemHeight (int index)
		{
			if (index < 0 || index >= Items.Count)
				throw new ArgumentOutOfRangeException ("Index of out range");

			return ItemHeight;
		}

		public Rectangle GetItemRectangle (int index)
		{
			if (index < 0 || index >= Items.Count)
				throw new  ArgumentOutOfRangeException ("GetItemRectangle index out of range.");

			Rectangle rect = new Rectangle ();

			if (MultiColumn == false) {

				rect.X = 0;
				rect.Y = ItemHeight * index;
				rect.Height = ItemHeight;
				rect.Width = listbox_info.textdrawing_rect.Width;
			}
			else {
				int which_page;

				if (listbox_info.page_size == 0) {
					listbox_info.page_size = 1;
				}

				which_page = index / listbox_info.page_size;
				rect.Y = (index % listbox_info.page_size) * ItemHeight;
				rect.X = which_page * ColumnWidthInternal;
				rect.Height = ItemHeight;
				rect.Width = ColumnWidthInternal;
			}

			return rect;
		}

		public bool GetSelected (int index)
		{
			if (index < 0 || index >= Items.Count)
				throw new ArgumentOutOfRangeException ("Index of out range");

			return (Items.GetListBoxItem (index)).Selected;
		}

		public int IndexFromPoint (Point p)
		{
			return IndexFromPoint (p.X, p.Y);
		}

		// Only returns visible points
		public int IndexFromPoint (int x, int y)
		{
			for (int i = LBoxInfo.top_item; i < LBoxInfo.last_item; i++) {
				if (GetItemRectangle (i).Contains (x,y) == true)
					return i;
			}

			return -1;
		}

		protected override void OnChangeUICues (UICuesEventArgs e)
		{
			base.OnChangeUICues (e);
		}

		protected override void OnDataSourceChanged (EventArgs e)
		{
			base.OnDataSourceChanged (e);
		}

		protected override void OnDisplayMemberChanged (EventArgs e)
		{
			base.OnDisplayMemberChanged (e);
		}

		protected virtual void OnDrawItem (DrawItemEventArgs e)
		{
			if (DrawItem != null && (DrawMode == DrawMode.OwnerDrawFixed || DrawMode == DrawMode.OwnerDrawVariable))
				DrawItem (this, e);

			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
					(ThemeEngine.Current.ColorHilight), e.Bounds);

				e.Graphics.DrawString (Items[e.Index].ToString (), e.Font,
					ThemeEngine.Current.ResPool.GetSolidBrush (ThemeEngine.Current.ColorHilightText),
					e.Bounds, string_format);
			}
			else {
				e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
					(e.BackColor), e.Bounds);

				e.Graphics.DrawString (Items[e.Index].ToString (), e.Font,
					ThemeEngine.Current.ResPool.GetSolidBrush (e.ForeColor),
					e.Bounds, string_format);
			}
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);

			UpdateShowHorizontalScrollBar ();
			UpdateShowVerticalScrollBar ();
			RellocateScrollBars ();
			CalcClientArea ();
			UpdateItemInfo (false, -1, -1);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);

			UpdateInternalClientRect (ClientRectangle);
			Controls.Add (vscrollbar_ctrl);
			Controls.Add (hscrollbar_ctrl);

			if (Sorted)
				Sort ();
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected virtual void OnMeasureItem (MeasureItemEventArgs e)
		{

		}

		protected override void OnParentChanged (EventArgs e)
		{
			base.OnParentChanged (e);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
			UpdateInternalClientRect (ClientRectangle);
		}

		protected override void OnSelectedIndexChanged (EventArgs e)
		{
			base.OnSelectedIndexChanged (e);

			if (SelectedIndexChanged != null)
				SelectedIndexChanged (this, e);
		}

		protected override void OnSelectedValueChanged (EventArgs e)
		{
			base.OnSelectedValueChanged (e);
		}

		public override void Refresh ()
		{
			base.Refresh ();
		}

		protected override void RefreshItem (int index)
		{

		}

		protected override void SetBoundsCore (int x,  int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected override void SetItemCore (int index,  object value)
		{
			if (index < 0 || index >= Items.Count)
				return;

			Items[index] = value;
		}

		protected override void SetItemsCore (IList value)
		{

		}

		public void SetSelected (int index, bool value)
		{
			if (index < 0 || index >= Items.Count)
				throw new ArgumentOutOfRangeException ("Index of out range");

			if (SelectionMode == SelectionMode.None)
				throw new InvalidOperationException ();

			if (value)
				SelectItem (index);
			else
    				UnSelectItem (index, true);
		}

		protected virtual void Sort ()
		{
			if (Items.Count == 0)
				return;

			Items.Sort ();
			Refresh ();
		}

		public override string ToString ()
		{
			return base.ToString () + ", Items Count: " + Items.Count;
		}

		protected virtual void WmReflectCommand (ref Message m)
		{

		}

		protected override void WndProc (ref Message m)
		{
			switch ((Msg) m.Msg) {

			case Msg.WM_PAINT: {
				PaintEventArgs	paint_event;
				paint_event = XplatUI.PaintEventStart (Handle);
				OnPaintLB (paint_event);
				XplatUI.PaintEventEnd (Handle);
				return;
			}

			case Msg.WM_ERASEBKGND:
				m.Result = (IntPtr) 1;
				return;

			default:
				break;
			}

			base.WndProc (ref m);
		}

		#endregion Public Methods

		#region Private Methods

		internal void CalcClientArea ()
		{
			listbox_info.textdrawing_rect = listbox_info.client_rect;
			listbox_info.textdrawing_rect.Y += ThemeEngine.Current.DrawListBoxDecorationTop (BorderStyle);
			listbox_info.textdrawing_rect.X += ThemeEngine.Current.DrawListBoxDecorationLeft (BorderStyle);
			listbox_info.textdrawing_rect.Height -= ThemeEngine.Current.DrawListBoxDecorationBottom (BorderStyle);
			listbox_info.textdrawing_rect.Width -= ThemeEngine.Current.DrawListBoxDecorationRight (BorderStyle);

			if (listbox_info.show_verticalsb)
				listbox_info.textdrawing_rect.Width -= vscrollbar_ctrl.Width;

			if (listbox_info.show_horizontalsb)
				listbox_info.textdrawing_rect.Height -= hscrollbar_ctrl.Height;

			//listbox_info.page_size = listbox_info.client_rect.Height / listbox_info.item_height;
			listbox_info.page_size = listbox_info.textdrawing_rect.Height / listbox_info.item_height;

			/* Adjust size to visible the maxim number of displayable items */
			if (IntegralHeight == true) {

				// From MS Docs: The integral height is based on the height of the ListBox, rather than
				// the client area height. As a result, when the IntegralHeight property is set true,
				// items can still be partially shown if scroll bars are displayed.

				int remaining =  (listbox_info.client_rect.Height -
					ThemeEngine.Current.DrawListBoxDecorationBottom (BorderStyle) -
					ThemeEngine.Current.DrawListBoxDecorationBottom (BorderStyle)) %
					listbox_info.item_height;

				if (remaining > 0) {
					listbox_info.client_rect.Height -= remaining;
					CalcClientArea ();
					RellocateScrollBars ();
					Refresh ();
				}
			}

			LBoxInfo.last_item = LastVisibleItem ();

		}

		internal void Draw (Rectangle clip)
		{	
			if (LBoxInfo.textdrawing_rect.Contains (clip) == false) {
				// IntegralHeight has effect, we also have to paint the unused area
				if (ClientRectangle.Height > listbox_info.client_rect.Height) {
					Region area = new Region (ClientRectangle);
					area.Exclude (listbox_info.client_rect);

					DeviceContext.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (Parent.BackColor),
						area.GetBounds (DeviceContext));
				}

				DeviceContext.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), LBoxInfo.textdrawing_rect);				
			}					

			if (Items.Count > 0) {
				Rectangle item_rect;
				DrawItemState state = DrawItemState.None;

				for (int i = LBoxInfo.top_item; i < LBoxInfo.last_item; i++) {
					item_rect = GetItemDisplayRectangle (i, LBoxInfo.top_item);

					if (clip.IntersectsWith (item_rect) == false)
						continue;

					/* Draw item */
					state = DrawItemState.None;

					if ((Items.GetListBoxItem (i)).Selected) {
						state |= DrawItemState.Selected;
					}

					OnDrawItem (new DrawItemEventArgs (DeviceContext, Font, item_rect,
						i, state, ForeColor, BackColor));
				}
			}			
			
			ThemeEngine.Current.DrawListBoxDecorations (DeviceContext, this);
		}

		// Converts a GetItemRectangle to a one that we can display
		internal Rectangle GetItemDisplayRectangle (int index, int first_displayble)
		{
			Rectangle item_rect;
			Rectangle first_item_rect = GetItemRectangle (first_displayble);
			item_rect = GetItemRectangle (index);
			item_rect.X -= first_item_rect.X;
			item_rect.Y -= first_item_rect.Y;

			item_rect.Y += ThemeEngine.Current.DrawListBoxDecorationTop (BorderStyle);
			item_rect.X += ThemeEngine.Current.DrawListBoxDecorationLeft (BorderStyle);
			item_rect.Width -= ThemeEngine.Current.DrawListBoxDecorationRight (BorderStyle);

			return item_rect;
		}

		// Value Changed
		private void HorizontalScrollEvent (object sender, EventArgs e)
		{
			LBoxInfo.top_item = listbox_info.page_size * hscrollbar_ctrl.Value;
			LBoxInfo.last_item = LastVisibleItem ();
			Refresh ();
		}

		// Only returns visible points. The diference of with IndexFromPoint is that the rectangle
		// has screen coordinates
		internal int IndexFromPointDisplayRectangle (int x, int y)
		{
    			for (int i = LBoxInfo.top_item; i < LBoxInfo.last_item; i++) {
				if (GetItemDisplayRectangle (i, LBoxInfo.top_item).Contains (x, y) == true)
					return i;
			}

			return -1;
		}

		private int LastVisibleItem ()
		{
			Rectangle item_rect;
			int top_y = LBoxInfo.textdrawing_rect.Y + LBoxInfo.textdrawing_rect.Height;
			int i = 0;

			for (i = LBoxInfo.top_item; i < Items.Count; i++) {

				item_rect = GetItemDisplayRectangle (i, LBoxInfo.top_item);
				if (MultiColumn) {

					if (item_rect.X > LBoxInfo.textdrawing_rect.Width)
						return i - 1;
				}
				else {
					if (IntegralHeight) {
						if (item_rect.Y + item_rect.Height > top_y) {
							return i;
						}
					}
					else {
						if (item_rect.Y > top_y)
							return i;
					}
				}
			}

			return i;
		}


		internal virtual void OnMouseDownLB (object sender, MouseEventArgs e)
    		{
    			int index = IndexFromPointDisplayRectangle (e.X, e.Y);

    			if (index == -1) return;

    			switch (SelectionMode) {
    				case SelectionMode.None: // Do nothing
    					break;
    				case SelectionMode.One: {
    					SelectedIndex = index;
    					break;
    				}

    				case SelectionMode.MultiSimple: {
					if (selected_index == -1) {
						SelectedIndex = index;
					} else {

						if ((Items.GetListBoxItem (index)).Selected)
							UnSelectItem (index, true);
						else {
    							SelectItem (index);
    							OnSelectedIndexChanged  (new EventArgs ());
    						}
    					}
    					break;
    				}
    				default:
    					break;
    			}
    		}

		private void OnPaintLB (PaintEventArgs pevent)
		{
			if (Width <= 0 || Height <=  0 || Visible == false || suspend_ctrlupdate == true)
    				return;

			/* Copies memory drawing buffer to screen*/
			Draw (pevent.ClipRectangle);
			pevent.Graphics.DrawImage (ImageBuffer, pevent.ClipRectangle, pevent.ClipRectangle, GraphicsUnit.Pixel);

			if (Paint != null)
				Paint (this, pevent);
		}

		internal void RellocateScrollBars ()
		{

			if (listbox_info.show_verticalsb) {

				vscrollbar_ctrl.Size = new Size (vscrollbar_ctrl.Width,
					listbox_info.client_rect.Height - ThemeEngine.Current.DrawListBoxDecorationTop (BorderStyle) -
					ThemeEngine.Current.DrawListBoxDecorationBottom (BorderStyle));

				vscrollbar_ctrl.Location = new Point (listbox_info.client_rect.Width - vscrollbar_ctrl.Width
					- ThemeEngine.Current.DrawListBoxDecorationRight (BorderStyle),
					ThemeEngine.Current.DrawListBoxDecorationTop (BorderStyle));

			}

			if (listbox_info.show_horizontalsb) {

				int width;

				width = listbox_info.client_rect.Width - (ThemeEngine.Current.DrawListBoxDecorationLeft (BorderStyle) + ThemeEngine.Current.DrawListBoxDecorationRight (BorderStyle));

				if (listbox_info.show_verticalsb)
					width -= vscrollbar_ctrl.Width;

				hscrollbar_ctrl.Size = new Size (width, hscrollbar_ctrl.Height);

				hscrollbar_ctrl.Location = new Point (ThemeEngine.Current.DrawListBoxDecorationLeft (BorderStyle),
					listbox_info.client_rect.Height - hscrollbar_ctrl.Height
					- ThemeEngine.Current.DrawListBoxDecorationTop (BorderStyle));
			}

			CalcClientArea ();
		}

		// Add an item in the Selection array and marks it visually as selected
		private void SelectItem (int index)
		{
			if (index == -1)
				return;

			Rectangle invalidate = GetItemDisplayRectangle (index, LBoxInfo.top_item);
			(Items.GetListBoxItem (index)).Selected = true;
    			selected_indices.AddIndex (index);
    			selected_items.AddObject (Items[index]);

    			if (ClientRectangle.Contains (invalidate))
    				Invalidate (invalidate);

		}

		// Removes an item in the Selection array and marks it visually as unselected
		private void UnSelectItem (int index, bool remove)
		{
			if (index == -1)
				return;

			Rectangle invalidate = GetItemDisplayRectangle (index, LBoxInfo.top_item);
			(Items.GetListBoxItem (index)).Selected = false;

			if (remove) {
				selected_indices.RemoveIndex (index);
				selected_items.RemoveObject (Items[index]);
			}


			if (ClientRectangle.Contains (invalidate))
				Invalidate (invalidate);
		}

		private void UpdateFormatString ()
		{
			if (RightToLeft == RightToLeft.No)
				string_format.Alignment = StringAlignment.Near;
			else
				string_format.Alignment = StringAlignment.Far;

			if (UseTabStops)
				string_format.SetTabStops (0, new float [] {(float)(Font.Height * 3.7)});
		}

		// Updates the scrollbar's position with the new items and inside area
		internal virtual void UpdateItemInfo (bool adding, int first, int last)
		{
			if (!IsHandleCreated || suspend_ctrlupdate == true)
				return;

			UpdateShowVerticalScrollBar ();

			if (listbox_info.show_verticalsb && Items.Count > listbox_info.page_size)
				if (vscrollbar_ctrl.Enabled)
					vscrollbar_ctrl.Maximum = Items.Count - listbox_info.page_size;

			if (listbox_info.show_horizontalsb) {
				if (MultiColumn) {
					int fullpage = (listbox_info.page_size * (listbox_info.client_rect.Width / ColumnWidthInternal));

					if (hscrollbar_ctrl.Enabled && listbox_info.page_size > 0)
						hscrollbar_ctrl.Maximum  = 1 + ((Items.Count - fullpage) / listbox_info.page_size);
				}
			}

			if (MultiColumn == false) {
				/* Calc the longest items for non multicolumn listboxes */
				if ((first == -1 && last == -1) || (adding == false)) {

					SizeF size;
					for (int i = 0; i < Items.Count; i++) {
						size = DeviceContext.MeasureString (Items[i].ToString(), Font);

						if ((int) size.Width > listbox_info.max_itemwidth)
							listbox_info.max_itemwidth = (int) size.Width;
					}
				}
				else {
					if (adding) {

						SizeF size;
						for (int i = first; i < last + 1; i++) {
							size = DeviceContext.MeasureString (Items[i].ToString(), Font);

							if ((int) size.Width > listbox_info.max_itemwidth)
								listbox_info.max_itemwidth = (int) size.Width;
						}
					}
				}
			}

			if (sorted)
				Sort ();

			SelectedItems.ReCreate ();
			SelectedIndices.ReCreate ();

			UpdateShowHorizontalScrollBar ();
			Refresh ();
		}

		private void UpdateInternalClientRect (Rectangle client_rectangle)
		{
			listbox_info.client_rect = client_rectangle;
			UpdateShowHorizontalScrollBar ();
			UpdateShowVerticalScrollBar ();
			RellocateScrollBars ();
			UpdateItemInfo (false, -1, -1);
		}

		/* Determines if the horizontal scrollbar has to be displyed */
		private void UpdateShowHorizontalScrollBar ()
		{
			bool show = false;
			bool enabled = true;

			if (MultiColumn) {  /* Horizontal scrollbar is always shown in Multicolum mode */

				/* Is it really need it */
				int page_size = listbox_info.client_rect.Height / listbox_info.item_height;
				int fullpage = (page_size * (listbox_info.textdrawing_rect.Height / ColumnWidthInternal));

				if (Items.Count > fullpage) {
					if (IntegralHeight == false)
						show = true;
				}
				else { /* Acording to MS Documentation ScrollAlwaysVisible only affects Horizontal scrollbars but
					  this is not true for MultiColumn listboxes */
					if (ScrollAlwaysVisible == true) {
						enabled = false;
						show = true;
					}
				}

			} else { /* If large item*/

				if (listbox_info.max_itemwidth > listbox_info.client_rect.Width && HorizontalScrollbar) {
					show = true;
					hscrollbar_ctrl.Maximum = listbox_info.max_itemwidth;
				}
			}

			if (hscrollbar_ctrl.Enabled != enabled)
				hscrollbar_ctrl.Enabled = enabled;

			if (listbox_info.show_horizontalsb == show)
				return;

			listbox_info.show_horizontalsb = show;
			hscrollbar_ctrl.Visible = show;

			if (show == true) {
				RellocateScrollBars ();
			}

			CalcClientArea ();
		}

		/* Determines if the vertical scrollbar has to be displyed */
		private void UpdateShowVerticalScrollBar ()
		{
			bool show = false;
			bool enabled = true;

			if (!MultiColumn) {  /* Vertical scrollbar is never shown in Multicolum mode */
				if (Items.Count > listbox_info.page_size) {
					show = true;
				}
				else
					if (ScrollAlwaysVisible) {
						show = true;
						enabled = false;
					}
			}

			if (vscrollbar_ctrl.Enabled != enabled)
				vscrollbar_ctrl.Enabled = enabled;

			if (listbox_info.show_verticalsb == show)
				return;

			listbox_info.show_verticalsb = show;
			vscrollbar_ctrl.Visible = show;

			if (show == true) {
				if (vscrollbar_ctrl.Enabled)
					vscrollbar_ctrl.Maximum = Items.Count - listbox_info.page_size;

				RellocateScrollBars ();
			}

			CalcClientArea ();
		}

		// Value Changed
		private void VerticalScrollEvent (object sender, EventArgs e)
		{
			LBoxInfo.top_item = /*listbox_info.page_size + */ vscrollbar_ctrl.Value;
			LBoxInfo.last_item = LastVisibleItem ();

			Refresh ();
		}

		#endregion Private Methods

		/*
			ListBox.ObjectCollection
		*/
		public class ObjectCollection : IList, ICollection, IEnumerable
		{
			// Compare objects
			internal class ListObjectComparer : IComparer
			{
				private ListBox owner;
			
				public ListObjectComparer (ListBox owner)
				{
					this.owner = owner;
				}
				
				public int Compare (object a, object b)
				{
					string str1 = a.ToString ();
					string str2 = b.ToString ();					
					return str1.CompareTo (str2);
				}
			}

			// Compare ListItem
			internal class ListItemComparer : IComparer
			{
				private ListBox owner;
			
				public ListItemComparer (ListBox owner)
				{
					this.owner = owner;
				}
				
				public int Compare (object a, object b)
				{
					int index1 = ((ListBox.ListBoxItem) (a)).Index;
					int index2 = ((ListBox.ListBoxItem) (b)).Index;
					string str1 = owner.Items[index1].ToString ();
					string str2 = owner.Items[index2].ToString ();					
					return str1.CompareTo (str2);					
				}
			}

			private ListBox owner;
			internal ArrayList object_items = new ArrayList ();
			internal ArrayList listbox_items = new ArrayList ();

			public ObjectCollection (ListBox owner)
			{
				this.owner = owner;
			}

			public ObjectCollection (ListBox owner, object[] obj)
			{
				this.owner = owner;
				AddRange (obj);
			}

			public ObjectCollection (ListBox owner,  ObjectCollection obj)
			{
				this.owner = owner;
				AddRange (obj);
			}

			#region Public Properties
			public virtual int Count {
				get { return object_items.Count; }
			}

			public virtual bool IsReadOnly {
				get { return false; }
			}

			public virtual object this [int index] {
				get {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("Index of out range");

					return object_items[index];
				}
				set {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("Index of out range");

					object_items[index] = value;
				}
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return false; }
			}

			#endregion Public Properties

			#region Public Methods
			public int Add (object item)
			{
				int idx;

				idx = AddItem (item);
				owner.UpdateItemInfo (true, idx, idx);
				return idx;
			}

			public void AddRange (object[] items)
			{
				int cnt = Count;

				foreach (object mi in items)
					AddItem (mi);

				owner.UpdateItemInfo (true, cnt, Count);
			}

			public void AddRange (ObjectCollection col)
			{
				int cnt = Count;

				foreach (object mi in col)
					AddItem (mi);

				owner.UpdateItemInfo (true, cnt, Count);
			}

			public virtual void Clear ()
			{
				object_items.Clear ();
				listbox_items.Clear ();
				owner.UpdateItemInfo (false, -1, -1);
			}
			public virtual bool Contains (object obj)
			{
				return object_items.Contains (obj);
			}

			public void CopyTo (object[] dest, int arrayIndex)
			{
				object_items.CopyTo (dest, arrayIndex);
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				object_items.CopyTo (dest, index);
			}

			public virtual IEnumerator GetEnumerator ()
			{
				return object_items.GetEnumerator ();
			}

			int IList.Add (object item)
			{
				return Add (item);
			}

			public virtual int IndexOf (object value)
			{
				return object_items.IndexOf (value);
			}

			public virtual void Insert (int index,  object item)
			{
				throw new NotImplementedException ();
			}

			public virtual void Remove (object value)
			{
				RemoveAt (IndexOf (value));
				owner.UpdateItemInfo (false, -1, -1);
			}

			public virtual void RemoveAt (int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				object_items.RemoveAt (index);
				listbox_items.RemoveAt (index);
				owner.UpdateItemInfo (false, -1, -1);
			}
			#endregion Public Methods

			#region Private Methods
			private int AddItem (object item)
			{
				int cnt = object_items.Count;
				object_items.Add (item);
				listbox_items.Add (new ListBox.ListBoxItem (cnt));
				return cnt;
			}

			internal ListBox.ListBoxItem GetListBoxItem (int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				return (ListBox.ListBoxItem) listbox_items[index];
			}

			internal void SetListBoxItem (ListBox.ListBoxItem item, int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				listbox_items[index] = item;
			}

			internal void Sort ()
			{
				/* Keep this order */
				listbox_items.Sort (new ListItemComparer (owner));
				object_items.Sort (new ListObjectComparer (owner));

				for (int i = 0; i < listbox_items.Count; i++) {
					ListBox.ListBoxItem item = GetListBoxItem (i);
					item.Index = i;
				}
			}

			#endregion Private Methods
		}

		/*
			ListBox.SelectedIndexCollection
		*/
		public class SelectedIndexCollection : IList, ICollection, IEnumerable
		{
			private ListBox owner;
			private ArrayList indices = new ArrayList ();

			public SelectedIndexCollection (ListBox owner)
			{
				this.owner = owner;
			}

			#region Public Properties
			public virtual int Count {
				get { return indices.Count; }
			}

			public virtual bool IsReadOnly {
				get { return true; }
			}

			public int this [int index] {
				get {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("Index of out range");

					return (int) indices[index];
				}
			}

			bool ICollection.IsSynchronized {
				get { return true; }
			}

			bool IList.IsFixedSize{
				get { return true; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			#endregion Public Properties

			#region Public Methods
			public bool Contains (int selectedIndex)
			{
				return indices.Contains (selectedIndex);
			}

			public virtual void CopyTo (Array dest, int index)
			{
				indices.CopyTo (dest, index);
			}

			public virtual IEnumerator GetEnumerator ()
			{
				return indices.GetEnumerator ();
			}

			int IList.Add (object obj)
			{
				throw new NotSupportedException ();
			}

			void IList.Clear ()
			{
				throw new NotSupportedException ();
			}

			bool IList.Contains (object selectedIndex)
			{
				return Contains ((int)selectedIndex);
			}

			int IList.IndexOf (object selectedIndex)
			{
				return IndexOf ((int) selectedIndex);
			}

			void IList.Insert (int index, object value)
			{
				throw new NotSupportedException ();
			}

			void IList.Remove (object value)
			{
				throw new NotSupportedException ();
			}

			void IList.RemoveAt (int index)
			{
				throw new NotSupportedException ();
			}

			object IList.this[int index]{
				get {return indices[index]; }
				set {throw new NotImplementedException (); }
			}

			public int IndexOf (int selectedIndex)
			{
				return indices.IndexOf (selectedIndex);
			}
			#endregion Public Methods

			#region Private Methods

			internal void AddIndex (int index)
			{
				indices.Add (index);
			}

			internal void ClearIndices ()
			{
				indices.Clear ();
			}

			internal void RemoveIndex (int index)
			{
				indices.Remove (index);
			}

			internal void ReCreate ()
			{
				indices.Clear ();

				for (int i = 0; i < owner.Items.Count; i++) {
					ListBox.ListBoxItem item = owner.Items.GetListBoxItem (i);

					if (item.Selected)
						indices.Add (item.Index);
				}
			}

			#endregion Private Methods
		}

		/*
			SelectedObjectCollection
		*/
		public class SelectedObjectCollection : IList, ICollection, IEnumerable
		{
			private ListBox owner;
			private ArrayList object_items = new ArrayList ();

			public SelectedObjectCollection (ListBox owner)
			{
				this.owner = owner;
			}

			#region Public Properties
			public virtual int Count {
				get { return object_items.Count; }
			}

			public virtual bool IsReadOnly {
				get { return true; }
			}

			public virtual object this [int index] {
				get {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("Index of out range");

					return object_items[index];
				}
				set {throw new NotSupportedException ();}
			}

			bool ICollection.IsSynchronized {
				get { return true; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return true; }
			}

			object IList.this[int index] {
				get { return object_items[index]; }
				set { throw new NotSupportedException (); }
			}

			#endregion Public Properties

			#region Public Methods
			public virtual bool Contains (object selectedObject)
			{
				return object_items.Contains (selectedObject);
			}

			public virtual void CopyTo (Array dest, int index)
			{
				object_items.CopyTo (dest, index);
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ();
			}

			void IList.Clear ()
			{
				throw new NotSupportedException ();
			}

			bool IList.Contains (object selectedIndex)
			{
				throw new NotImplementedException ();
			}

			int IList.IndexOf (object selectedIndex)
			{
				return IndexOf ((int) selectedIndex);
			}
	
			void IList.Insert (int index, object value)
			{
				throw new NotSupportedException ();
			}

			void IList.Remove (object value)
			{
				throw new NotSupportedException ();
			}

			void IList.RemoveAt (int index)
			{
				throw new NotSupportedException ();
			}
	
			public int IndexOf (int selectedIndex)
			{
				return object_items.IndexOf (selectedIndex);
			}

			public virtual IEnumerator GetEnumerator ()
			{
				return object_items.GetEnumerator ();
			}

			#endregion Public Methods

			#region Private Methods
			internal void AddObject (object obj)
			{
				object_items.Add (obj);
			}

			internal void ClearObjects ()
			{
				object_items.Clear ();
			}

			internal void ReCreate ()
			{
				object_items.Clear ();

				for (int i = 0; i < owner.Items.Count; i++) {
					ListBox.ListBoxItem item = owner.Items.GetListBoxItem (i);

					if (item.Selected)
						object_items.Add (owner.Items[item.Index]);
				}
			}

			internal void RemoveObject (object obj)
			{
				object_items.Remove (obj);
			}



			#endregion Private Methods

		}

	}
}

