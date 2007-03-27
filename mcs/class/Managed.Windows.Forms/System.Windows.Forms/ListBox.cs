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
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[DefaultProperty("Items")]
	[DefaultEvent("SelectedIndexChanged")]
	[Designer ("System.Windows.Forms.Design.ListBoxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
#if NET_2_0
	[DefaultBindingProperty ("SelectedValue")]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
#endif
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
		private int requested_height = -1;
		private DrawMode draw_mode = DrawMode.Normal;
		private int horizontal_extent = 0;
		private bool horizontal_scrollbar = false;
		private bool integral_height = true;
		private bool multicolumn = false;
		private bool scroll_always_visible = false;
		private int selected_index = -1;		
		private SelectedIndexCollection selected_indices;		
		private SelectedObjectCollection selected_items;
		private ArrayList selection = new ArrayList ();
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

		public ListBox ()
		{
			border_style = BorderStyle.Fixed3D;			
			BackColor = ThemeEngine.Current.ColorWindow;

			items = CreateItemCollection ();
			selected_indices = new SelectedIndexCollection (this);
			selected_items = new SelectedObjectCollection (this);

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
			KeyDown += new KeyEventHandler (OnKeyDownLB);
			KeyUp += new KeyEventHandler (OnKeyUpLB);
			GotFocus += new EventHandler (OnGotFocus);
			LostFocus += new EventHandler (OnLostFocus);
			
			SetStyle (ControlStyles.UserPaint, false);
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

#if NET_2_0
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
#else
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
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

				base.Refresh ();
    			}
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
					SizeF sz = DeviceContext.MeasureString ("The quick brown Fox", Font);
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
#if NET_2_0
		[MergableProperty (false)]
#endif
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
			}
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
			get { return selected_index;}
			set {
				if (value < -1 || value >= Items.Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				if (SelectionMode == SelectionMode.None)
					throw new ArgumentException ("cannot call this method if SelectionMode is SelectionMode.None");

				if (selected_index == value)
					return;

				if (value == -1)
					ClearSelected ();
				else if (SelectionMode == SelectionMode.One)
					UnSelectItem (selected_index, true);

    				if (value != -1 && value < top_index) {
    					top_index = value;
    					UpdateTopItem ();
    				} else {
    					int rows = items_area.Height / ItemHeight;
    					if (value >= (top_index + rows))
    					{
    						top_index = value - rows + 1;
    						UpdateTopItem ();
    					}
    				}
    				SelectItem (value);
    				selected_index = value;
    				FocusedItem = value;
    				OnSelectedIndexChanged  (new EventArgs ());
    				OnSelectedValueChanged (new EventArgs ());
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
					ClearSelected ();
					break;						

				case SelectionMode.One:
					while (SelectedIndices.Count > 1)
						UnSelectItem (SelectedIndices [SelectedIndices.Count - 1], true);
					break;

				default:
					break;						
				}
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

				top_index = value;
				UpdateTopItem ();
				base.Refresh ();
			}
		}

		[DefaultValue (true)]
		public bool UseTabStops {
			get { return use_tabstops; }

    			set {
				if (use_tabstops == value)
					return;

    				use_tabstops = value;    				
				if (use_tabstops)
					StringFormat.SetTabStops (0, new float [] {(float)(Font.Height * 3.7)});
				else
					StringFormat.SetTabStops (0, new float [0]);
				base.Refresh ();
    			}
		}

#if NET_2_0
		protected override bool AllowSelection {
			get {
				return SelectionMode != SelectionMode.None;
			}
		}
#endif

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

		#region Public Methods
#if NET_2_0
		[Obsolete ("this method has been deprecated")]
