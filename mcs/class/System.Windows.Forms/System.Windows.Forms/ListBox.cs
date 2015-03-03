/// Permission is hereby granted, free of charge, to any person obtaining
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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//	Mike Kestner  <mkestner@novell.com>
//

// COMPLETE

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace System.Windows.Forms
{
	[DefaultProperty("Items")]
	[DefaultEvent("SelectedIndexChanged")]
	[Designer ("System.Windows.Forms.Design.ListBoxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DefaultBindingProperty ("SelectedValue")]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public class ListBox : ListControl
	{
		public const int DefaultItemHeight = 13;
		public const int NoMatches = -1;
		
		internal enum ItemNavigation
		{
			First,
			Last,
			Next,
			Previous,
			NextPage,
			PreviousPage,
			PreviousColumn,
			NextColumn
		}
		
		Hashtable item_heights;
		private int item_height = -1;
		private int column_width = 0;
		private int requested_height;
		private DrawMode draw_mode = DrawMode.Normal;
		private int horizontal_extent = 0;
		private bool horizontal_scrollbar = false;
		private bool integral_height = true;
		private bool multicolumn = false;
		private bool scroll_always_visible = false;
		private SelectedIndexCollection selected_indices;
		private SelectedObjectCollection selected_items;
		private SelectionMode selection_mode = SelectionMode.One;
		private bool sorted = false;
		private bool use_tabstops = true;
		private int column_width_internal = 120;
		private ImplicitVScrollBar vscrollbar;
		private ImplicitHScrollBar hscrollbar;
		private int hbar_offset;
		private bool suspend_layout;
		private bool ctrl_pressed = false;
		private bool shift_pressed = false;
		private bool explicit_item_height = false;
		private int top_index = 0;
		private int last_visible_index = 0;
		private Rectangle items_area;
		private int focused_item = -1;
		private ObjectCollection items;
		private IntegerCollection custom_tab_offsets;
		private Padding padding;
		private bool use_custom_tab_offsets;

		public ListBox ()
		{
			items = CreateItemCollection ();
			selected_indices = new SelectedIndexCollection (this);
			selected_items = new SelectedObjectCollection (this);

			requested_height = bounds.Height;
			InternalBorderStyle = BorderStyle.Fixed3D;
			BackColor = ThemeEngine.Current.ColorWindow;

			/* Vertical scrollbar */
			vscrollbar = new ImplicitVScrollBar ();
			vscrollbar.Minimum = 0;
			vscrollbar.SmallChange = 1;
			vscrollbar.LargeChange = 1;
			vscrollbar.Maximum = 0;
			vscrollbar.ValueChanged += new EventHandler (VerticalScrollEvent);
			vscrollbar.Visible = false;

			/* Horizontal scrollbar */
			hscrollbar = new ImplicitHScrollBar ();
			hscrollbar.Minimum = 0;
			hscrollbar.SmallChange = 1;
			hscrollbar.LargeChange = 1;
			hscrollbar.Maximum = 0;
			hscrollbar.Visible = false;
			hscrollbar.ValueChanged += new EventHandler (HorizontalScrollEvent);

			Controls.AddImplicit (vscrollbar);
			Controls.AddImplicit (hscrollbar);

			/* Events */
			MouseDown += new MouseEventHandler (OnMouseDownLB);
			MouseMove += new MouseEventHandler (OnMouseMoveLB);
			MouseUp += new MouseEventHandler (OnMouseUpLB);
			MouseWheel += new MouseEventHandler (OnMouseWheelLB);
			KeyUp += new KeyEventHandler (OnKeyUpLB);
			GotFocus += new EventHandler (OnGotFocus);
			LostFocus += new EventHandler (OnLostFocus);
			
			SetStyle (ControlStyles.UserPaint, false);

			custom_tab_offsets = new IntegerCollection (this);
		}

		#region Events
		static object DrawItemEvent = new object ();
		static object MeasureItemEvent = new object ();
		static object SelectedIndexChangedEvent = new object ();

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler Click {
			add { base.Click += value; }
			remove { base.Click -= value; }
		}

		public event DrawItemEventHandler DrawItem {
			add { Events.AddHandler (DrawItemEvent, value); }
			remove { Events.RemoveHandler (DrawItemEvent, value); }
		}

		public event MeasureItemEventHandler MeasureItem {
			add { Events.AddHandler (MeasureItemEvent, value); }
			remove { Events.RemoveHandler (MeasureItemEvent, value); }
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event MouseEventHandler MouseClick {
			add { base.MouseClick += value; }
			remove { base.MouseClick -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler PaddingChanged {
			add { base.PaddingChanged += value; }
			remove { base.PaddingChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}

		public event EventHandler SelectedIndexChanged {
			add { Events.AddHandler (SelectedIndexChangedEvent, value); }
			remove { Events.RemoveHandler (SelectedIndexChangedEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}
		#endregion // Events

		#region UIA Framework Events 
		//NOTE:
		//	We are using Reflection to add/remove internal events.
		//	Class ListProvider uses the events.
		//
		//Event used to generate UIA Selection Pattern 
		static object UIASelectionModeChangedEvent = new object ();

		internal event EventHandler UIASelectionModeChanged {
			add { Events.AddHandler (UIASelectionModeChangedEvent, value); }
			remove { Events.RemoveHandler (UIASelectionModeChangedEvent, value); }
		}

		internal void OnUIASelectionModeChangedEvent ()
		{
			EventHandler eh = (EventHandler) Events [UIASelectionModeChangedEvent];
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		static object UIAFocusedItemChangedEvent = new object ();

		internal event EventHandler UIAFocusedItemChanged {
			add { Events.AddHandler (UIAFocusedItemChangedEvent, value); }
			remove { Events.RemoveHandler (UIAFocusedItemChangedEvent, value); }
		}

		internal void OnUIAFocusedItemChangedEvent ()
		{
			EventHandler eh = (EventHandler) Events [UIAFocusedItemChangedEvent];
			if (eh != null)
				eh (this, EventArgs.Empty);
		}
		#endregion UIA Framework Events 

		#region Public Properties
		public override Color BackColor {
			get { return base.BackColor; }
			set {
				if (base.BackColor == value)
					return;

    				base.BackColor = value;
				base.Refresh ();	// Careful. Calling the base method is not the same that calling 
			}				// the overriden one that refresh also all the items
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { 
    				base.BackgroundImage = value;
				base.Refresh ();
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}

		[DefaultValue (BorderStyle.Fixed3D)]
		[DispId(-504)]
		public BorderStyle BorderStyle {
			get { return InternalBorderStyle; }
			set { 
				InternalBorderStyle = value; 
				UpdateListBoxBounds ();
			}
		}

		[DefaultValue (0)]
		[Localizable (true)]
		public int ColumnWidth {
			get { return column_width; }
			set {
				if (value < 0)
					throw new ArgumentException ("A value less than zero is assigned to the property.");

				column_width = value;

				if (value == 0)
					ColumnWidthInternal = 120;
				else
					ColumnWidthInternal = value;

				base.Refresh ();
			}
		}

		protected override CreateParams CreateParams {
			get { return base.CreateParams;}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public IntegerCollection CustomTabOffsets {
			get { return custom_tab_offsets; }
		}

		protected override Size DefaultSize {
			get { return new Size (120, 96); }
		}

		[RefreshProperties(RefreshProperties.Repaint)]
		[DefaultValue (DrawMode.Normal)]
		public virtual DrawMode DrawMode {
			get { return draw_mode; }
			set {
				if (!Enum.IsDefined (typeof (DrawMode), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for DrawMode", value));
					
				if (value == DrawMode.OwnerDrawVariable && multicolumn == true)
					throw new ArgumentException ("Cannot have variable height and multicolumn");

				if (draw_mode == value)
					return;

				draw_mode = value;

				if (draw_mode == DrawMode.OwnerDrawVariable)
					item_heights = new Hashtable ();
				else
					item_heights = null;

				if (Parent != null)
					Parent.PerformLayout (this, "DrawMode");
				base.Refresh ();
			}
		}

		public override Font Font {
			get { return base.Font; }
			set { base.Font = value; }
		}

		public override Color ForeColor {
			get { return base.ForeColor; }
			set {
				if (base.ForeColor == value)
					return;

				base.ForeColor = value;
				base.Refresh ();
			}
		}

		[DefaultValue (0)]
		[Localizable (true)]
		public int HorizontalExtent {
			get { return horizontal_extent; }
			set {
				if (horizontal_extent == value)
					return;

				horizontal_extent = value;
				base.Refresh ();
			}
		}

		[DefaultValue (false)]
		[Localizable (true)]
		public bool HorizontalScrollbar {
			get { return horizontal_scrollbar; }
			set {
				if (horizontal_scrollbar == value)
					return;

				horizontal_scrollbar = value;
				UpdateScrollBars ();
				base.Refresh ();
			}
		}

		[DefaultValue (true)]
		[Localizable (true)]
		[RefreshProperties(RefreshProperties.Repaint)]
		public bool IntegralHeight {
			get { return integral_height; }
			set {
				if (integral_height == value)
					return;

				integral_height = value;
				UpdateListBoxBounds ();
			}
		}

		[DefaultValue (13)]
		[Localizable (true)]
		[RefreshProperties(RefreshProperties.Repaint)]
		public virtual int ItemHeight {
			get {
				if (item_height == -1) {
					SizeF sz = TextRenderer.MeasureString ("The quick brown Fox", Font);
					item_height = (int) sz.Height;
				}
				return item_height;
			}
			set {
				if (value > 255)
					throw new ArgumentOutOfRangeException ("The ItemHeight property was set beyond 255 pixels");

				explicit_item_height = true;
				if (item_height == value)
					return;

				item_height = value;
				if (IntegralHeight)
					UpdateListBoxBounds ();
				LayoutListBox ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Localizable (true)]
		[Editor ("System.Windows.Forms.Design.ListControlStringCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[MergableProperty (false)]
		public ObjectCollection Items {
			get { return items; }
		}

		[DefaultValue (false)]
		public bool MultiColumn {
			get { return multicolumn; }
			set {
				if (multicolumn == value)
					return;

				if (value == true && DrawMode == DrawMode.OwnerDrawVariable)
					throw new ArgumentException ("A multicolumn ListBox cannot have a variable-sized height.");
					
				multicolumn = value;
				LayoutListBox ();
				Invalidate ();
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Padding Padding {
			get { return padding; }
			set { padding = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public int PreferredHeight {
			get {
				int itemsHeight = 0;
				if (draw_mode == DrawMode.Normal)
					itemsHeight = FontHeight * items.Count;
				else if (draw_mode == DrawMode.OwnerDrawFixed)
					itemsHeight = ItemHeight * items.Count;
				else if (draw_mode == DrawMode.OwnerDrawVariable) {
					for (int i = 0; i < items.Count; i++)
						itemsHeight += (int) item_heights [Items [i]];
				}
				
				return itemsHeight;
			}
		}

		public override RightToLeft RightToLeft {
			get { return base.RightToLeft; }
			set {
				base.RightToLeft = value;
				if (base.RightToLeft == RightToLeft.Yes)
					StringFormat.Alignment = StringAlignment.Far;
				else
					StringFormat.Alignment = StringAlignment.Near;
				base.Refresh ();
			}
		}

		// Only affects the Vertical ScrollBar
		[DefaultValue (false)]
		[Localizable (true)]
		public bool ScrollAlwaysVisible {
			get { return scroll_always_visible; }
			set {
				if (scroll_always_visible == value)
					return;

				scroll_always_visible = value;
				UpdateScrollBars ();
			}
		}

		[Bindable(true)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override int SelectedIndex {
			get { 
				if (selected_indices == null)
					return -1;
					
				return selected_indices.Count > 0 ? selected_indices [0] : -1;
			}
			set {
				if (value < -1 || value >= Items.Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				if (SelectionMode == SelectionMode.None)
					throw new ArgumentException ("cannot call this method if SelectionMode is SelectionMode.None");

				if (value == -1)
					selected_indices.Clear ();
				else
					selected_indices.Add (value);
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public SelectedIndexCollection SelectedIndices {
			get { return selected_indices; }
		}

		[Bindable(true)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public object SelectedItem {
			get {
				if (SelectedItems.Count > 0)
					return SelectedItems[0];
				else
					return null;
			}
			set {
				if (value != null && !Items.Contains (value))
					return; // FIXME: this is probably an exception
					
				SelectedIndex = value == null ? - 1 : Items.IndexOf (value);
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public SelectedObjectCollection SelectedItems {
			get {return selected_items;}
		}

		[DefaultValue (SelectionMode.One)]
		public virtual SelectionMode SelectionMode {
			get { return selection_mode; }
			set {
				if (!Enum.IsDefined (typeof (SelectionMode), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for SelectionMode", value));

				if (selection_mode == value)
					return;
					
				selection_mode = value;
					
				switch (selection_mode) {
				case SelectionMode.None: 
					SelectedIndices.Clear ();
					break;

				case SelectionMode.One:
					// FIXME: Probably this can be improved
					ArrayList old_selection = (ArrayList) SelectedIndices.List.Clone ();
					for (int i = 1; i < old_selection.Count; i++)
						SelectedIndices.Remove ((int)old_selection [i]);
					break;

				default:
					break;
				}

				// UIA Framework: Generates SelectionModeChanged event.
				OnUIASelectionModeChangedEvent ();
			}
		}

		[DefaultValue (false)]
		public bool Sorted {
			get { return sorted; }
			set {
				if (sorted == value)
					return;

				sorted = value;
				if (sorted)
					Sort ();
			}
		}

		[Bindable (false)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public override string Text {
			get {
				if (SelectionMode != SelectionMode.None && SelectedIndex != -1)
					return GetItemText (SelectedItem);

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

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int TopIndex {
			get { return top_index; }
			set {
				if (value == top_index)
					return;

				if (value < 0 || value >= Items.Count)
					return;

				int page_size = (items_area.Height / ItemHeight);
				
				if (Items.Count < page_size)
					value = 0;
				else if (!multicolumn)
					top_index = Math.Min (value, Items.Count - page_size);
				else
					top_index = value;
					
				UpdateTopItem ();
				base.Refresh ();
			}
		}

		[Browsable (false)]
		[DefaultValue (false)]
		public bool UseCustomTabOffsets {
			get { return use_custom_tab_offsets; }
			set { 
				if (use_custom_tab_offsets != value) {
					use_custom_tab_offsets = value;
					CalculateTabStops ();
				}
			 }
		}

		[DefaultValue (true)]
		public bool UseTabStops {
			get { return use_tabstops; }
			set {
				if (use_tabstops == value)
					return;

				use_tabstops = value;
				CalculateTabStops ();
			}
		}

		protected override bool AllowSelection {
			get {
				return SelectionMode != SelectionMode.None;
			}
		}
		#endregion Public Properties

		#region Private Properties

		private int ColumnWidthInternal {
			get { return column_width_internal; }
			set { column_width_internal = value; }
		}

		private int row_count = 1;
		private int RowCount {
			get {
				return MultiColumn ? row_count : Items.Count;
			}
		}

		#endregion Private Properties

		#region UIA Framework Properties

		internal ScrollBar UIAHScrollBar {
			get { return hscrollbar; }
		}

		internal ScrollBar UIAVScrollBar {
			get { return vscrollbar; }
		}

		#endregion UIA Framework Properties

		#region Public Methods
		[Obsolete ("this method has been deprecated")]
		protected virtual void AddItemsCore (object[] value)
		{
			Items.AddRange (value);
		}

		public void BeginUpdate ()
		{
			suspend_layout = true;
		}

		public void ClearSelected ()
		{
			selected_indices.Clear ();
		}

		protected virtual ObjectCollection CreateItemCollection ()
		{
			return new ObjectCollection (this);
		}

		public void EndUpdate ()
		{
			suspend_layout = false;
			LayoutListBox ();
			base.Refresh ();
		}

		public int FindString (String s)
		{
			return FindString (s, -1);
		}

		public int FindString (string s,  int startIndex)
		{
			if (Items.Count == 0)
				return -1; // No exception throwing if empty

			if (startIndex < -1 || startIndex >= Items.Count)
				throw new ArgumentOutOfRangeException ("Index of out range");

			startIndex = (startIndex == Items.Count - 1) ? 0 : startIndex + 1;

			int i = startIndex;
			while (true) {
				string text = GetItemText (Items [i]);
				if (CultureInfo.CurrentCulture.CompareInfo.IsPrefix (text, s,
						CompareOptions.IgnoreCase))
					return i;

				i = (i == Items.Count - 1) ? 0 : i + 1;
				if (i == startIndex)
					break;
			}

			return NoMatches;
		}

		public int FindStringExact (string s)
		{
			return FindStringExact (s, -1);
		}

		public int FindStringExact (string s,  int startIndex)
		{
			if (Items.Count == 0)
				return -1; // No exception throwing if empty

			if (startIndex < -1 || startIndex >= Items.Count)
				throw new ArgumentOutOfRangeException ("Index of out range");

			startIndex = (startIndex + 1 == Items.Count) ? 0 : startIndex + 1;

			int i = startIndex;
			while (true) {
				if (String.Compare (GetItemText (Items[i]), s, true) == 0)
					return i;

				i = (i + 1 == Items.Count) ? 0 : i + 1;
				if (i == startIndex)
					break;
			}

			return NoMatches;
		}

		public int GetItemHeight (int index)
		{
			if (index < 0 || index >= Items.Count)
				throw new ArgumentOutOfRangeException ("Index of out range");
				
			if (DrawMode == DrawMode.OwnerDrawVariable && IsHandleCreated == true) {
				
				object o = Items [index];
				if (item_heights.Contains (o))
					return (int) item_heights [o];
				
				MeasureItemEventArgs args = new MeasureItemEventArgs (DeviceContext, index, ItemHeight);
				OnMeasureItem (args);
				item_heights [o] = args.ItemHeight;
				return args.ItemHeight;
			}

			return ItemHeight;
		}

		public Rectangle GetItemRectangle (int index)
		{
			if (index < 0 || index >= Items.Count)
				throw new  ArgumentOutOfRangeException ("GetItemRectangle index out of range.");

			Rectangle rect = new Rectangle ();

			if (MultiColumn) {
				int col = index / RowCount;
				int y = index;
				if (y < 0) // We convert it to valid positive value 
					y += RowCount * (top_index / RowCount);
				rect.Y = (y % RowCount) * ItemHeight;
				rect.X = (col - (top_index / RowCount)) * ColumnWidthInternal;
				rect.Height = ItemHeight;
				rect.Width = ColumnWidthInternal;
			} else {
				rect.X = 0;
				rect.Height = GetItemHeight (index);
				rect.Width = items_area.Width;
				
				if (DrawMode == DrawMode.OwnerDrawVariable) {
					rect.Y = 0;
					if (index >= top_index) {
						for (int i = top_index; i < index; i++) {
							rect.Y += GetItemHeight (i);
						}
					} else {
						for (int i = index; i < top_index; i++) {
							rect.Y -= GetItemHeight (i);
						}
					}
				} else {
					rect.Y = ItemHeight * (index - top_index);	
				}
			}

			if (this is CheckedListBox)
				rect.Width += 15;
				
			return rect;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override Rectangle GetScaledBounds (Rectangle bounds, SizeF factor, BoundsSpecified specified)
		{
			bounds.Height = requested_height;

			return base.GetScaledBounds (bounds, factor, specified);
		}

		public bool GetSelected (int index)
		{
			if (index < 0 || index >= Items.Count)
				throw new ArgumentOutOfRangeException ("Index of out range");

			return SelectedIndices.Contains (index);
		}

		public int IndexFromPoint (Point p)
		{
			return IndexFromPoint (p.X, p.Y);
		}

		// Only returns visible points
		public int IndexFromPoint (int x, int y)
		{

			if (Items.Count == 0) {
				return -1;
			}

			for (int i = top_index; i <= last_visible_index; i++) {
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
			BindDataItems ();
			
			if (DataSource == null || DataManager == null) {
				SelectedIndex = -1;
			} else {
				SelectedIndex = DataManager.Position;
			}
		}

		protected override void OnDisplayMemberChanged (EventArgs e)
		{
			base.OnDisplayMemberChanged (e);

			if (DataManager == null || !IsHandleCreated)
				return;

			BindDataItems ();
			base.Refresh ();
		}

		protected virtual void OnDrawItem (DrawItemEventArgs e)
		{			
			switch (DrawMode) {
			case DrawMode.OwnerDrawFixed:
			case DrawMode.OwnerDrawVariable:
				DrawItemEventHandler eh = (DrawItemEventHandler)(Events [DrawItemEvent]);
				if (eh != null)
					eh (this, e);

				break;

			default:
				ThemeEngine.Current.DrawListBoxItem (this, e);
				break;
			}
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);

			if (use_tabstops)
				StringFormat.SetTabStops (0, new float [] {(float)(Font.Height * 3.7)});

			if (explicit_item_height) {
				base.Refresh ();
			} else {
				SizeF sz = TextRenderer.MeasureString ("The quick brown Fox", Font);
				item_height = (int) sz.Height;
				if (IntegralHeight)
					UpdateListBoxBounds ();
				LayoutListBox ();
			}
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);

			if (IntegralHeight)
				UpdateListBoxBounds ();

			LayoutListBox ();
			EnsureVisible (focused_item);
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected virtual void OnMeasureItem (MeasureItemEventArgs e)
		{
			if (draw_mode != DrawMode.OwnerDrawVariable)
				return;

			MeasureItemEventHandler eh = (MeasureItemEventHandler)(Events [MeasureItemEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnParentChanged (EventArgs e)
		{
			base.OnParentChanged (e);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
			if (canvas_size.IsEmpty || MultiColumn)
				LayoutListBox ();
				
			Invalidate ();
		}

		protected override void OnSelectedIndexChanged (EventArgs e)
		{
			base.OnSelectedIndexChanged (e);

			EventHandler eh = (EventHandler)(Events [SelectedIndexChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnSelectedValueChanged (EventArgs e)
		{
			base.OnSelectedValueChanged (e);
		}

		public override void Refresh ()
		{
			if (draw_mode == DrawMode.OwnerDrawVariable)
				item_heights.Clear ();
			
			base.Refresh ();
		}

		protected override void RefreshItem (int index)
		{
			if (index < 0 || index >= Items.Count)
				throw new ArgumentOutOfRangeException ("Index of out range");
				
			if (draw_mode == DrawMode.OwnerDrawVariable)
				item_heights.Remove (Items [index]);
		}

		protected override void RefreshItems ()
		{
			for (int i = 0; i < Items.Count; i++) {
				RefreshItem (i);
			}

			LayoutListBox ();
			Refresh ();
		}

		public override void ResetBackColor ()
		{
			base.ResetBackColor ();
		}

		public override void ResetForeColor ()
		{
			base.ResetForeColor ();
		}

		protected override void ScaleControl (SizeF factor, BoundsSpecified specified)
		{
			base.ScaleControl (factor, specified);
		}

		private int SnapHeightToIntegral (int height)
		{
			int border;

			switch (border_style) {
			case BorderStyle.Fixed3D:
				border = ThemeEngine.Current.Border3DSize.Height;
				break;
			case BorderStyle.FixedSingle:
				border = ThemeEngine.Current.BorderSize.Height;
				break;
			case BorderStyle.None:
			default:
				border = 0;
				break;
			}

			height -= (2 * border);
			height -= height % ItemHeight;
			height += (2 * border);

			return height;
		}
		
		protected override void SetBoundsCore (int x,  int y, int width, int height, BoundsSpecified specified)
		{
			if ((specified & BoundsSpecified.Height) == BoundsSpecified.Height)
				requested_height = height;

			if (IntegralHeight && IsHandleCreated)
				height = SnapHeightToIntegral (height);

			base.SetBoundsCore (x, y, width, height, specified);
			UpdateScrollBars ();
			last_visible_index = LastVisibleItem ();
		}

		protected override void SetItemCore (int index,  object value)
		{
			if (index < 0 || index >= Items.Count)
				return;

			Items[index] = value;
		}

		protected override void SetItemsCore (IList value)
		{
			BeginUpdate ();
			try {
				Items.Clear ();
				Items.AddItems (value);
			} finally {
				EndUpdate ();
			}
		}

		public void SetSelected (int index, bool value)
		{
			if (index < 0 || index >= Items.Count)
				throw new ArgumentOutOfRangeException ("Index of out range");

			if (SelectionMode == SelectionMode.None)
				throw new InvalidOperationException ();

			if (value)
				SelectedIndices.Add (index);
			else
				SelectedIndices.Remove (index);
		}

		protected virtual void Sort ()
		{
			Sort (true);
		}

		//
		// Sometimes we could need to Sort, and request a Refresh
		// in a different place, to not have the painting done twice
		//
		void Sort (bool paint)
		{
			if (Items.Count == 0)
				return;

			Items.Sort ();

			if (paint)
				base.Refresh ();
		}

		public override string ToString ()
		{
			return base.ToString ();
		}

		protected virtual void WmReflectCommand (ref Message m)
		{
		}

		protected override void WndProc (ref Message m)
		{
			if ((Msg)m.Msg == Msg.WM_KEYDOWN) {
				if (ProcessKeyMessage (ref m))
					m.Result = IntPtr.Zero;
				else {
					HandleKeyDown ((Keys)m.WParam.ToInt32 ());
					DefWndProc (ref m);
				}

				return;
			}

			base.WndProc (ref m);
		}

		#endregion Public Methods

		#region Private Methods

		private void CalculateTabStops ()
		{
			if (use_tabstops) {
				if (use_custom_tab_offsets) {
					float[] f = new float[custom_tab_offsets.Count];
					custom_tab_offsets.CopyTo (f, 0);
					StringFormat.SetTabStops (0, f);
				}
				else
					StringFormat.SetTabStops (0, new float[] { (float)(Font.Height * 3.7) });
			} else
				StringFormat.SetTabStops (0, new float[0]);

			this.Invalidate ();
		}

		private Size canvas_size;

		private void LayoutListBox ()
		{
			if (!IsHandleCreated || suspend_layout)
				return;

			if (MultiColumn)
				LayoutMultiColumn ();
			else
				LayoutSingleColumn ();

			last_visible_index = LastVisibleItem ();
			UpdateScrollBars ();
		}

		private void LayoutSingleColumn ()
		{
			int height, width;

			switch (DrawMode) {
			case DrawMode.OwnerDrawVariable:
				height = 0;
				width = HorizontalExtent;
				for (int i = 0; i < Items.Count; i++) {
					height += GetItemHeight (i);
				}
				break;

			case DrawMode.OwnerDrawFixed:
				height = Items.Count * ItemHeight;
				width = HorizontalExtent;
				break;

			case DrawMode.Normal:
			default:
				height = Items.Count * ItemHeight;
				width = 0;
				for (int i = 0; i < Items.Count; i++) {
					SizeF sz = TextRenderer.MeasureString (GetItemText (Items[i]), Font);
					int t = (int)sz.Width;
					
					if (this is CheckedListBox)
						t += 15;
						
					if (t > width)
						width = t;
				}
				break;
			}

			canvas_size = new Size (width, height);
		}

		private void LayoutMultiColumn ()
		{
			int usable_height = ClientRectangle.Height - (ScrollAlwaysVisible ? hscrollbar.Height : 0);
			row_count = Math.Max (1, usable_height / ItemHeight);

			int cols = (int) Math.Ceiling ((float)Items.Count / (float) row_count);
			Size sz = new Size (cols * ColumnWidthInternal, row_count * ItemHeight);
			if (!ScrollAlwaysVisible && sz.Width > ClientRectangle.Width && row_count > 1) {
				usable_height = ClientRectangle.Height - hscrollbar.Height;
				row_count = Math.Max (1, usable_height / ItemHeight);
				cols = (int) Math.Ceiling ((float)Items.Count / (float) row_count);
				sz = new Size (cols * ColumnWidthInternal, row_count * ItemHeight);
			}
			canvas_size = sz;
		}

		internal void Draw (Rectangle clip, Graphics dc)
		{
			Theme theme = ThemeEngine.Current;

			if (hscrollbar.Visible && vscrollbar.Visible) {
				// Paint the dead space in the bottom right corner
				Rectangle rect = new Rectangle (hscrollbar.Right, vscrollbar.Bottom, vscrollbar.Width, hscrollbar.Height);
				if (rect.IntersectsWith (clip))
					dc.FillRectangle (theme.ResPool.GetSolidBrush (theme.ColorControl), rect);
			}

			dc.FillRectangle (theme.ResPool.GetSolidBrush (BackColor), items_area);

			if (Items.Count == 0)
				return;

			for (int i = top_index; i <= last_visible_index; i++) {
				Rectangle rect = GetItemDisplayRectangle (i, top_index);

				if (!clip.IntersectsWith (rect))
					continue;

				DrawItemState state = DrawItemState.None;

				if (SelectedIndices.Contains (i))
					state |= DrawItemState.Selected;
					
				if (has_focus && FocusedItem == i)
					state |= DrawItemState.Focus;
					
				if (MultiColumn == false && hscrollbar != null && hscrollbar.Visible) {
					rect.X -= hscrollbar.Value;
					rect.Width += hscrollbar.Value;
				}

				Color fore_color = !Enabled ? ThemeEngine.Current.ColorGrayText :
					(state & DrawItemState.Selected) != 0 ? ThemeEngine.Current.ColorHighlightText : ForeColor;
				OnDrawItem (new DrawItemEventArgs (dc, Font, rect, i, state, fore_color, BackColor));
			}
		}

		// Converts a GetItemRectangle to a one that we can display
		internal Rectangle GetItemDisplayRectangle (int index, int first_displayble)
		{
			Rectangle item_rect;
			Rectangle first_item_rect = GetItemRectangle (first_displayble);
			item_rect = GetItemRectangle (index);
			item_rect.X -= first_item_rect.X;
			item_rect.Y -= first_item_rect.Y;
			
			// Subtract the checkboxes from the width
			if (this is CheckedListBox)
				item_rect.Width -= 14;

			return item_rect;
		}

		// Value Changed
		private void HorizontalScrollEvent (object sender, EventArgs e)
		{
			if (multicolumn) {
				int top_item = top_index;
				int last_item = last_visible_index;

				top_index = RowCount * hscrollbar.Value;
				last_visible_index = LastVisibleItem ();

				if (top_item != top_index || last_item != last_visible_index)
					Invalidate (items_area);
			}
			else {
				int old_offset = hbar_offset;
				hbar_offset = hscrollbar.Value;

				if (hbar_offset < 0)
					hbar_offset = 0;

				if (IsHandleCreated) {
					XplatUI.ScrollWindow (Handle, items_area, old_offset - hbar_offset, 0, false);

					// Invalidate the previous selection border, to keep it properly updated.
					Rectangle selection_border_area = new Rectangle (items_area.Width - (hbar_offset - old_offset) - 3, 0, 
							3, items_area.Height);
					Invalidate (selection_border_area);
				}
			}
		}

		// Only returns visible points. The diference of with IndexFromPoint is that the rectangle
		// has screen coordinates
		private int IndexAtClientPoint (int x, int y)
		{	
			if (Items.Count == 0)
				return -1;
			
			if (x < 0)
				x = 0;
			else if (x > ClientRectangle.Right)
				x = ClientRectangle.Right;

			if (y < 0)
				y = 0;
			else if (y > ClientRectangle.Bottom)
				y = ClientRectangle.Bottom;

			for (int i = top_index; i <= last_visible_index; i++)
				if (GetItemDisplayRectangle (i, top_index).Contains (x, y))
					return i;

			return -1;
		}

		internal override bool IsInputCharInternal (char charCode)
		{
			return true;
		}

		private int LastVisibleItem ()
		{
			Rectangle item_rect;
			int top_y = items_area.Y + items_area.Height;
			int i = 0;

			if (top_index >= Items.Count)
				return top_index;

			for (i = top_index; i < Items.Count; i++) {
				item_rect = GetItemDisplayRectangle (i, top_index);
				if (MultiColumn) {
					if (item_rect.X > items_area.Width)
						return i - 1;
				} else {
					if (item_rect.Y + item_rect.Height > top_y)
						return i;
				}
			}
			return i - 1;
		}

		private void UpdateTopItem ()
		{
			if (MultiColumn) {
				int col = top_index / RowCount;
				
				if (col > hscrollbar.Maximum)
					hscrollbar.Value = hscrollbar.Maximum;
				else
					hscrollbar.Value = col;
			} else {
				if (top_index > vscrollbar.Maximum)
					vscrollbar.Value = vscrollbar.Maximum;
				else
					vscrollbar.Value = top_index;
				Scroll (vscrollbar, vscrollbar.Value - top_index);
			}
		}
		
		// Navigates to the indicated item and returns the new item
		private int NavigateItemVisually (ItemNavigation navigation)
		{			
			int page_size, columns, selected_index = -1;

			if (multicolumn) {
				columns = items_area.Width / ColumnWidthInternal; 
				page_size = columns * RowCount;
				if (page_size == 0) {
					page_size = RowCount;
				}
			} else {
				page_size = items_area.Height / ItemHeight;	
			}

			switch (navigation) {

			case ItemNavigation.PreviousColumn: {
				if (SelectedIndex - RowCount < 0) {
					return -1;
				}

				if (SelectedIndex - RowCount < top_index) {
					top_index = SelectedIndex - RowCount;
					UpdateTopItem ();
				}
					
				selected_index = SelectedIndex - RowCount;
				break;
			}
			
			case ItemNavigation.NextColumn: {
				if (SelectedIndex + RowCount >= Items.Count) {
					break;
				}

				if (SelectedIndex + RowCount > last_visible_index) {
					top_index = SelectedIndex;
					UpdateTopItem ();
				}
					
				selected_index = SelectedIndex + RowCount;
				break;
			}

			case ItemNavigation.First: {
				top_index = 0;
				selected_index  = 0;
				UpdateTopItem ();
				break;
			}

			case ItemNavigation.Last: {

				int rows = items_area.Height / ItemHeight;
				
				if (multicolumn) {
					selected_index = Items.Count - 1;
					break;
				}
				if (Items.Count < rows) {
					top_index = 0;
					selected_index  = Items.Count - 1;
					UpdateTopItem ();
				} else {
					top_index = Items.Count - rows;
					selected_index  = Items.Count - 1;
					UpdateTopItem ();
				}
				break;
			}

			case ItemNavigation.Next: {
				if (FocusedItem == Items.Count - 1)
					return -1;

				if (multicolumn) {
					selected_index = FocusedItem + 1;
					break;
				}
				
				int bottom = 0;
				ArrayList heights = new ArrayList ();
				if (draw_mode == DrawMode.OwnerDrawVariable) {
					for (int i = top_index; i <= FocusedItem + 1; i++) {
						int h = GetItemHeight (i);
						bottom += h;
						heights.Add (h);
					}
				} else {
					bottom = ((FocusedItem + 1) - top_index + 1) * ItemHeight;
				}

				if (bottom >= items_area.Height) {
					int overhang = bottom - items_area.Height;

					int offset = 0;
					if (draw_mode == DrawMode.OwnerDrawVariable)
						while (overhang > 0)
							overhang -= (int) heights [offset];
					else
						offset = (int) Math.Ceiling ((float)overhang / (float) ItemHeight);
					top_index += offset;
					UpdateTopItem ();
				}
				selected_index = FocusedItem + 1;
				break;
			}

			case ItemNavigation.Previous: {
				if (FocusedItem > 0) {
					if (FocusedItem - 1 < top_index) {
						top_index--;
						UpdateTopItem ();
					}
					selected_index = FocusedItem - 1;
				}
				break;
			}

			case ItemNavigation.NextPage: {
				if (Items.Count < page_size) {
					NavigateItemVisually (ItemNavigation.Last);
					break;
				}

				if (FocusedItem + page_size - 1 >= Items.Count) {
					top_index = Items.Count - page_size;
					UpdateTopItem ();
					selected_index = Items.Count - 1;
				}
				else {
					if (FocusedItem + page_size - 1  > last_visible_index) {
						top_index = FocusedItem;
						UpdateTopItem ();
					}
					
					selected_index = FocusedItem + page_size - 1;
				}
					
				break;
			}			

			case ItemNavigation.PreviousPage: {
					
				int rows = items_area.Height / ItemHeight;
				if (FocusedItem - (rows - 1) <= 0) {
					top_index = 0;
					UpdateTopItem ();
					selected_index = 0;
				}
				else { 
					if (SelectedIndex - (rows - 1)  < top_index) {
						top_index = FocusedItem - (rows - 1);
						UpdateTopItem ();
					}
					
					selected_index = FocusedItem - (rows - 1);
				}
					
				break;
			}		
			default:
				break;
			}
			
			return selected_index;
		}
		
		
		private void OnGotFocus (object sender, EventArgs e)
		{
			if (Items.Count == 0)
				return;

			if (FocusedItem == -1)
				FocusedItem = 0;

			InvalidateItem (FocusedItem);
		}
		
		private void OnLostFocus (object sender, EventArgs e)
		{
			if (FocusedItem != -1)
				InvalidateItem (FocusedItem);
		}

		private bool KeySearch (Keys key)
		{
			char c = (char) key;
			if (!Char.IsLetterOrDigit (c))
				return false;

			int idx = FindString (c.ToString (), SelectedIndex);
			if (idx != ListBox.NoMatches)
				SelectedIndex = idx;

			return true;
		}

		internal void HandleKeyDown (Keys key)
		{
			int new_item = -1;
			
			if (Items.Count == 0)
				return;

			if (KeySearch (key))
				return;

			switch (key) {
				
				case Keys.ControlKey:
					ctrl_pressed = true;
					break;
					
				case Keys.ShiftKey:
					shift_pressed = true;
					break;
					
				case Keys.Home:
					new_item = NavigateItemVisually (ItemNavigation.First);
					break;

				case Keys.End:
					new_item = NavigateItemVisually (ItemNavigation.Last);
					break;

				case Keys.Up:
					new_item = NavigateItemVisually (ItemNavigation.Previous);
					break;
	
				case Keys.Down:
					new_item = NavigateItemVisually (ItemNavigation.Next);
					break;
				
				case Keys.PageUp:
					new_item = NavigateItemVisually (ItemNavigation.PreviousPage);
					break;
	
				case Keys.PageDown:
					new_item = NavigateItemVisually (ItemNavigation.NextPage);
					break;

				case Keys.Right:
					if (multicolumn == true) {
						new_item = NavigateItemVisually (ItemNavigation.NextColumn);
					}
					break;
	
				case Keys.Left:
					if (multicolumn == true) {
						new_item = NavigateItemVisually (ItemNavigation.PreviousColumn);
					}
					break;
					
				case Keys.Space:
					if (selection_mode == SelectionMode.MultiSimple) {
						SelectedItemFromNavigation (FocusedItem);
					}
					break;
				

				default:
					break;
				}
				
				if (new_item != -1) {
					FocusedItem = new_item;

					if (selection_mode != SelectionMode.MultiSimple)
						SelectedItemFromNavigation (new_item);
				}
		}
		
		private void OnKeyUpLB (object sender, KeyEventArgs e)
		{
			switch (e.KeyCode) {
				case Keys.ControlKey:
					ctrl_pressed = false;
					break;
				case Keys.ShiftKey:
					shift_pressed = false;
					break;
				default: 
					break;
			}
		}

		internal void InvalidateItem (int index)
		{
			if (!IsHandleCreated)
				return;
			Rectangle bounds = GetItemDisplayRectangle (index, top_index);
			if (ClientRectangle.IntersectsWith (bounds))
				Invalidate (bounds);
		}

		internal virtual void OnItemClick (int index)
		{
			OnSelectedIndexChanged  (EventArgs.Empty);
			OnSelectedValueChanged (EventArgs.Empty);
		}

		int anchor = -1;
		int[] prev_selection;
		bool button_pressed = false;
		Point button_pressed_loc = new Point (-1, -1);

		private void SelectExtended (int index)
		{
			SuspendLayout ();

			ArrayList new_selection = new ArrayList ();
			int start = anchor < index ? anchor : index;
			int end = anchor > index ? anchor : index;
			for (int i = start; i <= end; i++)
				new_selection.Add (i);

			if (ctrl_pressed)
				foreach (int i in prev_selection)
					if (!new_selection.Contains (i))
						new_selection.Add (i);

			// Need to make a copy since we can't enumerate and modify the collection
			// at the same time
			ArrayList sel_indices = (ArrayList) selected_indices.List.Clone ();
			foreach (int i in sel_indices)
				if (!new_selection.Contains (i))
					selected_indices.Remove (i);

			foreach (int i in new_selection)
				if (!sel_indices.Contains (i))
					selected_indices.AddCore (i);
			ResumeLayout ();
		}

		private void OnMouseDownLB (object sender, MouseEventArgs e)
		{
			// Only do stuff with the left mouse button
			if ((e.Button & MouseButtons.Left) == 0)
				return;
			
			int index = IndexAtClientPoint (e.X, e.Y);
			if (index == -1)
				return;

			switch (SelectionMode) {
			case SelectionMode.One:
				SelectedIndices.AddCore (index); // Unselects previous one
				break;

			case SelectionMode.MultiSimple:
				if (SelectedIndices.Contains (index))
					SelectedIndices.RemoveCore (index);
				else
					SelectedIndices.AddCore (index);
				break;

			case SelectionMode.MultiExtended:
				shift_pressed = (XplatUI.State.ModifierKeys & Keys.Shift) != 0;
				ctrl_pressed = (XplatUI.State.ModifierKeys & Keys.Control) != 0;

				if (shift_pressed) {
					SelectedIndices.ClearCore ();
					SelectExtended (index);
					break;
				}

				anchor = index;

				if (ctrl_pressed) {
					prev_selection = new int [SelectedIndices.Count];
					SelectedIndices.CopyTo (prev_selection, 0);

					if (SelectedIndices.Contains (index))
						SelectedIndices.RemoveCore (index);
					else
						SelectedIndices.AddCore (index);

					break;
				}

				SelectedIndices.ClearCore ();
				SelectedIndices.AddCore (index);
				break;

			case SelectionMode.None:
				break;
			default:
				return;
			}

			button_pressed = true;
			button_pressed_loc = new Point (e.X, e.Y);
			FocusedItem = index;
		}

		private void OnMouseMoveLB (object sender, MouseEventArgs e)
		{
			// Don't take into account MouseMove events generated with MouseDown
			if (!button_pressed || button_pressed_loc == new Point (e.X, e.Y))
				return;

			int index = IndexAtClientPoint (e.X, e.Y);
			if (index == -1)
				return;

			switch (SelectionMode) {
			case SelectionMode.One:
				SelectedIndices.AddCore (index); // Unselects previous one
				break;

			case SelectionMode.MultiSimple:
				break;

			case SelectionMode.MultiExtended:
				SelectExtended (index);
				break;

			case SelectionMode.None:
				break;
			default:
				return;
			}

			FocusedItem = index;
		}

		internal override void OnDragDropEnd (DragDropEffects effects)
		{
			// In the case of a DnD operation (started on MouseDown)
			// there will be no MouseUp event, so we need to reset 
			// the state here
			button_pressed = false;
		}

		private void OnMouseUpLB (object sender, MouseEventArgs e)
		{
			// Only do stuff with the left mouse button
			if ((e.Button & MouseButtons.Left) == 0)
				return;

			if (e.Clicks > 1) {
				OnDoubleClick (EventArgs.Empty);
				OnMouseDoubleClick (e);
			}
			else if (e.Clicks == 1) {
				OnClick (EventArgs.Empty);
				OnMouseClick (e);
			}
			
			if (!button_pressed)
				return;

			int index = IndexAtClientPoint (e.X, e.Y);
			OnItemClick (index);
			button_pressed = ctrl_pressed = shift_pressed = false;
		}

		private void Scroll (ScrollBar scrollbar, int delta)
		{
			if (delta == 0 || !scrollbar.Visible || !scrollbar.Enabled)
				return;

			int max;
			if (scrollbar == hscrollbar)
				max = hscrollbar.Maximum - (items_area.Width / ColumnWidthInternal) + 1;
			else
				max = vscrollbar.Maximum - (items_area.Height / ItemHeight) + 1;

			int val = scrollbar.Value + delta;
			if (val > max)
				val = max;
			else if (val < scrollbar.Minimum)
				val = scrollbar.Minimum;
			scrollbar.Value = val;
		}

		private void OnMouseWheelLB (object sender, MouseEventArgs me)
		{
			if (Items.Count == 0)
				return;

			int lines = me.Delta / 120;

			if (MultiColumn)
				Scroll (hscrollbar, -SystemInformation.MouseWheelScrollLines * lines);
			else
				Scroll (vscrollbar, -lines);
		}

		internal override void OnPaintInternal (PaintEventArgs pevent)
		{
			if (suspend_layout)
				return;

			Draw (pevent.ClipRectangle, pevent.Graphics);
		}

		internal void RepositionScrollBars ()
		{
			if (vscrollbar.is_visible) {
				vscrollbar.Size = new Size (vscrollbar.Width, items_area.Height);
				vscrollbar.Location = new Point (items_area.Width, 0);
			}

			if (hscrollbar.is_visible) {
				hscrollbar.Size = new Size (items_area.Width, hscrollbar.Height);
				hscrollbar.Location = new Point (0, items_area.Height);
			}
		}

		// An item navigation operation (mouse or keyboard) has caused to select a new item
		internal void SelectedItemFromNavigation (int index)
		{
			switch (SelectionMode) {
				case SelectionMode.None:
					// .Net doesn't select the item, only ensures that it's visible
					// and fires the selection related events
					EnsureVisible (index);
					OnSelectedIndexChanged (EventArgs.Empty);
					OnSelectedValueChanged (EventArgs.Empty);
					break;
				case SelectionMode.One: {
					SelectedIndex = index;
					break;
				}
				case SelectionMode.MultiSimple: {
					if (SelectedIndex == -1) {
						SelectedIndex = index;
					} else {

						if (SelectedIndices.Contains (index))
							SelectedIndices.Remove (index);
						else {
							SelectedIndices.AddCore (index);

							OnSelectedIndexChanged  (EventArgs.Empty);
							OnSelectedValueChanged (EventArgs.Empty);
						}
					}
					break;
				}
				
				case SelectionMode.MultiExtended: {
					if (SelectedIndex == -1) {
						SelectedIndex = index;
					} else {

						if (ctrl_pressed == false && shift_pressed == false) {
							SelectedIndices.Clear ();
						}
						
						if (shift_pressed == true) {
							ShiftSelection (index);
						} else { // ctrl_pressed or single item
							SelectedIndices.AddCore (index);

						}

						OnSelectedIndexChanged  (EventArgs.Empty);
						OnSelectedValueChanged (EventArgs.Empty);
					}
					break;
				}
				
				default:
					break;
			}
		}
		
		private void ShiftSelection (int index)
		{
			int shorter_item = -1, dist = Items.Count + 1, cur_dist;
			
			foreach (int idx in selected_indices) {
				if (idx > index) {
					cur_dist = idx - index;
				} else {
					cur_dist = index - idx;
				}

				if (cur_dist < dist) {
					dist = cur_dist;
					shorter_item = idx;
				}
			}
			
			if (shorter_item != -1) {
				int start, end;
				
				if (shorter_item > index) {
					start = index;
					end = shorter_item;
				} else {
					start = shorter_item;
					end = index;
				}
				
				selected_indices.Clear ();
				for (int idx = start; idx <= end; idx++) {
					selected_indices.AddCore (idx);
				}
			}
		}
		
		internal int FocusedItem {
			get { return focused_item; }
			set {
				if (focused_item == value)
					return;

				int prev = focused_item;
			
				focused_item = value;
			
				if (has_focus == false)
					return;

				if (prev != -1)
					InvalidateItem (prev);
			
				if (value != -1)
					InvalidateItem (value);

				// UIA Framework: Generates FocusedItemChanged event.
				OnUIAFocusedItemChangedEvent ();
			}
		}

		StringFormat string_format;
		internal StringFormat StringFormat {
			get {
				if (string_format == null) {
					string_format = new StringFormat ();
					string_format.FormatFlags = StringFormatFlags.NoWrap;

					if (RightToLeft == RightToLeft.Yes)
						string_format.Alignment = StringAlignment.Far;
					else
						string_format.Alignment = StringAlignment.Near;
					CalculateTabStops ();
				}
				return string_format;
			}
		}

		internal virtual void CollectionChanged ()
		{
			if (sorted) 
				Sort (false);

			if (Items.Count == 0) {
				selected_indices.List.Clear ();
				focused_item = -1;
				top_index = 0;
			}
			if (Items.Count <= focused_item)
				focused_item = Items.Count - 1;

			if (!IsHandleCreated || suspend_layout)
				return;

			LayoutListBox ();

			base.Refresh ();
		}

		void EnsureVisible (int index)
		{
			if (!IsHandleCreated || index == -1)
				return;

			if (index < top_index) {
				top_index = index;
				UpdateTopItem ();
				Invalidate ();
			} else if (!multicolumn) {
				int rows = items_area.Height / ItemHeight;
				if (index >= (top_index + rows))
					top_index = index - rows + 1;

				UpdateTopItem ();
			} else {
				int rows = Math.Max (1, items_area.Height / ItemHeight);
				int cols = Math.Max (1, items_area.Width / ColumnWidthInternal);
				
				if (index >= (top_index + (rows * cols))) {
					int incolumn = index / rows;
					top_index = (incolumn - (cols - 1)) * rows;

					UpdateTopItem ();
					Invalidate ();
				}
			}
		}

		private void UpdateListBoxBounds ()
		{
			if (IsHandleCreated)
				SetBoundsInternal (bounds.X, bounds.Y, bounds.Width, IntegralHeight ? SnapHeightToIntegral (requested_height) : requested_height, BoundsSpecified.None);
		}

		private void UpdateScrollBars ()
		{
			items_area = ClientRectangle;
			if (UpdateHorizontalScrollBar ()) {
				items_area.Height -= hscrollbar.Height;
				if (UpdateVerticalScrollBar ()) {
					items_area.Width -= vscrollbar.Width;
					UpdateHorizontalScrollBar ();
				}
			} else if (UpdateVerticalScrollBar ()) {
				items_area.Width -= vscrollbar.Width;
				if (UpdateHorizontalScrollBar ()) {
					items_area.Height -= hscrollbar.Height;
					UpdateVerticalScrollBar ();
				}
			}

			RepositionScrollBars ();
		}

		/* Determines if the horizontal scrollbar has to be displyed */
		private bool UpdateHorizontalScrollBar ()
		{
			bool show = false;
			bool enabled = true;

			if (MultiColumn) {
				if (canvas_size.Width > items_area.Width) {
					show = true;
					hscrollbar.Maximum  = canvas_size.Width / ColumnWidthInternal - 1;
				} else if (ScrollAlwaysVisible == true) {
					enabled = false;
					show = true;
					hscrollbar.Maximum  = 0;
				}
			} else if (canvas_size.Width > ClientRectangle.Width && HorizontalScrollbar) {
				show = true;
				hscrollbar.Maximum = canvas_size.Width;
				hscrollbar.LargeChange = Math.Max (0, items_area.Width);
			} else if (scroll_always_visible && horizontal_scrollbar) {
				show = true;
				enabled = false;
				hscrollbar.Maximum = 0;
			}

			hbar_offset = hscrollbar.Value;
			hscrollbar.Enabled = enabled;
			hscrollbar.Visible = show;

			return show;
		}

		/* Determines if the vertical scrollbar has to be displyed */
		private bool UpdateVerticalScrollBar ()
		{
			if (MultiColumn || (Items.Count == 0 && !scroll_always_visible)) {
				vscrollbar.Visible = false;
				return false;
			} else if (Items.Count == 0) {
				vscrollbar.Visible = true;
				vscrollbar.Enabled = false;
				vscrollbar.Maximum = 0;
				return true;
			}

			bool show = false;
			bool enabled = true;
			if (canvas_size.Height > items_area.Height) {
				show = true;
				vscrollbar.Maximum = Items.Count - 1;
				vscrollbar.LargeChange = Math.Max (items_area.Height / ItemHeight, 0);
			} else if (ScrollAlwaysVisible) {
				show = true;
				enabled = false;
				vscrollbar.Maximum = 0;
			}

			vscrollbar.Enabled = enabled;
			vscrollbar.Visible = show;

			return show;
		}

		// Value Changed
		private void VerticalScrollEvent (object sender, EventArgs e)
		{
			int top_item = top_index;

			top_index = /*row_count + */ vscrollbar.Value;
			last_visible_index = LastVisibleItem ();

			int delta = (top_item - top_index) * ItemHeight;
			if (DrawMode == DrawMode.OwnerDrawVariable) {
				delta = 0;

				if (top_index < top_item)
					for (int i = top_index; i < top_item; i++)
						delta += GetItemHeight (i);
				else 
					for (int i = top_item; i < top_index; i++)
						delta -= GetItemHeight (i);
			}

			if (IsHandleCreated)
				XplatUI.ScrollWindow (Handle, items_area, 0, delta, false);
		}

		#endregion Private Methods

		public class IntegerCollection : IList, ICollection, IEnumerable
		{
			private ListBox owner;
			private List<int> list;
			
			#region Public Constructor
			public IntegerCollection (ListBox owner)
			{
				this.owner = owner;
				list = new List<int> ();
			}
			#endregion

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get { return list.Count; }
			}
			
			public int this [int index] {
				get { return list[index]; }
				set { list[index] = value; owner.CalculateTabStops (); }
			}
			#endregion

			#region Public Methods
			public int Add (int item)
			{
				// This collection does not allow duplicates
				if (!list.Contains (item)) {
					list.Add (item);
					list.Sort ();
					owner.CalculateTabStops ();
				}
				
				return list.IndexOf (item);
			}
			
			public void AddRange (int[] items)
			{
				AddItems (items);
			}
			
			public void AddRange (IntegerCollection value)
			{
				AddItems (value);
			}

			void AddItems (IList items)
			{
				if (items == null)
					throw new ArgumentNullException ("items");

				foreach (int i in items)
					if (!list.Contains (i))
						list.Add (i);

				list.Sort ();
			}

			public void Clear ()
			{
				list.Clear ();
				owner.CalculateTabStops ();
			}
			
			public bool Contains (int item)
			{
				return list.Contains (item);
			}
			
			public void CopyTo (Array destination, int index)
			{
				for (int i = 0; i < list.Count; i++)
					destination.SetValue (list[i], index++);
			}
			
			public int IndexOf (int item)
			{
				return list.IndexOf (item);
			}
			
			public void Remove (int item)
			{
				list.Remove (item);
				list.Sort ();
				owner.CalculateTabStops ();
			}
			
			public void RemoveAt (int index)
			{
				if (index < 0)
					throw new IndexOutOfRangeException ();

				list.RemoveAt (index);
				list.Sort ();
				owner.CalculateTabStops ();
			}
			#endregion

			#region IEnumerable Members
			IEnumerator IEnumerable.GetEnumerator ()
			{
				return list.GetEnumerator ();
			}
			#endregion

			#region IList Members
			int IList.Add (object item)
			{
				int? intValue = item as int?;
				if (!intValue.HasValue)
					throw new ArgumentException ("item");
				return Add (intValue.Value);
			}

			void IList.Clear ()
			{
				Clear ();
			}

			bool IList.Contains (object item)
			{
				int? intValue = item as int?;
				if (!intValue.HasValue)
					return false;
				return Contains (intValue.Value);
			}

			int IList.IndexOf (object item)
			{
				int? intValue = item as int?;
				if (!intValue.HasValue)
					return -1;
				return IndexOf (intValue.Value);
			}

			void IList.Insert (int index, object value)
			{
				throw new NotSupportedException (string.Format (
					CultureInfo.InvariantCulture, "No items "
					+ "can be inserted into {0}, since it is"
					+ " a sorted collection.", this.GetType ()));
			}

			bool IList.IsFixedSize
			{
				get { return false; }
			}

			bool IList.IsReadOnly
			{
				get { return false; }
			}

			void IList.Remove (object value)
			{
				int? intValue = value as int?;
				if (!intValue.HasValue)
					throw new ArgumentException ("value");

				Remove (intValue.Value);
			}

			void IList.RemoveAt (int index)
			{
				RemoveAt (index);
			}

			object IList.this[int index] {
				get { return this[index]; }
				set { this[index] = (int)value; }
			}
			#endregion

			#region ICollection Members
			bool ICollection.IsSynchronized {
				get { return true; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}
			#endregion
		}

		[ListBindable (false)]
		public class ObjectCollection : IList, ICollection, IEnumerable
		{
			internal class ListObjectComparer : IComparer
			{
				public int Compare (object a, object b)
				{
					string str1 = a.ToString ();
					string str2 = b.ToString ();
					return str1.CompareTo (str2);
				}
			}

			private ListBox owner;
			internal ArrayList object_items = new ArrayList ();
			
			#region UIA Framework Events 
			//NOTE:
			//	We are using Reflection to add/remove internal events.
			//	Class ListProvider uses the events.
			//
			//Event used to generate UIA StructureChangedEvent
			static object UIACollectionChangedEvent = new object ();

			internal event CollectionChangeEventHandler UIACollectionChanged {
				add { owner.Events.AddHandler (UIACollectionChangedEvent, value); }
				remove { owner.Events.RemoveHandler (UIACollectionChangedEvent, value); }
			}

			internal void OnUIACollectionChangedEvent (CollectionChangeEventArgs args)
			{
				CollectionChangeEventHandler eh
					= (CollectionChangeEventHandler) owner.Events [UIACollectionChangedEvent];
				if (eh != null)
					eh (owner, args);
			}
			#endregion UIA Framework Events 

			public ObjectCollection (ListBox owner)
			{
				this.owner = owner;
			}

			public ObjectCollection (ListBox owner, object[] value)
			{
				this.owner = owner;
				AddRange (value);
			}

			public ObjectCollection (ListBox owner,  ObjectCollection value)
			{
				this.owner = owner;
				AddRange (value);
			}

			#region Public Properties
			public int Count {
				get { return object_items.Count; }
			}

			public bool IsReadOnly {
				get { return false; }
			}

			[Browsable(false)]
			[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
			public virtual object this [int index] {
				get {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("Index of out range");

					return object_items[index];
				}
				set {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("Index of out range");
					if (value == null)
						throw new ArgumentNullException ("value");
						
					//UIA Framework event: Item Removed
					OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Remove, object_items [index]));

					object_items[index] = value;
					
					//UIA Framework event: Item Added
					OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, value));

					owner.CollectionChanged ();
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
				object[] selectedItems = null;

				// we need to remember the original selected items so that we can update the indices
				if (owner.sorted) {
					selectedItems = new object[owner.SelectedItems.Count];
					owner.SelectedItems.CopyTo (selectedItems, 0);
				}

				idx = AddItem (item);
				owner.CollectionChanged ();
				
				// If we are sorted, the item probably moved indexes, get the real one
				if (owner.sorted) {
					// update indices of selected items
					owner.SelectedIndices.Clear ();
					for (int i = 0; i < selectedItems.Length; i++) {
						owner.SelectedIndex = this.IndexOf (selectedItems [i]);
					}
					return this.IndexOf (item);
				}
					
				return idx;
			}

			public void AddRange (object[] items)
			{
				AddItems (items);
			}

			public void AddRange (ObjectCollection value)
			{
				AddItems (value);
			}

			internal void AddItems (IList items)
			{
				if (items == null)
					throw new ArgumentNullException ("items");

				foreach (object mi in items)
					AddItem (mi);

				owner.CollectionChanged ();
			}

			public virtual void Clear ()
			{
				owner.selected_indices.ClearCore ();
				object_items.Clear ();
				owner.CollectionChanged ();

				//UIA Framework event: Items list cleared
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Refresh, null));
			}

			public bool Contains (object value)
			{
				if (value == null)
					throw new ArgumentNullException ("value");

				return object_items.Contains (value);
			}

			public void CopyTo (object[] destination, int arrayIndex)
			{
				object_items.CopyTo (destination, arrayIndex);
			}

			void ICollection.CopyTo (Array destination, int index)
			{
				object_items.CopyTo (destination, index);
			}

			public IEnumerator GetEnumerator ()
			{
				return object_items.GetEnumerator ();
			}

			int IList.Add (object item)
			{
				return Add (item);
			}

			public int IndexOf (object value)
			{
				if (value == null)
					throw new ArgumentNullException ("value");

				return object_items.IndexOf (value);
			}

			public void Insert (int index,  object item)
			{
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("Index of out range");
				if (item == null)
					throw new ArgumentNullException ("item");
					
				owner.BeginUpdate ();
				object_items.Insert (index, item);
				owner.CollectionChanged ();
				owner.EndUpdate ();
				
				//UIA Framework event: Item Added
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, item));
			}

			public void Remove (object value)
			{
				if (value == null)
					return;

				int index = IndexOf (value);
				if (index != -1)
					RemoveAt (index);
			}

			public void RemoveAt (int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				//UIA Framework element removed
				object removed = object_items [index];
				UpdateSelection (index);
				object_items.RemoveAt (index);
				owner.CollectionChanged ();

				//UIA Framework event: Item Removed
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Remove, removed));
			}
			#endregion Public Methods

			#region Private Methods
			internal int AddItem (object item)
			{
				if (item == null)
					throw new ArgumentNullException ("item");

				int cnt = object_items.Count;
				object_items.Add (item);

				//UIA Framework event: Item Added
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, item));

				return cnt;
			}

			// we receive the index to be removed
			void UpdateSelection (int removed_index)
			{
				owner.selected_indices.Remove (removed_index);

				if (owner.selection_mode != SelectionMode.None) {
					int last_idx = object_items.Count - 1;

					// if the last item was selected, remove it from selection,
					// since it will become invalid after the removal
					if (owner.selected_indices.Contains (last_idx)) {
						owner.selected_indices.Remove (last_idx);

						// in SelectionMode.One try to put the selection on the new last item
						int new_idx = last_idx - 1;
						if (owner.selection_mode == SelectionMode.One && new_idx > -1)
							owner.selected_indices.Add (new_idx);
					}
				}

			}

			internal void Sort ()
			{
				object_items.Sort (new ListObjectComparer ());
			}

			#endregion Private Methods
		}

		public class SelectedIndexCollection : IList, ICollection, IEnumerable
		{
			private ListBox owner;
			ArrayList selection;
			bool sorting_needed; // Selection state retrieval is done sorted - we do it lazyly

			#region UIA Framework Events 

			//NOTE:
			//	We are using Reflection to add/remove internal events.
			//	Class ListProvider uses the events.
			//
			//Event used to generate UIA StructureChangedEvent
			static object UIACollectionChangedEvent = new object ();

			internal event CollectionChangeEventHandler UIACollectionChanged {
				add { owner.Events.AddHandler (UIACollectionChangedEvent, value); }
				remove { owner.Events.RemoveHandler (UIACollectionChangedEvent, value); }
			}

			internal void OnUIACollectionChangedEvent (CollectionChangeEventArgs args)
			{
				CollectionChangeEventHandler eh
					= (CollectionChangeEventHandler) owner.Events [UIACollectionChangedEvent];
				if (eh != null)
					eh (owner, args);
			}

			#endregion UIA Framework Events 


			public SelectedIndexCollection (ListBox owner)
			{
				this.owner = owner;
				selection = new ArrayList ();
			}

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get { return selection.Count; }
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public int this [int index] {
				get {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("Index of out range");

					CheckSorted ();
					return (int)selection [index];
				}
			}

			bool ICollection.IsSynchronized {
				get { return true; }
			}

			bool IList.IsFixedSize{
				get { return true; }
			}

			object ICollection.SyncRoot {
				get { return selection; }
			}

			#endregion Public Properties

			#region Public Methods
			public void Add (int index)
			{
				if (AddCore (index)) {
					owner.OnSelectedIndexChanged (EventArgs.Empty);
					owner.OnSelectedValueChanged (EventArgs.Empty);
				}
			}

			// Need to separate selection logic from events,
			// since selection changes using keys/mouse handle them their own way
			internal bool AddCore (int index)
			{
				if (selection.Contains (index))
					return false;

				if (index == -1) // Weird MS behaviour
					return false;
				if (index < -1 || index >= owner.Items.Count)
					throw new ArgumentOutOfRangeException ("index");
				if (owner.selection_mode == SelectionMode.None)
					throw new InvalidOperationException ("Cannot call this method when selection mode is SelectionMode.None");

				if (owner.selection_mode == SelectionMode.One && Count > 0) // Unselect previously selected item
					RemoveCore ((int)selection [0]);

				selection.Add (index);
				sorting_needed = true;
				owner.EnsureVisible (index);
				owner.FocusedItem = index;
				owner.InvalidateItem (index);

				// UIA Framework event: Selected item added
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, index));

				return true;
			}

			public void Clear ()
			{
				if (ClearCore ()) {
					owner.OnSelectedIndexChanged (EventArgs.Empty);
					owner.OnSelectedValueChanged (EventArgs.Empty);
				}
			}

			internal bool ClearCore ()
			{
				if (selection.Count == 0)
					return false;

				foreach (int index in selection)
					owner.InvalidateItem (index);

				selection.Clear ();

				// UIA Framework event: Selected items list updated
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Refresh, -1));

				return true;
			}

			public bool Contains (int selectedIndex)
			{
				foreach (int index in selection)
					if (index == selectedIndex)
						return true;
				return false;
			}

			public void CopyTo (Array destination, int index)
			{
				CheckSorted ();
				selection.CopyTo (destination, index);
			}

			public IEnumerator GetEnumerator ()
			{
				CheckSorted ();
				return selection.GetEnumerator ();
			}

			// FIXME: Probably we can avoid sorting when calling
			// IndexOf (imagine a scenario where multiple removal of items
			// happens)
			public void Remove (int index)
			{
				// Separate logic from events here too
				if (RemoveCore (index)) {
					owner.OnSelectedIndexChanged (EventArgs.Empty);
					owner.OnSelectedValueChanged (EventArgs.Empty);
				}
			}

			internal bool RemoveCore (int index)
			{
				int idx = IndexOf (index);
				if (idx == -1)
					return false;

				selection.RemoveAt (idx);
				owner.InvalidateItem (index);

				// UIA Framework event: Selected item removed from selection
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Remove, index));

				return true;
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
				get { return this [index]; }
				set {throw new NotImplementedException (); }
			}

			public int IndexOf (int selectedIndex)
			{
				CheckSorted ();

				for (int i = 0; i < selection.Count; i++)
					if ((int)selection [i] == selectedIndex)
						return i;

				return -1;
			}
			#endregion Public Methods
			internal ArrayList List {
				get {
					CheckSorted ();
					return selection;
				}
			}

			void CheckSorted ()
			{
				if (sorting_needed) {
					sorting_needed = false;
					selection.Sort ();
				}
			}
		}

		public class SelectedObjectCollection : IList, ICollection, IEnumerable
		{
			private ListBox owner;

			public SelectedObjectCollection (ListBox owner)
			{
				this.owner = owner;
			}

			#region Public Properties
			public int Count {
				get { return owner.selected_indices.Count; }
			}

			public bool IsReadOnly {
				get { return true; }
			}

			[Browsable(false)]
			[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
			public object this [int index] {
				get {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("Index of out range");

					return owner.items [owner.selected_indices [index]];
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

			#endregion Public Properties

			#region Public Methods
			public void Add (object value)
			{
				if (owner.selection_mode == SelectionMode.None)
					throw new ArgumentException ("Cannot call this method if SelectionMode is SelectionMode.None");

				int idx = owner.items.IndexOf (value);
				if (idx == -1)
					return;

				owner.selected_indices.Add (idx);
			}

			public void Clear ()
			{
				owner.selected_indices.Clear ();
			}

			public bool Contains (object selectedObject)
			{
				int idx = owner.items.IndexOf (selectedObject);
				return idx == -1 ? false : owner.selected_indices.Contains (idx);
			}

			public void CopyTo (Array destination, int index)
			{
				for (int i = 0; i < Count; i++)
					destination.SetValue (this [i], index++);
			}

			public void Remove (object value)
			{
				if (value == null)
					return;

				int idx = owner.items.IndexOf (value);
				if (idx == -1)
					return;

				owner.selected_indices.Remove (idx);
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ();
			}

			void IList.Clear ()
			{
				throw new NotSupportedException ();
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
	
			public int IndexOf (object selectedObject)
			{
				int idx = owner.items.IndexOf (selectedObject);
				return idx == -1 ? -1 : owner.selected_indices.IndexOf (idx);
			}

			public IEnumerator GetEnumerator ()
			{
				//FIXME: write an enumerator that uses selection.GetEnumerator
				//  so that invalidation is write on selection changes
				object [] items = new object [Count];
				for (int i = 0; i < Count; i++) {
					items [i] = owner.items [owner.selected_indices [i]];
				}

				return items.GetEnumerator ();
			}

			#endregion Public Methods
		}
	}
}