#endif
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
			foreach (int i in selected_indices) {
				UnSelectItem (i, false);
			}

			selection.Clear ();
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

			if (startIndex < -1 || startIndex >= Items.Count - 1)
				throw new ArgumentOutOfRangeException ("Index of out range");

			startIndex++;
			for (int i = startIndex; i < Items.Count; i++) {
				if ((GetItemText (Items[i])).StartsWith (s))
					return i;
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

			if (startIndex < -1 || startIndex >= Items.Count - 1)
				throw new ArgumentOutOfRangeException ("Index of out range");

			startIndex++;
			for (int i = startIndex; i < Items.Count; i++) {
				if ((GetItemText (Items[i])).Equals (s))
					return i;
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
				rect.Y = ((index - top_index) % RowCount) * ItemHeight;
				rect.X = col * ColumnWidthInternal;
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

			return rect;
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
			} 
			else {
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
				SizeF sz = DeviceContext.MeasureString ("The quick brown Fox", Font);
				item_height = (int) sz.Height;
				if (IntegralHeight)
					UpdateListBoxBounds ();
				LayoutListBox ();
			}
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
			LayoutListBox ();
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

		protected override void SetBoundsCore (int x,  int y, int width, int height, BoundsSpecified specified)
		{
			if ((specified & BoundsSpecified.Height) == BoundsSpecified.Height)
				requested_height = height;

			if (IntegralHeight) {
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
			}

			base.SetBoundsCore (x, y, width, height, specified);
			UpdateScrollBars ();
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
				Items.AddRange (value);
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
				SelectItem (index);
			else
    				UnSelectItem (index, true);
		}

		protected virtual void Sort ()
		{
			if (Items.Count == 0)
				return;

			Items.Sort ();
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
			base.WndProc (ref m);
		}

		#endregion Public Methods

		#region Private Methods

		private Size canvas_size;

		private void LayoutListBox ()
		{
			if (!IsHandleCreated || suspend_layout)
				return;

			if (MultiColumn)
				LayoutMultiColumn ();
			else
				LayoutSingleColumn ();

			UpdateScrollBars ();
			last_visible_index = LastVisibleItem ();
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
					SizeF sz = DeviceContext.MeasureString (GetItemText (Items[i]), Font);
					if ((int) sz.Width > width)
						width = (int) sz.Width;
				}
				break;
			}

			canvas_size = new Size (width, height);
		}

		private void LayoutMultiColumn ()
		{
			int usable_height = ClientRectangle.Height - (ScrollAlwaysVisible ? hscrollbar.Height : 0);
			row_count = usable_height / ItemHeight;
			if (row_count == 0)
				row_count = 1;
			int cols = (int) Math.Ceiling ((float)Items.Count / (float) row_count);
			Size sz = new Size (cols * ColumnWidthInternal, row_count * ItemHeight);
			if (!ScrollAlwaysVisible && sz.Width > ClientRectangle.Width && row_count > 1) {
				usable_height = ClientRectangle.Height - hscrollbar.Height;
				row_count = usable_height / ItemHeight;
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

				OnDrawItem (new DrawItemEventArgs (dc, Font, rect, i, state, ForeColor, BackColor));
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

				if (IsHandleCreated)
					XplatUI.ScrollWindow (Handle, items_area, old_offset - hbar_offset, 0, false);
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
				}
				else {					
					if (item_rect.Y + item_rect.Height > top_y) {
						return i;
					}
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
				int val = vscrollbar.Value;
				if (top_index > vscrollbar.Maximum)
					vscrollbar.Value = vscrollbar.Maximum;
				else
					vscrollbar.Value = top_index;
				Scroll (vscrollbar, vscrollbar.Value - top_index);
				if (IsHandleCreated)
					XplatUI.ScrollWindow (Handle, items_area, 0, ItemHeight * (val - vscrollbar.Value), false);
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
				if (FocusedItem - RowCount < 0) {
					return -1;
				}

				if (FocusedItem - RowCount < top_index) {
					top_index = FocusedItem - RowCount;
					UpdateTopItem ();
				}
					
				selected_index = FocusedItem - RowCount;
				break;
			}
			
			case ItemNavigation.NextColumn: {
				if (FocusedItem + RowCount >= Items.Count) {
					break;
				}

				if (FocusedItem + RowCount > last_visible_index) {
					top_index = FocusedItem;
					UpdateTopItem ();
				}
					
				selected_index = FocusedItem + RowCount;					
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
					SelectedIndex = 0;					
				}
				else { 
					if (FocusedItem - (rows - 1)  < top_index) {
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
			if (FocusedItem != -1)
				InvalidateItem (FocusedItem);
		}		
		
		private void OnLostFocus (object sender, EventArgs e) 			
		{			
			if (FocusedItem != -1)
				InvalidateItem (FocusedItem);
		}		

		private void OnKeyDownLB (object sender, KeyEventArgs e)
		{					
			int new_item = -1;
			
			if (Items.Count == 0)
				return;

			switch (e.KeyCode) {
				
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
					if (selection_mode != SelectionMode.MultiSimple && selection_mode != SelectionMode.None) {
						SelectedItemFromNavigation (new_item);
					}
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
					if (!selection.Contains (i))
						new_selection.Add (i);

			foreach (int i in SelectedIndices)
				if (!new_selection.Contains (i))
					UnSelectItem (i, true);

			foreach (int i in new_selection)
				if (!SelectedIndices.Contains (i))
					SelectItem (i);
			ResumeLayout ();
		}

		private void OnMouseDownLB (object sender, MouseEventArgs e)
    		{
    			int index = IndexAtClientPoint (e.X, e.Y);
    			    				
			if (index == -1)
				return;			

			switch (SelectionMode) {
			case SelectionMode.One:
				if (SelectedIndex != index) {
					UnSelectItem (SelectedIndex, true);
					SelectItem (index);
				}
				selected_index = index;
				break;

			case SelectionMode.MultiSimple:
				if (SelectedIndices.Contains (index))
					UnSelectItem (index, true);
				else
					SelectItem (index);
				break;

			case SelectionMode.MultiExtended:
				shift_pressed = (XplatUI.State.ModifierKeys & Keys.Shift) != 0;
				ctrl_pressed = (XplatUI.State.ModifierKeys & Keys.Control) != 0;

				if (ctrl_pressed) {
					prev_selection = new int [selection.Count];
					SelectedIndices.CopyTo (prev_selection, 0);
				} else
					ClearSelected ();

				if (!shift_pressed)
					anchor = index;

				SelectExtended (index);
				break;

			case SelectionMode.None:
			default:
				return;
			}
				
			button_pressed = true;
			FocusedItem = index;
    		}

		private void OnMouseMoveLB (object sender, MouseEventArgs e)
    		{
			if (!button_pressed)
				return;

    			int index = IndexAtClientPoint (e.X, e.Y);

			switch (SelectionMode) {
			case SelectionMode.One:
				if (index == selected_index)
					return;

				UnSelectItem (SelectedIndex, true);
				SelectItem (index);
				selected_index = index;
				break;

			case SelectionMode.MultiSimple:
				break;

			case SelectionMode.MultiExtended:
				SelectExtended (index);
				break;

			case SelectionMode.None:
			default:
				return;
			}

			FocusedItem = index;
		}

		private void OnMouseUpLB (object sender, MouseEventArgs e)
    		{
			if (e.Clicks > 1)
				OnDoubleClick (EventArgs.Empty);
			else if (e.Clicks == 1)
				OnClick (EventArgs.Empty);
			
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

		// Add an item in the Selection array and marks it visually as selected
		private void SelectItem (int index)
		{
			if (index == -1 || SelectedIndices.Contains (index))
				return;

    			selection.Add (Items[index]);
    			InvalidateItem (index);
		}		
		
		// An item navigation operation (mouse or keyboard) has caused to select a new item
		internal void SelectedItemFromNavigation (int index)
		{
			switch (SelectionMode) {
    				case SelectionMode.None: // Do nothing
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
							UnSelectItem (index, true);
						else {
    							SelectItem (index);
    							OnSelectedIndexChanged  (new EventArgs ());
    							OnSelectedValueChanged (new EventArgs ());
    						}
    					}
    					break;
    				}
    				
    				case SelectionMode.MultiExtended: {
					if (SelectedIndex == -1) {
						SelectedIndex = index;
					} else {

						if (ctrl_pressed == false && shift_pressed == false) {
							ClearSelected ();
						}
						
						if (shift_pressed == true) {
							ShiftSelection (index);
						} else { // ctrl_pressed or single item
							SelectItem (index);
						}
    						
    						OnSelectedIndexChanged  (new EventArgs ());
    						OnSelectedValueChanged (new EventArgs ());
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
				}
				else {
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
				
				ClearSelected ();
				for (int idx = start; idx <= end; idx++) {
					SelectItem (idx);	
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
			}
		}

		// Removes an item in the Selection array and marks it visually as unselected
		private void UnSelectItem (int index, bool remove)
		{
			if (index == -1)
				return;

			if (remove)
				selection.Remove (Items[index]);

			InvalidateItem (index);
		}

		StringFormat string_format;
		internal StringFormat StringFormat {
			get {
				if (string_format == null) {
					string_format = new StringFormat ();
					if (RightToLeft == RightToLeft.Yes)
						string_format.Alignment = StringAlignment.Far;
					else
						string_format.Alignment = StringAlignment.Near;
					if (use_tabstops)
						string_format.SetTabStops (0, new float [] {(float)(Font.Height * 3.7)});
				}
				return string_format;
			}

		}

		internal virtual void CollectionChanged ()
		{
			if (sorted) 
				Sort ();
			
			if (Items.Count == 0) {
				selected_index = -1;
				focused_item = -1;
				top_index = 0;
			}
			
			if (!IsHandleCreated || suspend_layout)
				return;

			LayoutListBox ();

			base.Refresh ();
		}

		private void UpdateListBoxBounds ()
		{
			if (requested_height == -1)
				return;

			SetBounds(bounds.X, bounds.Y, bounds.Width, requested_height, BoundsSpecified.Height);
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
				hscrollbar.LargeChange = items_area.Width;
			}

			hbar_offset = hscrollbar.Value;
			hscrollbar.Enabled = enabled;
			hscrollbar.Visible = show;

			return show;
		}

		/* Determines if the vertical scrollbar has to be displyed */
		private bool UpdateVerticalScrollBar ()
		{
			if (MultiColumn || Items.Count == 0) {
				vscrollbar.Visible = false;
				return false;
			}

			bool show = false;
			bool enabled = true;
			if (canvas_size.Height > items_area.Height) {
				show = true;
				vscrollbar.Maximum = Items.Count - 1;
				vscrollbar.LargeChange = items_area.Height / ItemHeight;
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

			int diff = top_item - top_index;

			if (IsHandleCreated)
				XplatUI.ScrollWindow (Handle, items_area, 0, ItemHeight * diff, false);
		}

		#endregion Private Methods

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

					object_items[index] = value;
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

				idx = AddItem (item);
				owner.CollectionChanged ();
				return idx;
			}

			public void AddRange (object[] items)
			{
				foreach (object mi in items)
					AddItem (mi);

				owner.CollectionChanged ();
			}

			public void AddRange (ObjectCollection col)
			{
				foreach (object mi in col)
					AddItem (mi);

				owner.CollectionChanged ();
			}

			internal void AddRange (IList list)
			{
				foreach (object mi in list)
					AddItem (mi);

				owner.CollectionChanged ();
			}

			public virtual void Clear ()
			{
				owner.selection.Clear ();
				object_items.Clear ();
				owner.CollectionChanged ();
			}
			public bool Contains (object obj)
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
				return object_items.IndexOf (value);
			}

			public void Insert (int index,  object item)
			{
				if (index < 0 || index > Count)
					throw new ArgumentOutOfRangeException ("Index of out range");
					
				owner.BeginUpdate ();
				object_items.Insert (index, item);
				owner.CollectionChanged ();
				owner.EndUpdate ();
			}

			public void Remove (object value)
			{				
				RemoveAt (IndexOf (value));				
			}

			public void RemoveAt (int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				owner.selection.Remove (object_items [index]);
				object_items.RemoveAt (index);
				owner.CollectionChanged ();
			}
			#endregion Public Methods

			#region Private Methods
			internal int AddItem (object item)
			{
				if (item == null)
					throw new ArgumentNullException ("item");

				int cnt = object_items.Count;
				object_items.Add (item);
				return cnt;
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

			public SelectedIndexCollection (ListBox owner)
			{
				this.owner = owner;
			}

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get { return owner.selection.Count; }
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public int this [int index] {
				get {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("Index of out range");

					return owner.Items.IndexOf (owner.selection [index]);
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
				foreach (object o in owner.selection)
					if (owner.Items.IndexOf (o) == selectedIndex)
						return true;
				return false;
			}

			public void CopyTo (Array dest, int index)
			{
				foreach (object o in owner.selection)
					dest.SetValue(owner.Items.IndexOf (o), index++);
			}

			public IEnumerator GetEnumerator ()
			{
				//FIXME: write an enumerator that uses owner.selection.GetEnumerator
				//  so that invalidation is write on selection changes
				ArrayList indices = new ArrayList ();
				foreach (object o in owner.selection)
					indices.Add (owner.Items.IndexOf (o));
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
				get {return owner.Items.IndexOf (owner.selection [index]); }
				set {throw new NotImplementedException (); }
			}

			public int IndexOf (int selectedIndex)
			{
				for (int i = 0; i < owner.selection.Count; i++)
					if (owner.Items.IndexOf (owner.selection [i]) == selectedIndex)
						return i;
				return -1;
			}
			#endregion Public Methods
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
				get { return owner.selection.Count; }
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

					return owner.selection [index];
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
			public bool Contains (object selectedObject)
			{
				return owner.selection.Contains (selectedObject);
			}

			public void CopyTo (Array dest, int index)
			{
				owner.selection.CopyTo (dest, index);
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
	
			public int IndexOf (object item)
			{
				return owner.selection.IndexOf (item);
			}

			public IEnumerator GetEnumerator ()
			{
				return owner.selection.GetEnumerator ();
			}

			#endregion Public Methods
		}

	}
}

