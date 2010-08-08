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
// Copyright (c) 2004-2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Ravindra Kumar (rkumar@novell.com)
//	Jordi Mas i Hernandez, jordi@ximian.com
//	Mike Kestner (mkestner@novell.com)
//	Daniel Nauck (dna(at)mono-project(dot)de)
//	Carlos Alberto Cortez <calberto.cortez@gmail.com>
//


// NOT COMPLETE


using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Globalization;
#if NET_2_0
using System.Collections.Generic;
#endif

namespace System.Windows.Forms
{
	[DefaultEvent ("SelectedIndexChanged")]
	[DefaultProperty ("Items")]
	[Designer ("System.Windows.Forms.Design.ListViewDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
#if NET_2_0
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	[Docking (DockingBehavior.Ask)]
#endif
	public class ListView : Control
	{
		private ItemActivation activation = ItemActivation.Standard;
		private ListViewAlignment alignment = ListViewAlignment.Top;
		private bool allow_column_reorder;
		private bool auto_arrange = true;
		private bool check_boxes;
		private readonly CheckedIndexCollection checked_indices;
		private readonly CheckedListViewItemCollection checked_items;
		private readonly ColumnHeaderCollection columns;
		internal int focused_item_index = -1;
		private bool full_row_select;
		private bool grid_lines;
		private ColumnHeaderStyle header_style = ColumnHeaderStyle.Clickable;
		private bool hide_selection = true;
		private bool hover_selection;
		private IComparer item_sorter;
		private readonly ListViewItemCollection items;
#if NET_2_0
		private readonly ListViewGroupCollection groups;
		private bool owner_draw;
		private bool show_groups = true;
#endif
		private bool label_edit;
		private bool label_wrap = true;
		private bool multiselect = true;
		private bool scrollable = true;
		private bool hover_pending;
		private readonly SelectedIndexCollection selected_indices;
		private readonly SelectedListViewItemCollection selected_items;
		private SortOrder sort_order = SortOrder.None;
		private ImageList state_image_list;
		internal bool updating;
		private View view = View.LargeIcon;
		private int layout_wd;    // We might draw more than our client area
		private int layout_ht;    // therefore we need to have these two.
		internal HeaderControl header_control;
		internal ItemControl item_control;
		internal ScrollBar h_scroll; // used for scrolling horizontally
		internal ScrollBar v_scroll; // used for scrolling vertically
		internal int h_marker;		// Position markers for scrolling
		internal int v_marker;
		private int keysearch_tickcnt;
		private string keysearch_text;
		static private readonly int keysearch_keydelay = 1000;
		private int[] reordered_column_indices;
		private int[] reordered_items_indices;
		private Point [] items_location;
		private ItemMatrixLocation [] items_matrix_location;
		private Size item_size; // used for caching item size
		private int custom_column_width; // used when using Columns with SmallIcon/List views
		private int hot_item_index = -1;
#if NET_2_0
		private bool hot_tracking;
		private ListViewInsertionMark insertion_mark;
		private bool show_item_tooltips;
		private ToolTip item_tooltip;
		private Size tile_size;
		private bool virtual_mode;
		private int virtual_list_size;
		private bool right_to_left_layout;
#endif
		// selection is available after the first time the handle is created, *even* if later
		// the handle is either recreated or destroyed - so keep this info around.
		private bool is_selection_available;

		// internal variables
		internal ImageList large_image_list;
		internal ImageList small_image_list;
		internal Size text_size = Size.Empty;

		#region Events
		static object AfterLabelEditEvent = new object ();
		static object BeforeLabelEditEvent = new object ();
		static object ColumnClickEvent = new object ();
		static object ItemActivateEvent = new object ();
		static object ItemCheckEvent = new object ();
		static object ItemDragEvent = new object ();
		static object SelectedIndexChangedEvent = new object ();
#if NET_2_0
		static object DrawColumnHeaderEvent = new object();
		static object DrawItemEvent = new object();
		static object DrawSubItemEvent = new object();
		static object ItemCheckedEvent = new object ();
		static object ItemMouseHoverEvent = new object ();
		static object ItemSelectionChangedEvent = new object ();
		static object CacheVirtualItemsEvent = new object ();
		static object RetrieveVirtualItemEvent = new object ();
		static object RightToLeftLayoutChangedEvent = new object ();
		static object SearchForVirtualItemEvent = new object ();
		static object VirtualItemsSelectionRangeChangedEvent = new object ();
#endif

		public event LabelEditEventHandler AfterLabelEdit {
			add { Events.AddHandler (AfterLabelEditEvent, value); }
			remove { Events.RemoveHandler (AfterLabelEditEvent, value); }
		}

#if !NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}
#endif

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}
#endif

		public event LabelEditEventHandler BeforeLabelEdit {
			add { Events.AddHandler (BeforeLabelEditEvent, value); }
			remove { Events.RemoveHandler (BeforeLabelEditEvent, value); }
		}

		public event ColumnClickEventHandler ColumnClick {
			add { Events.AddHandler (ColumnClickEvent, value); }
			remove { Events.RemoveHandler (ColumnClickEvent, value); }
		}

#if NET_2_0
		public event DrawListViewColumnHeaderEventHandler DrawColumnHeader {
			add { Events.AddHandler(DrawColumnHeaderEvent, value); }
			remove { Events.RemoveHandler(DrawColumnHeaderEvent, value); }
		}

		public event DrawListViewItemEventHandler DrawItem {
			add { Events.AddHandler(DrawItemEvent, value); }
			remove { Events.RemoveHandler(DrawItemEvent, value); }
		}

		public event DrawListViewSubItemEventHandler DrawSubItem {
			add { Events.AddHandler(DrawSubItemEvent, value); }
			remove { Events.RemoveHandler(DrawSubItemEvent, value); }
		}
#endif

		public event EventHandler ItemActivate {
			add { Events.AddHandler (ItemActivateEvent, value); }
			remove { Events.RemoveHandler (ItemActivateEvent, value); }
		}

		public event ItemCheckEventHandler ItemCheck {
			add { Events.AddHandler (ItemCheckEvent, value); }
			remove { Events.RemoveHandler (ItemCheckEvent, value); }
		}

#if NET_2_0
		public event ItemCheckedEventHandler ItemChecked {
			add { Events.AddHandler (ItemCheckedEvent, value); }
			remove { Events.RemoveHandler (ItemCheckedEvent, value); }
		}
#endif

		public event ItemDragEventHandler ItemDrag {
			add { Events.AddHandler (ItemDragEvent, value); }
			remove { Events.RemoveHandler (ItemDragEvent, value); }
		}

#if NET_2_0
		public event ListViewItemMouseHoverEventHandler ItemMouseHover {
			add { Events.AddHandler (ItemMouseHoverEvent, value); }
			remove { Events.RemoveHandler (ItemMouseHoverEvent, value); }
		}

		public event ListViewItemSelectionChangedEventHandler ItemSelectionChanged {
			add { Events.AddHandler (ItemSelectionChangedEvent, value); }
			remove { Events.RemoveHandler (ItemSelectionChangedEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler PaddingChanged {
			add { base.PaddingChanged += value; }
			remove { base.PaddingChanged -= value; }
		}
#endif

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
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

#if NET_2_0
		public event CacheVirtualItemsEventHandler CacheVirtualItems {
			add { Events.AddHandler (CacheVirtualItemsEvent, value); }
			remove { Events.RemoveHandler (CacheVirtualItemsEvent, value); }
		}

		public event RetrieveVirtualItemEventHandler RetrieveVirtualItem {
			add { Events.AddHandler (RetrieveVirtualItemEvent, value); }
			remove { Events.RemoveHandler (RetrieveVirtualItemEvent, value); }
		}

		public event EventHandler RightToLeftLayoutChanged {
			add { Events.AddHandler (RightToLeftLayoutChangedEvent, value); }
			remove { Events.RemoveHandler (RightToLeftLayoutChangedEvent, value); }
		}

		public event SearchForVirtualItemEventHandler SearchForVirtualItem {
			add { Events.AddHandler (SearchForVirtualItemEvent, value); }
			remove { Events.AddHandler (SearchForVirtualItemEvent, value); }
		}
		
		public event ListViewVirtualItemsSelectionRangeChangedEventHandler VirtualItemsSelectionRangeChanged {
			add { Events.AddHandler (VirtualItemsSelectionRangeChangedEvent, value); }
			remove { Events.RemoveHandler (VirtualItemsSelectionRangeChangedEvent, value); }
		}
#endif

		#endregion // Events

		#region Public Constructors
		public ListView ()
		{
			background_color = ThemeEngine.Current.ColorWindow;
#if NET_2_0
			groups = new ListViewGroupCollection (this);
#endif
			items = new ListViewItemCollection (this);
			items.Changed += new CollectionChangedHandler (OnItemsChanged);
			checked_indices = new CheckedIndexCollection (this);
			checked_items = new CheckedListViewItemCollection (this);
			columns = new ColumnHeaderCollection (this);
			foreground_color = SystemColors.WindowText;
			selected_indices = new SelectedIndexCollection (this);
			selected_items = new SelectedListViewItemCollection (this);
			items_location = new Point [16];
			items_matrix_location = new ItemMatrixLocation [16];
			reordered_items_indices = new int [16];
#if NET_2_0
			item_tooltip = new ToolTip ();
			item_tooltip.Active = false;
			insertion_mark = new ListViewInsertionMark (this);
#endif

			InternalBorderStyle = BorderStyle.Fixed3D;

			header_control = new HeaderControl (this);
			header_control.Visible = false;
			Controls.AddImplicit (header_control);

			item_control = new ItemControl (this);
			Controls.AddImplicit (item_control);

			h_scroll = new ImplicitHScrollBar ();
			Controls.AddImplicit (this.h_scroll);

			v_scroll = new ImplicitVScrollBar ();
			Controls.AddImplicit (this.v_scroll);

			h_marker = v_marker = 0;
			keysearch_tickcnt = 0;

			// scroll bars are disabled initially
			h_scroll.Visible = false;
			h_scroll.ValueChanged += new EventHandler(HorizontalScroller);
			v_scroll.Visible = false;
			v_scroll.ValueChanged += new EventHandler(VerticalScroller);

			// event handlers
			base.KeyDown += new KeyEventHandler(ListView_KeyDown);
			SizeChanged += new EventHandler (ListView_SizeChanged);
			GotFocus += new EventHandler (FocusChanged);
			LostFocus += new EventHandler (FocusChanged);
			MouseWheel += new MouseEventHandler(ListView_MouseWheel);
			MouseEnter += new EventHandler (ListView_MouseEnter);
			Invalidated += new InvalidateEventHandler (ListView_Invalidated);

#if NET_2_0
			BackgroundImageTiled = false;
#endif

			this.SetStyle (ControlStyles.UserPaint | ControlStyles.StandardClick
#if NET_2_0
				| ControlStyles.UseTextForAccessibility
#endif
				, false);
		}
		#endregion	// Public Constructors

		#region Private Internal Properties
		internal Size CheckBoxSize {
			get {
				if (this.check_boxes) {
					if (this.state_image_list != null)
						return this.state_image_list.ImageSize;
					else
						return ThemeEngine.Current.ListViewCheckBoxSize;
				}
				return Size.Empty;
			}
		}

		internal Size ItemSize {
			get {
				if (view != View.Details)
					return item_size;

				Size size = new Size ();
				size.Height = item_size.Height;
				for (int i = 0; i < columns.Count; i++)
					size.Width += columns [i].Wd;

				return size;
			}
			set {
				item_size = value;
			}
		}

		internal int HotItemIndex {
			get {
				return hot_item_index;
			}
			set {
				hot_item_index = value;
			}
		}

#if NET_2_0
		internal bool UsingGroups {
			get {
				return show_groups && groups.Count > 0 && view != View.List && 
					Application.VisualStylesEnabled;
			}
		}
#endif

		internal override bool ScaleChildrenInternal {
			get { return false; }
		}

		internal bool UseCustomColumnWidth {
			get {
				return (view == View.List || view == View.SmallIcon) && columns.Count > 0;
			}
		}

		internal ColumnHeader EnteredColumnHeader {
			get {
				return header_control.EnteredColumnHeader;
			}
		}
		#endregion	// Private Internal Properties

		#region	 Protected Properties
		protected override CreateParams CreateParams {
			get { return base.CreateParams; }
		}

		protected override Size DefaultSize {
			get { return ThemeEngine.Current.ListViewDefaultSize; }
		}
#if NET_2_0
		protected override bool DoubleBuffered {
			get {
				return base.DoubleBuffered;
			}
			set {
				base.DoubleBuffered = value;
			}
		}
#endif
		#endregion	// Protected Properties

		#region Public Instance Properties
		[DefaultValue (ItemActivation.Standard)]
		public ItemActivation Activation {
			get { return activation; }
			set { 
				if (value != ItemActivation.Standard && value != ItemActivation.OneClick && 
					value != ItemActivation.TwoClick) {
					throw new InvalidEnumArgumentException (string.Format
						("Enum argument value '{0}' is not valid for Activation", value));
				}
#if NET_2_0
				if (hot_tracking && value != ItemActivation.OneClick)
					throw new ArgumentException ("When HotTracking is on, activation must be ItemActivation.OneClick");
#endif
				
				activation = value;
			}
		}

		[DefaultValue (ListViewAlignment.Top)]
		[Localizable (true)]
		public ListViewAlignment Alignment {
			get { return alignment; }
			set {
				if (value != ListViewAlignment.Default && value != ListViewAlignment.Left && 
					value != ListViewAlignment.SnapToGrid && value != ListViewAlignment.Top) {
					throw new InvalidEnumArgumentException (string.Format 
						("Enum argument value '{0}' is not valid for Alignment", value));
				}
				
				if (this.alignment != value) {
					alignment = value;
					// alignment does not matter in Details/List views
					if (this.view == View.LargeIcon || this.View == View.SmallIcon)
						this.Redraw (true);
				}
			}
		}

		[DefaultValue (false)]
		public bool AllowColumnReorder {
			get { return allow_column_reorder; }
			set { allow_column_reorder = value; }
		}

		[DefaultValue (true)]
		public bool AutoArrange {
			get { return auto_arrange; }
			set {
				if (auto_arrange != value) {
					auto_arrange = value;
					// autoarrange does not matter in Details/List views
					if (this.view == View.LargeIcon || this.View == View.SmallIcon)
						this.Redraw (true);
				}
			}
		}

		public override Color BackColor {
			get {
				if (background_color.IsEmpty)
					return ThemeEngine.Current.ColorWindow;
				else
					return background_color;
			}
			set { 
				background_color = value;
				item_control.BackColor = value;
			}
		}

#if !NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}
#endif

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout {
			get {
				return base.BackgroundImageLayout;
			}
			set {
				base.BackgroundImageLayout = value;
			}
		}

		[DefaultValue (false)]
		public bool BackgroundImageTiled {
			get {
				return item_control.BackgroundImageLayout == ImageLayout.Tile;
			}
			set {
				ImageLayout new_image_layout = value ? ImageLayout.Tile : ImageLayout.None;
				if (new_image_layout == item_control.BackgroundImageLayout)
					return;

				item_control.BackgroundImageLayout = new_image_layout;
			}
		}
#endif

		[DefaultValue (BorderStyle.Fixed3D)]
		[DispId (-504)]
		public BorderStyle BorderStyle {
			get { return InternalBorderStyle; }
			set { InternalBorderStyle = value; }
		}

		[DefaultValue (false)]
		public bool CheckBoxes {
			get { return check_boxes; }
			set {
				if (check_boxes != value) {
#if NET_2_0
					if (value && View == View.Tile)
						throw new NotSupportedException ("CheckBoxes are not"
							+ " supported in Tile view. Choose a different"
							+ " view or set CheckBoxes to false.");
#endif

					check_boxes = value;
					this.Redraw (true);

#if NET_2_0
					//UIA Framework: Event used by ListView to set/unset Toggle Pattern
					OnUIACheckBoxesChanged ();
#endif
				}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public CheckedIndexCollection CheckedIndices {
			get { return checked_indices; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public CheckedListViewItemCollection CheckedItems {
			get { return checked_items; }
		}

#if NET_2_0
		[Editor ("System.Windows.Forms.Design.ColumnHeaderCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Localizable (true)]
		[MergableProperty (false)]
		public ColumnHeaderCollection Columns {
			get { return columns; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ListViewItem FocusedItem {
			get {
				if (focused_item_index == -1)
					return null;

				return GetItemAtDisplayIndex (focused_item_index);
			}
#if NET_2_0
			set {
				if (value == null || value.ListView != this || 
						!IsHandleCreated)
					return;

				SetFocusedItem (value.DisplayIndex);
			}
#endif
		}

		public override Color ForeColor {
			get {
				if (foreground_color.IsEmpty)
					return ThemeEngine.Current.ColorWindowText;
				else
					return foreground_color;
			}
			set { foreground_color = value; }
		}

		[DefaultValue (false)]
		public bool FullRowSelect {
			get { return full_row_select; }
			set { 
				if (full_row_select != value) {
					full_row_select = value;
					InvalidateSelection ();
				}
			}
		}

		[DefaultValue (false)]
		public bool GridLines {
			get { return grid_lines; }
			set {
				if (grid_lines != value) {
					grid_lines = value;
					this.Redraw (false);
				}
			}
		}

		[DefaultValue (ColumnHeaderStyle.Clickable)]
		public ColumnHeaderStyle HeaderStyle {
			get { return header_style; }
			set {
				if (header_style == value)
					return;

				switch (value) {
				case ColumnHeaderStyle.Clickable:
				case ColumnHeaderStyle.Nonclickable:
				case ColumnHeaderStyle.None:
					break;
				default:
					throw new InvalidEnumArgumentException (string.Format 
						("Enum argument value '{0}' is not valid for ColumnHeaderStyle", value));
				}
				
				header_style = value;
				if (view == View.Details)
					Redraw (true);
			}
		}

		[DefaultValue (true)]
		public bool HideSelection {
			get { return hide_selection; }
			set {
				if (hide_selection != value) {
					hide_selection = value;
					InvalidateSelection ();
				}
			}
		}

#if NET_2_0
		[DefaultValue (false)]
		public bool HotTracking {
			get {
				return hot_tracking;
			}
			set {
				if (hot_tracking == value)
					return;
				
				hot_tracking = value;
				if (hot_tracking) {
					hover_selection = true;
					activation = ItemActivation.OneClick;
				}
			}
		}
#endif

		[DefaultValue (false)]
		public bool HoverSelection {
			get { return hover_selection; }
			set { 
#if NET_2_0
				if (hot_tracking && value == false)
					throw new ArgumentException ("When HotTracking is on, hover selection must be true");
#endif
				hover_selection = value; 
			}
		}

#if NET_2_0
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public ListViewInsertionMark InsertionMark {
			get {
				return insertion_mark;
			}
		}
#endif

#if NET_2_0
		[Editor ("System.Windows.Forms.Design.ListViewItemCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
#endif
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Localizable (true)]
		[MergableProperty (false)]
		public ListViewItemCollection Items {
			get { return items; }
		}

		[DefaultValue (false)]
		public bool LabelEdit {
			get { return label_edit; }
			set { 
				if (value != label_edit) {
					label_edit = value; 

#if NET_2_0
					// UIA Framework: Event used by Value Pattern in ListView.ListItem provider
					OnUIALabelEditChanged ();
#endif
				}

			}
		}

		[DefaultValue (true)]
		[Localizable (true)]
		public bool LabelWrap {
			get { return label_wrap; }
			set {
				if (label_wrap != value) {
					label_wrap = value;
					this.Redraw (true);
				}
			}
		}

		[DefaultValue (null)]
		public ImageList LargeImageList {
			get { return large_image_list; }
			set {
				large_image_list = value;
				this.Redraw (true);
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public IComparer ListViewItemSorter {
			get {
				if (View != View.SmallIcon && View != View.LargeIcon && item_sorter is ItemComparer)
					return null;
				return item_sorter;
			}
			set {
				if (item_sorter != value) {
					item_sorter = value;
					Sort ();
				}
			}
		}

		[DefaultValue (true)]
		public bool MultiSelect {
			get { return multiselect; }
			set {
				if (value != multiselect) {
					multiselect = value; 

#if NET_2_0
					// UIA Framework: Event used by Selection Pattern in ListView.ListItem provider
					OnUIAMultiSelectChanged ();
#endif
				}
			}
		}


#if NET_2_0
		[DefaultValue(false)]
		public bool OwnerDraw {
			get { return owner_draw; }
			set { 
				owner_draw = value;
				Redraw (true);
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Padding Padding {
			get {
				return base.Padding;
			}
			set {
				base.Padding = value;
			}
		}
		
		[MonoTODO ("RTL not supported")]
		[Localizable (true)]
		[DefaultValue (false)]
		public virtual bool RightToLeftLayout {
			get { return right_to_left_layout; }
			set { 
				if (right_to_left_layout != value) {
					right_to_left_layout = value;
					OnRightToLeftLayoutChanged (EventArgs.Empty);
				}
			}
		}
#endif

		[DefaultValue (true)]
		public bool Scrollable {
			get { return scrollable; }
			set {
				if (scrollable != value) {
					scrollable = value;
					this.Redraw (true);
				}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public SelectedIndexCollection SelectedIndices {
			get { return selected_indices; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public SelectedListViewItemCollection SelectedItems {
			get { return selected_items; }
		}

#if NET_2_0
		[DefaultValue(true)]
		public bool ShowGroups {
			get { return show_groups; }
			set {
				if (show_groups != value) {
					show_groups = value;
					Redraw(true);

					// UIA Framework: Used to update a11y Tree
					OnUIAShowGroupsChanged ();
				}
			}
		}

		[LocalizableAttribute (true)]
		[MergableProperty (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Editor ("System.Windows.Forms.Design.ListViewGroupCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public ListViewGroupCollection Groups {
			get { return groups; }
		}

		[DefaultValue (false)]
		public bool ShowItemToolTips {
			get {
				return show_item_tooltips;
			}
			set {
				show_item_tooltips = value;
				item_tooltip.Active = false;
			}
		}
#endif

		[DefaultValue (null)]
		public ImageList SmallImageList {
			get { return small_image_list; }
			set {
				small_image_list = value;
				this.Redraw (true);
			}
		}

		[DefaultValue (SortOrder.None)]
		public SortOrder Sorting {
			get { return sort_order; }
			set { 
				if (!Enum.IsDefined (typeof (SortOrder), value)) {
					throw new InvalidEnumArgumentException ("value", (int) value,
						typeof (SortOrder));
				}
				
				if (sort_order == value)
					return;

				sort_order = value;

#if NET_2_0
				if (virtual_mode) // Sorting is not allowed in virtual mode
					return;
#endif

				if (value == SortOrder.None) {
					if (item_sorter != null) {
						// ListViewItemSorter should never be reset for SmallIcon
						// and LargeIcon view
						if (View != View.SmallIcon && View != View.LargeIcon)
#if NET_2_0
							item_sorter = null;
#else
							// in .NET 1.1, only internal IComparer would be
							// set to null
							if (item_sorter is ItemComparer)
								item_sorter = null;
#endif
					}
					this.Redraw (false);
				} else {
					if (item_sorter == null)
						item_sorter = new ItemComparer (value);
					if (item_sorter is ItemComparer) {
#if NET_2_0
						item_sorter = new ItemComparer (value);
#else
						// in .NET 1.1, the sort order is not updated for
						// SmallIcon and LargeIcon views if no custom IComparer
						// is set
						if (View != View.SmallIcon && View != View.LargeIcon)
							item_sorter = new ItemComparer (value);
#endif
					}
					Sort ();
				}
			}
		}

		private void OnImageListChanged (object sender, EventArgs args)
		{
			item_control.Invalidate ();
		}

		[DefaultValue (null)]
		public ImageList StateImageList {
			get { return state_image_list; }
			set {
				if (state_image_list == value)
					return;

				if (state_image_list != null)
					state_image_list.Images.Changed -= new EventHandler (OnImageListChanged);

				state_image_list = value;

				if (state_image_list != null)
					state_image_list.Images.Changed += new EventHandler (OnImageListChanged);

				this.Redraw (true);
			}
		}

		[Bindable (false)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string Text {
			get { return base.Text; } 
			set {
				if (value == base.Text)
					return;

				base.Text = value;
				this.Redraw (true);
			}
		}

#if NET_2_0
		[Browsable (true)]
		public Size TileSize {
			get {
				return tile_size;
			}
			set {
				if (value.Width <= 0 || value.Height <= 0)
					throw new ArgumentOutOfRangeException ("value");

				tile_size = value;
				if (view == View.Tile)
					Redraw (true);
			}
		}
#endif

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ListViewItem TopItem {
			get {
#if NET_2_0
				if (view == View.LargeIcon || view == View.SmallIcon || view == View.Tile)
					throw new InvalidOperationException ("Cannot get the top item in LargeIcon, SmallIcon or Tile view.");
#endif
				// there is no item
				if (this.items.Count == 0)
					return null;
				// if contents are not scrolled
				// it is the first item
				else if (h_marker == 0 && v_marker == 0)
					return this.items [0];
				// do a hit test for the scrolled position
				else {
					int header_offset = header_control.Height;
					for (int i = 0; i < items.Count; i++) {
						Point item_loc = GetItemLocation (i);
						if (item_loc.X >= 0 && item_loc.Y - header_offset >= 0)
							return items [i];
					}
					return null;
				}
			}
#if NET_2_0
			set {
				if (view == View.LargeIcon || view == View.SmallIcon || view == View.Tile)
					throw new InvalidOperationException ("Cannot set the top item in LargeIcon, SmallIcon or Tile view.");

				// .Net doesn't throw any exception in the cases below
				if (value == null || value.ListView != this)
					return;

				// Take advantage this property is only valid for Details view.
				SetScrollValue (v_scroll, item_size.Height * value.Index);
			}
#endif
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[DefaultValue (true)]
		[Browsable (false)]
		[MonoInternalNote ("Stub, not implemented")]
		public bool UseCompatibleStateImageBehavior {
			get {
				return false;
			}
			set {
			}
		}
#endif

		[DefaultValue (View.LargeIcon)]
		public View View {
			get { return view; }
			set { 
				if (!Enum.IsDefined (typeof (View), value))
					throw new InvalidEnumArgumentException ("value", (int) value,
						typeof (View));

				if (view != value) {
#if NET_2_0
					if (CheckBoxes && value == View.Tile)
						throw new NotSupportedException ("CheckBoxes are not"
							+ " supported in Tile view. Choose a different"
							+ " view or set CheckBoxes to false.");
					if (VirtualMode && value == View.Tile)
						throw new NotSupportedException ("VirtualMode is"
							+ " not supported in Tile view. Choose a different"
							+ " view or set ViewMode to false.");
#endif

					h_scroll.Value = v_scroll.Value = 0;
					view = value; 
					Redraw (true);

#if NET_2_0
					// UIA Framework: Event used to update UIA Tree.
					OnUIAViewChanged ();
#endif
				}
			}
		}

#if NET_2_0
		[DefaultValue (false)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public bool VirtualMode {
			get {
				return virtual_mode;
			}
			set {
				if (virtual_mode == value)
					return;

				if (!virtual_mode && items.Count > 0)
					throw new InvalidOperationException ();
				if (value && view == View.Tile)
					throw new NotSupportedException ("VirtualMode is"
						+ " not supported in Tile view. Choose a different"
						+ " view or set ViewMode to false.");

				virtual_mode = value;
				Redraw (true);
			}
		}

		[DefaultValue (0)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public int VirtualListSize {
			get {
				return virtual_list_size;
			}
			set {
				if (value < 0)
					throw new ArgumentException ("value");

				if (virtual_list_size == value)
					return;

				virtual_list_size = value;
				if (virtual_mode) {
					selected_indices.Reset ();
					Redraw (true);
				}
			}
		}
#endif
		#endregion	// Public Instance Properties

		#region Internal Methods Properties
		
		internal int FirstVisibleIndex {
			get {
				// there is no item
				if (this.items.Count == 0)
					return 0;
				
				if (h_marker == 0 && v_marker == 0)
					return 0;
				
				Size item_size = ItemSize;
#if NET_2_0
				// In virtual mode we always have fixed positions, and we can infer the positon easily
				if (virtual_mode) {
					int first = 0;
					switch (view) {
						case View.Details:
							first = v_marker / item_size.Height;
							break;
						case View.LargeIcon:
						case View.SmallIcon:
							first = (v_marker / (item_size.Height + y_spacing)) * cols;
							break;
						case View.List:
							first = (h_marker / (item_size.Width * x_spacing)) * rows;
							break;
					}

					if (first >= items.Count)
						first = items.Count;

					return first;
				}
#endif
				for (int i = 0; i < items.Count; i++) {
					Rectangle item_rect = new Rectangle (GetItemLocation (i), item_size);
					if (item_rect.Right >= 0 && item_rect.Bottom >= 0)
						return i;
				}

				return 0;
			}
		}

		
		internal int LastVisibleIndex {
			get {
				for (int i = FirstVisibleIndex; i < Items.Count; i++) {
					if (View == View.List || Alignment == ListViewAlignment.Left) {
						if (GetItemLocation (i).X > item_control.ClientRectangle.Right)
							return i - 1;
					} else {
						if (GetItemLocation (i).Y > item_control.ClientRectangle.Bottom)
							return i - 1;
					}
				}
				
				return Items.Count - 1;
			}
		}

		internal void OnSelectedIndexChanged ()
		{
			if (is_selection_available)
				OnSelectedIndexChanged (EventArgs.Empty);
		}

		internal int TotalWidth {
			get { return Math.Max (this.Width, this.layout_wd); }
		}

		internal int TotalHeight {
			get { return Math.Max (this.Height, this.layout_ht); }
		}

		internal void Redraw (bool recalculate)
		{
			// Avoid calculations when control is being updated
			if (updating)
				return;
#if NET_2_0
			// VirtualMode doesn't do any calculations until handle is created
			if (virtual_mode && !IsHandleCreated)
				return;
#endif


			if (recalculate)
				CalculateListView (this.alignment);

			Invalidate (true);
		}

		void InvalidateSelection ()
		{
			foreach (int selected_index in SelectedIndices)
				items [selected_index].Invalidate ();
		}

		const int text_padding = 15;

		internal Size GetChildColumnSize (int index)
		{
			Size ret_size = Size.Empty;
			ColumnHeader col = this.columns [index];

			if (col.Width == -2) { // autosize = max(items, columnheader)
				Size size = Size.Ceiling (TextRenderer.MeasureString
					(col.Text, this.Font));
				size.Width += text_padding;
				ret_size = BiggestItem (index);
				if (size.Width > ret_size.Width)
					ret_size = size;
			}
			else { // -1 and all the values < -2 are put under one category
				ret_size = BiggestItem (index);
				// fall back to empty columns' width if no subitem is available for a column
				if (ret_size.IsEmpty) {
					ret_size.Width = ThemeEngine.Current.ListViewEmptyColumnWidth;
					if (col.Text.Length > 0)
						ret_size.Height = Size.Ceiling (TextRenderer.MeasureString
										(col.Text, this.Font)).Height;
					else
						ret_size.Height = this.Font.Height;
				}
			}

			ret_size.Height += text_padding;

			// adjust the size for icon and checkbox for 0th column
			if (index == 0) {
				ret_size.Width += (this.CheckBoxSize.Width + 4);
				if (this.small_image_list != null)
					ret_size.Width += this.small_image_list.ImageSize.Width;
			}
			return ret_size;
		}

		// Returns the size of biggest item text in a column
		// or the sum of the text and indent count if we are on 2.0
		private Size BiggestItem (int col)
		{
			Size temp = Size.Empty;
			Size ret_size = Size.Empty;
#if NET_2_0
    			bool use_indent_count = small_image_list != null;

			// VirtualMode uses the first item text size
			if (virtual_mode && items.Count > 0) {
				ListViewItem item = items [0];
				ret_size = Size.Ceiling (TextRenderer.MeasureString (item.SubItems[col].Text,
							Font));

				if (use_indent_count)
					ret_size.Width += item.IndentCount * small_image_list.ImageSize.Width;
			} else {
#endif
				// 0th column holds the item text, we check the size of
				// the various subitems falling in that column and get
				// the biggest one's size.
				foreach (ListViewItem item in items) {
					if (col >= item.SubItems.Count)
						continue;

					temp = Size.Ceiling (TextRenderer.MeasureString
								(item.SubItems [col].Text, Font));

#if NET_2_0
					if (use_indent_count)
						temp.Width += item.IndentCount * small_image_list.ImageSize.Width;
#endif
    
					if (temp.Width > ret_size.Width)
						ret_size = temp;
				}
#if NET_2_0
			}
#endif

			// adjustment for space in Details view
			if (!ret_size.IsEmpty && view == View.Details)
				ret_size.Width += ThemeEngine.Current.ListViewItemPaddingWidth;

			return ret_size;
		}

		const int max_wrap_padding = 30;

		// Sets the size of the biggest item text as per the view
		private void CalcTextSize ()
		{
			// clear the old value
			text_size = Size.Empty;

			if (items.Count == 0)
				return;

			text_size = BiggestItem (0);

			if (view == View.LargeIcon && this.label_wrap) {
				Size temp = Size.Empty;
				if (this.check_boxes)
					temp.Width += 2 * this.CheckBoxSize.Width;
				int icon_w = LargeImageList == null ? 12 : LargeImageList.ImageSize.Width;
				temp.Width += icon_w + max_wrap_padding;
				// wrapping is done for two lines only
				if (text_size.Width > temp.Width) {
					text_size.Width = temp.Width;
					text_size.Height *= 2;
				}
			}
			else if (view == View.List) {
				// in list view max text shown in determined by the
				// control width, even if scolling is enabled.
				int max_wd = this.Width - (this.CheckBoxSize.Width - 2);
				if (this.small_image_list != null)
					max_wd -= this.small_image_list.ImageSize.Width;

				if (text_size.Width > max_wd)
					text_size.Width = max_wd;
			}

			// we do the default settings, if we have got 0's
			if (text_size.Height <= 0)
				text_size.Height = this.Font.Height;
			if (text_size.Width <= 0)
				text_size.Width = this.Width;

			// little adjustment
			text_size.Width += 2;
			text_size.Height += 2;
		}

		private void SetScrollValue (ScrollBar scrollbar, int val)
		{
			int max;
			if (scrollbar == h_scroll)
				max = h_scroll.Maximum - h_scroll.LargeChange + 1;
			else
				max = v_scroll.Maximum - v_scroll.LargeChange + 1;

			if (val > max)
				val = max;
			else if (val < scrollbar.Minimum)
				val = scrollbar.Minimum;

			scrollbar.Value = val;
		}

		private void Scroll (ScrollBar scrollbar, int delta)
		{
			if (delta == 0 || !scrollbar.Visible)
				return;

			SetScrollValue (scrollbar, scrollbar.Value + delta);
		}

		private void CalculateScrollBars ()
		{
			Rectangle client_area = ClientRectangle;
			int height = client_area.Height;
			int width = client_area.Width;
			Size item_size;
			
			if (!scrollable) {
				h_scroll.Visible = false;
				v_scroll.Visible = false;
				item_control.Size = new Size (width, height);
				header_control.Width = width;
				return;
			}

			// Don't calculate if the view is not displayable
			if (client_area.Height < 0 || client_area.Width < 0)
				return;

			// making a scroll bar visible might make
			// other scroll bar visible
			if (layout_wd > client_area.Right) {
				h_scroll.Visible = true;
				if ((layout_ht + h_scroll.Height) > client_area.Bottom)
					v_scroll.Visible = true;
				else
					v_scroll.Visible = false;
			} else if (layout_ht > client_area.Bottom) {
				v_scroll.Visible = true;
				if ((layout_wd + v_scroll.Width) > client_area.Right)
					h_scroll.Visible = true;
				else
					h_scroll.Visible = false;
			} else {
				h_scroll.Visible = false;
				v_scroll.Visible = false;
			}


			item_size = ItemSize;

			if (h_scroll.is_visible) {
				h_scroll.Location = new Point (client_area.X, client_area.Bottom - h_scroll.Height);
				h_scroll.Minimum = 0;

				// if v_scroll is visible, adjust the maximum of the
				// h_scroll to account for the width of v_scroll
				if (v_scroll.Visible) {
					h_scroll.Maximum = layout_wd + v_scroll.Width;
					h_scroll.Width = client_area.Width - v_scroll.Width;
				}
				else {
					h_scroll.Maximum = layout_wd;
					h_scroll.Width = client_area.Width;
				}

				if (view == View.List)
					h_scroll.SmallChange = item_size.Width + ThemeEngine.Current.ListViewHorizontalSpacing;
				else
					h_scroll.SmallChange = Font.Height;

				h_scroll.LargeChange = client_area.Width;
				height -= h_scroll.Height;
			}

			if (v_scroll.is_visible) {
				v_scroll.Location = new Point (client_area.Right - v_scroll.Width, client_area.Y);
				v_scroll.Minimum = 0;

				// if h_scroll is visible, adjust the height of
				// v_scroll to account for the height of h_scroll
				if (h_scroll.Visible) {
					v_scroll.Maximum = layout_ht + h_scroll.Height;
					v_scroll.Height = client_area.Height - h_scroll.Height;
				} else {
					v_scroll.Maximum = layout_ht;
					v_scroll.Height = client_area.Height;
				}

				if (view == View.Details) {
					// Need to update Maximum if using LargeChange with value other than the visible area
					v_scroll.LargeChange = v_scroll.Height - (header_control.Height + item_size.Height);
					v_scroll.Maximum -= header_control.Height + item_size.Height;
				} else
					v_scroll.LargeChange = v_scroll.Height;

				v_scroll.SmallChange = item_size.Height;
				width -= v_scroll.Width;
			}
			
			item_control.Size = new Size (width, height);

			if (header_control.is_visible)
				header_control.Width = width;
		}

#if NET_2_0
		internal int GetReorderedColumnIndex (ColumnHeader column)
		{
			if (reordered_column_indices == null)
				return column.Index;

			for (int i = 0; i < Columns.Count; i++)
				if (reordered_column_indices [i] == column.Index)
					return i;

			return -1;
		}
#endif

		internal ColumnHeader GetReorderedColumn (int index)
		{
			if (reordered_column_indices == null)
				return Columns [index];
			else
				return Columns [reordered_column_indices [index]];
		}

		internal void ReorderColumn (ColumnHeader col, int index, bool fireEvent)
		{
#if NET_2_0
			if (fireEvent) {
				ColumnReorderedEventHandler eh = (ColumnReorderedEventHandler) (Events [ColumnReorderedEvent]);
				if (eh != null){
					ColumnReorderedEventArgs args = new ColumnReorderedEventArgs (col.Index, index, col);

					eh (this, args);
					if (args.Cancel) {
						header_control.Invalidate ();
						item_control.Invalidate ();
						return;
					}
				}
			}
#endif
			int column_count = Columns.Count;

			if (reordered_column_indices == null) {
				reordered_column_indices = new int [column_count];
				for (int i = 0; i < column_count; i++)
					reordered_column_indices [i] = i;
			}

			if (reordered_column_indices [index] == col.Index)
				return;

			int[] curr = reordered_column_indices;
			int [] result = new int [column_count];
			int curr_idx = 0;
			for (int i = 0; i < column_count; i++) {
				if (curr_idx < column_count && curr [curr_idx] == col.Index)
					curr_idx++;

				if (i == index)
					result [i] = col.Index;
				else
					result [i] = curr [curr_idx++];
			}

			ReorderColumns (result, true);
		}

		internal void ReorderColumns (int [] display_indices, bool redraw)
		{
			reordered_column_indices = display_indices;
			for (int i = 0; i < Columns.Count; i++) {
				ColumnHeader col = Columns [i];
				col.InternalDisplayIndex = reordered_column_indices [i];
			}
			if (redraw && view == View.Details && IsHandleCreated) {
				LayoutDetails ();
				header_control.Invalidate ();
				item_control.Invalidate ();
			}
		}

		internal void AddColumn (ColumnHeader newCol, int index, bool redraw)
		{
			int column_count = Columns.Count;
			newCol.SetListView (this);

			int [] display_indices = new int [column_count];
			for (int i = 0; i < column_count; i++) {
				ColumnHeader col = Columns [i];
				if (i == index) {
					display_indices [i] = index;
				} else {
					int display_index = col.InternalDisplayIndex;
					if (display_index < index) {
						display_indices [i] = display_index;
					} else {
						display_indices [i] = (display_index + 1);
					}
				}
			}

			ReorderColumns (display_indices, redraw);
			Invalidate ();
		}

		Size LargeIconItemSize
		{
			get {
				int image_w = LargeImageList == null ? 12 : LargeImageList.ImageSize.Width;
				int image_h = LargeImageList == null ? 2 : LargeImageList.ImageSize.Height;
				int h = text_size.Height + 2 + Math.Max (CheckBoxSize.Height, image_h);
				int w = Math.Max (text_size.Width, image_w);

				if (check_boxes)
					w += 2 + CheckBoxSize.Width;

				return new Size (w, h);
			}
		}

		Size SmallIconItemSize {
			get {
				int image_w = SmallImageList == null ? 0 : SmallImageList.ImageSize.Width;
				int image_h = SmallImageList == null ? 0 : SmallImageList.ImageSize.Height;
				int h = Math.Max (text_size.Height, Math.Max (CheckBoxSize.Height, image_h));
				int w = text_size.Width + image_w;

				if (check_boxes)
					w += 2 + CheckBoxSize.Width;

				return new Size (w, h);
			}
		}

#if NET_2_0
		Size TileItemSize {
			get {
				// Calculate tile size if needed
				// It appears that using Font.Size instead of a SizeF value can give us
				// a slightly better approach to the proportions defined in .Net
				if (tile_size == Size.Empty) {
					int image_w = LargeImageList == null ? 0 : LargeImageList.ImageSize.Width;
					int image_h = LargeImageList == null ? 0 : LargeImageList.ImageSize.Height;
					int w = (int)Font.Size * ThemeEngine.Current.ListViewTileWidthFactor + image_w + 4;
					int h = Math.Max ((int)Font.Size * ThemeEngine.Current.ListViewTileHeightFactor, image_h);
				
					tile_size = new Size (w, h);
				}
			
				return tile_size;
			}
		}
#endif

		int GetDetailsItemHeight ()
		{
			int item_height;
			int checkbox_height = CheckBoxes ? CheckBoxSize.Height : 0;
			int small_image_height = SmallImageList == null ? 0 : SmallImageList.ImageSize.Height;
			item_height = Math.Max (checkbox_height, text_size.Height);
			item_height = Math.Max (item_height, small_image_height);
			return item_height;
		}

		void SetItemLocation (int index, int x, int y, int row, int col)
		{
			Point old_location = items_location [index];
			if (old_location.X == x && old_location.Y == y)
				return;

			items_location [index] = new Point (x, y);
			items_matrix_location [index] = new ItemMatrixLocation (row, col);

			//
			// Initial position matches item's position in ListViewItemCollection
			//
			reordered_items_indices [index] = index;
		}

#if NET_2_0
		void ShiftItemsPositions (int from, int to, bool forward)
		{
			if (forward) {
				for (int i = to + 1; i > from; i--) {
					reordered_items_indices [i] = reordered_items_indices [i - 1];

					ListViewItem item = items [reordered_items_indices [i]];
					item.Invalidate ();
					item.DisplayIndex = i;
					item.Invalidate ();
				}
			} else {
				for (int i = from - 1; i < to; i++) {
					reordered_items_indices [i] = reordered_items_indices [i + 1];

					ListViewItem item = items [reordered_items_indices [i]];
					item.Invalidate ();
					item.DisplayIndex = i;
					item.Invalidate ();
				}
			}
		}

		internal void ChangeItemLocation (int display_index, Point new_pos)
		{
			int new_display_index = GetDisplayIndexFromLocation (new_pos);
			if (new_display_index == display_index)
				return;

			int item_index = reordered_items_indices [display_index];
			ListViewItem item = items [item_index];

			bool forward = new_display_index < display_index;
			int index_from, index_to;
			if (forward) {
				index_from = new_display_index;
				index_to = display_index - 1;
			} else {
				index_from = display_index + 1;
				index_to = new_display_index;
			}

			ShiftItemsPositions (index_from, index_to, forward);

			reordered_items_indices [new_display_index] = item_index;

			item.Invalidate ();
			item.DisplayIndex = new_display_index;
			item.Invalidate ();
		}

		int GetDisplayIndexFromLocation (Point loc)
		{
			int display_index = -1;
			Rectangle item_area;

			// First item
			if (loc.X < 0 || loc.Y < 0)
				return 0;

			// Adjustment to put in the next position refered by 'loc'
			loc.X -= item_size.Width / 2;
			if (loc.X < 0)
				loc.X = 0;

			for (int i = 0; i < items.Count; i++) {
				item_area = new Rectangle (GetItemLocation (i), item_size);
				item_area.Inflate (ThemeEngine.Current.ListViewHorizontalSpacing,
						ThemeEngine.Current.ListViewVerticalSpacing);

				if (item_area.Contains (loc)) {
					display_index = i;
					break;
				}
			}

			// Put in in last position
			if (display_index == -1)
				display_index = items.Count - 1;

			return display_index;
		}

		// When using groups, the items with no group assigned
		// belong to the DefaultGroup
		int GetDefaultGroupItems ()
		{
			int count = 0;
			foreach (ListViewItem item in items)
				if (item.Group == null)
					count++;

			return count;
		}
#endif

#if NET_2_0
		// cache the spacing to let virtualmode compute the positions on the fly
		int x_spacing;
		int y_spacing;
#endif
		int rows;
		int cols;
		int[,] item_index_matrix;

		void CalculateRowsAndCols (Size item_size, bool left_aligned, int x_spacing, int y_spacing)
		{
			Rectangle area = ClientRectangle;

			if (UseCustomColumnWidth)
				CalculateCustomColumnWidth ();
#if NET_2_0
			if (UsingGroups) {
				// When groups are used the alignment is always top-aligned
				rows = 0;
				cols = 0;
				int items = 0;

				groups.DefaultGroup.ItemCount = GetDefaultGroupItems ();
				for (int i = 0; i < groups.InternalCount; i++) {
					ListViewGroup group = groups.GetInternalGroup (i);
					int items_in_group = group.GetActualItemCount ();

					if (items_in_group == 0)
						continue;

					int group_cols = (int) Math.Floor ((double)(area.Width - v_scroll.Width + x_spacing) / (double)(item_size.Width + x_spacing));
					if (group_cols <= 0)
						group_cols = 1;
					int group_rows = (int) Math.Ceiling ((double)items_in_group / (double)group_cols);

					group.starting_row = rows;
					group.rows = group_rows;
					group.starting_item = items;
					group.current_item = 0; // Reset layout

					cols = Math.Max (group_cols, cols);
					rows += group_rows;
					items += items_in_group;
				}
			} else
#endif
			{
				// Simple matrix if no groups are used
				if (left_aligned) {
					rows = (int) Math.Floor ((double)(area.Height - h_scroll.Height + y_spacing) / (double)(item_size.Height + y_spacing));
					if (rows <= 0)
						rows = 1;
					cols = (int) Math.Ceiling ((double)items.Count / (double)rows);
				} else {
					if (UseCustomColumnWidth)
						cols = (int) Math.Floor ((double)(area.Width - v_scroll.Width) / (double)(custom_column_width));
					else
						cols = (int) Math.Floor ((double)(area.Width - v_scroll.Width + x_spacing) / (double)(item_size.Width + x_spacing));

					if (cols < 1)
						cols = 1;

					rows = (int) Math.Ceiling ((double)items.Count / (double)cols);
				}
			}

			item_index_matrix = new int [rows, cols];
		}

		// When using custom column width, we look for the minimum one
		void CalculateCustomColumnWidth ()
		{
			int min_width = Int32.MaxValue;
			for (int i = 0; i < columns.Count; i++) {
				int col_width = columns [i].Width;

				if (col_width < min_width)
					min_width = col_width;
			}

			custom_column_width = min_width;
		}

		void LayoutIcons (Size item_size, bool left_aligned, int x_spacing, int y_spacing)
		{
			header_control.Visible = false;
			header_control.Size = Size.Empty;
			item_control.Visible = true;
			item_control.Location = Point.Empty;
			ItemSize = item_size; // Cache item size
#if NET_2_0
			this.x_spacing = x_spacing;
			this.y_spacing = y_spacing;
#endif

			if (items.Count == 0)
				return;

			Size sz = item_size;

			CalculateRowsAndCols (sz, left_aligned, x_spacing, y_spacing);

			layout_wd = UseCustomColumnWidth ? cols * custom_column_width : cols * (sz.Width + x_spacing) - x_spacing;
			layout_ht = rows * (sz.Height + y_spacing) - y_spacing;

#if NET_2_0
			if (virtual_mode) { // no actual assignment is needed on items for virtual mode
				item_control.Size = new Size (layout_wd, layout_ht);
				return;
			}

			bool using_groups = UsingGroups;
			if (using_groups) // the groups layout will override layout_ht
				CalculateGroupsLayout (sz, y_spacing, 0);
#endif

			int row = 0, col = 0;
			int x = 0, y = 0;
			int display_index = 0;

			for (int i = 0; i < items.Count; i++) {
				ListViewItem item = items [i];
#if NET_2_0
				if (using_groups) {
					ListViewGroup group = item.Group;
					if (group == null)
						group = groups.DefaultGroup;

					Point group_items_loc = group.items_area_location;
					int current_item = group.current_item++;
					int starting_row = group.starting_row;

					display_index = group.starting_item + current_item;
					row = (current_item / cols);
					col = current_item % cols;

					x = UseCustomColumnWidth ? col * custom_column_width : col * (item_size.Width + x_spacing);
					y = row * (item_size.Height + y_spacing) + group_items_loc.Y;

					SetItemLocation (display_index, x, y, row + starting_row, col);
					SetItemAtDisplayIndex (display_index, i);
					item_index_matrix [row + starting_row, col] = i;

				} else
#endif
				{
					x = UseCustomColumnWidth ? col * custom_column_width : col * (item_size.Width + x_spacing);
					y = row * (item_size.Height + y_spacing);
					display_index = i; // Same as item index in Items

					SetItemLocation (i, x, y, row, col);
					item_index_matrix [row, col] = i;

					if (left_aligned) {
						row++;
						if (row == rows) {
							row = 0;
							col++;
						}
					} else {
						if (++col == cols) {
							col = 0;
							row++;
						}
					}
				}

				item.Layout ();
				item.DisplayIndex = display_index;
#if NET_2_0					
				item.SetPosition (new Point (x, y));
#endif					
			}

			item_control.Size = new Size (layout_wd, layout_ht);
		}

#if NET_2_0
		void CalculateGroupsLayout (Size item_size, int y_spacing, int y_origin)
		{
			int y = y_origin;
			bool details = view == View.Details;

			for (int i = 0; i < groups.InternalCount; i++) {
				ListViewGroup group = groups.GetInternalGroup (i);
				if (group.ItemCount == 0)
					continue;

				y += LayoutGroupHeader (group, y, item_size.Height, y_spacing, details ? group.ItemCount : group.rows);
			}

			layout_ht = y; // Update height taking into account Groups' headers heights
		}

		int LayoutGroupHeader (ListViewGroup group, int y_origin, int item_height, int y_spacing, int rows)
		{
			Rectangle client_area = ClientRectangle;
			int header_height = Font.Height + 15; // one line height + some padding

			group.HeaderBounds = new Rectangle (0, y_origin, client_area.Width - v_scroll.Width, header_height);
			group.items_area_location = new Point (0, y_origin + header_height);

			int items_area_height = ((item_height + y_spacing) * rows);
			return header_height + items_area_height + 10; // Add a small bottom margin
		}

		void CalculateDetailsGroupItemsCount ()
		{
			int items = 0;

			groups.DefaultGroup.ItemCount = GetDefaultGroupItems ();
			for (int i = 0; i < groups.InternalCount; i++) {
				ListViewGroup group = groups.GetInternalGroup (i);
				int items_in_group = group.GetActualItemCount ();

				if (items_in_group == 0)
					continue;

				group.starting_item = items;
				group.current_item = 0; // Reset layout.
				items += items_in_group;
			}
		}
#endif

		void LayoutHeader ()
		{
			int x = 0;
			for (int i = 0; i < Columns.Count; i++) {
				ColumnHeader col = GetReorderedColumn (i);
				col.X = x;
				col.Y = 0;
				col.CalcColumnHeader ();
				x += col.Wd;
			}

			layout_wd = x;

			if (x < ClientRectangle.Width)
				x = ClientRectangle.Width;

			if (header_style == ColumnHeaderStyle.None) {
				header_control.Visible = false;
				header_control.Size = Size.Empty;
				layout_wd = ClientRectangle.Width;
			} else {
				header_control.Width = x;
				header_control.Height = columns.Count > 0 ? columns [0].Ht : ThemeEngine.Current.ListViewGetHeaderHeight (this, Font);
				header_control.Visible = true;
			}
		}

		void LayoutDetails ()
		{
			LayoutHeader ();

			if (columns.Count == 0) {
				item_control.Visible = false;
				layout_wd = ClientRectangle.Width;
				layout_ht = ClientRectangle.Height;
				return;
			}

			item_control.Visible = true;
			item_control.Location = Point.Empty;
			item_control.Width = ClientRectangle.Width;
			AdjustChildrenZOrder ();

			int item_height = GetDetailsItemHeight ();
			ItemSize = new Size (0, item_height); // We only cache Height for details view
			int y = header_control.Height;
			layout_ht = y + (item_height * items.Count);
			if (items.Count > 0 && grid_lines) // some space for bottom gridline
				layout_ht += 2;

#if NET_2_0
			bool using_groups = UsingGroups;
			if (using_groups) {
				// Observe that this routines will override our layout_ht value
				CalculateDetailsGroupItemsCount ();
				CalculateGroupsLayout (ItemSize, 2, y);
			}

			if (virtual_mode) // no assgination on items is needed
				return;
#endif

			for (int i = 0; i < items.Count; i++) {
				ListViewItem item = items [i];

				int display_index;
				int item_y;

#if NET_2_0
				if (using_groups) {
					ListViewGroup group = item.Group;
					if (group == null)
						group = groups.DefaultGroup;

					int current_item = group.current_item++;
					Point group_items_loc = group.items_area_location;
					display_index = group.starting_item + current_item;

					y = item_y = current_item * (item_height + 2) + group_items_loc.Y;
					SetItemLocation (display_index, 0, item_y, 0, 0);
					SetItemAtDisplayIndex (display_index, i);
				} else
#endif
				{
					display_index = i;
					item_y = y;
					SetItemLocation (i, 0, item_y, 0, 0);
					y += item_height;
				}

				item.Layout ();
				item.DisplayIndex = display_index;
#if NET_2_0					
				item.SetPosition (new Point (0, item_y));
#endif					
			}
		}

		// Need to make sure HeaderControl is on top, and we can't simply use BringToFront since
		// these controls are implicit, so we need to re-populate our collection.
		void AdjustChildrenZOrder ()
		{
			SuspendLayout ();
			Controls.ClearImplicit ();
			Controls.AddImplicit (header_control);
			Controls.AddImplicit (item_control);
			ResumeLayout ();
		}

		private void AdjustItemsPositionArray (int count)
		{
#if  NET_2_0
			// In virtual mode we compute the positions on the fly.
			if (virtual_mode)
				return;
#endif
			if (items_location.Length >= count)
				return;

			// items_location, items_matrix_location and reordered_items_indices must keep the same length
			count = Math.Max (count, items_location.Length * 2);
			items_location = new Point [count];
			items_matrix_location = new ItemMatrixLocation [count];
			reordered_items_indices = new int [count];
		}

		private void CalculateListView (ListViewAlignment align)
		{
			CalcTextSize ();

			AdjustItemsPositionArray (items.Count);

			switch (view) {
			case View.Details:
				LayoutDetails ();
				break;

			case View.SmallIcon:
				LayoutIcons (SmallIconItemSize, alignment == ListViewAlignment.Left, 
						ThemeEngine.Current.ListViewHorizontalSpacing, 2);
				break;

			case View.LargeIcon:
				LayoutIcons (LargeIconItemSize, alignment == ListViewAlignment.Left,
					ThemeEngine.Current.ListViewHorizontalSpacing,
					ThemeEngine.Current.ListViewVerticalSpacing);
				break;

			case View.List:
				LayoutIcons (SmallIconItemSize, true, 
						ThemeEngine.Current.ListViewHorizontalSpacing, 2);
				break;
#if NET_2_0
			case View.Tile:
				if (!Application.VisualStylesEnabled)
					goto case View.LargeIcon;

				LayoutIcons (TileItemSize, alignment == ListViewAlignment.Left, 
						ThemeEngine.Current.ListViewHorizontalSpacing,
						ThemeEngine.Current.ListViewVerticalSpacing);
				break;
#endif
			}

			CalculateScrollBars ();
		}

		internal Point GetItemLocation (int index)
		{
			Point loc = Point.Empty;
#if NET_2_0
			if (virtual_mode)
				loc = GetFixedItemLocation (index);
			else
#endif
				loc = items_location [index];

			loc.X -= h_marker; // Adjust to scroll
			loc.Y -= v_marker;

			return loc;
		}

#if NET_2_0
		Point GetFixedItemLocation (int index)
		{
			Point loc = Point.Empty;

			switch (view) {
				case View.LargeIcon:
				case View.SmallIcon:
					loc.X = index % cols * (item_size.Width + x_spacing);
					loc.Y = index / cols * (item_size.Height + y_spacing);
					break;
				case View.List:
					loc.X = index / rows * (item_size.Width + x_spacing);
					loc.Y = index % rows * (item_size.Height + y_spacing);
					break;
				case View.Details:
					loc.Y = header_control.Height + (index * item_size.Height);
					break;
			}

			return loc;
		}
#endif

		internal int GetItemIndex (int display_index)
		{
#if NET_2_0
			if (virtual_mode)
				return display_index; // no reordering in virtual mode.
#endif
			return reordered_items_indices [display_index];
		}

		internal ListViewItem GetItemAtDisplayIndex (int display_index)
		{
#if NET_2_0
			// in virtual mode there's no reordering at all.
			if (virtual_mode)
				return items [display_index];
#endif
			return items [reordered_items_indices [display_index]];
		}

		internal void SetItemAtDisplayIndex (int display_index, int index)
		{
			reordered_items_indices [display_index] = index;
		}

		private bool KeySearchString (KeyEventArgs ke)
		{
			int current_tickcnt = Environment.TickCount;
			if (keysearch_tickcnt > 0 && current_tickcnt - keysearch_tickcnt > keysearch_keydelay) {
				keysearch_text = string.Empty;
			}
			
			if (!Char.IsLetterOrDigit ((char)ke.KeyCode))
				return false;

			keysearch_text += (char)ke.KeyCode;
			keysearch_tickcnt = current_tickcnt;

			int prev_focused = FocusedItem == null ? 0 : FocusedItem.DisplayIndex;
			int start = prev_focused + 1 < Items.Count ? prev_focused + 1 : 0;

			ListViewItem item = FindItemWithText (keysearch_text, false, start, true, true);
			if (item != null && prev_focused != item.DisplayIndex) {
				selected_indices.Clear ();

				SetFocusedItem (item.DisplayIndex);
				item.Selected = true;
				EnsureVisible (GetItemIndex (item.DisplayIndex));
			}

			return true;
		}

		private void OnItemsChanged ()
		{
			ResetSearchString ();
		}

		private void ResetSearchString ()
		{
			keysearch_text = String.Empty;
		}

		int GetAdjustedIndex (Keys key)
		{
			int result = -1;

			if (View == View.Details) {
				switch (key) {
				case Keys.Up:
					result = FocusedItem.DisplayIndex - 1;
					break;
				case Keys.Down:
					result = FocusedItem.DisplayIndex + 1;
					if (result == items.Count)
						result = -1;
					break;
				case Keys.PageDown:
					int last_index = LastVisibleIndex;
					Rectangle item_rect = new Rectangle (GetItemLocation (last_index), ItemSize);
					if (item_rect.Bottom > item_control.ClientRectangle.Bottom)
						last_index--;
					if (FocusedItem.DisplayIndex == last_index) {
						if (FocusedItem.DisplayIndex < Items.Count - 1) {
							int page_size = item_control.Height / ItemSize.Height - 1;
							result = FocusedItem.DisplayIndex + page_size - 1;
							if (result >= Items.Count)
								result = Items.Count - 1;
						}
					} else
						result = last_index;
					break;
				case Keys.PageUp:
					int first_index = FirstVisibleIndex;
					if (GetItemLocation (first_index).Y < 0)
						first_index++;
					if (FocusedItem.DisplayIndex == first_index) {
						if (first_index > 0) {
							int page_size = item_control.Height / ItemSize.Height - 1;
							result = first_index - page_size + 1;
							if (result < 0)
								result = 0;
						}
					} else
						result = first_index;
					break;
				}
				return result;
			}

#if NET_2_0
			if (virtual_mode)
				return GetFixedAdjustedIndex (key);
#endif

			ItemMatrixLocation item_matrix_location = items_matrix_location [FocusedItem.DisplayIndex];
			int row = item_matrix_location.Row;
			int col = item_matrix_location.Col;

			int adjusted_index = -1;

			switch (key) {
			case Keys.Left:
				if (col == 0)
					return -1;
				adjusted_index = item_index_matrix [row, col - 1];
				break;

			case Keys.Right:
				if (col == (cols - 1))
					return -1;
				while (item_index_matrix [row, col + 1] == 0) {
					row--;
					if (row < 0)
						return -1;
				}
				adjusted_index = item_index_matrix [row, col + 1];
				break;

			case Keys.Up:
				if (row == 0)
					return -1;
				while (item_index_matrix [row - 1, col] == 0 && row != 1) {
					col--;
					if (col < 0)
						return -1;
				}
				adjusted_index = item_index_matrix [row - 1, col];
				break;

			case Keys.Down:
				if (row == (rows - 1) || row == Items.Count - 1)
					return -1;
				while (item_index_matrix [row + 1, col] == 0) {
					col--;
					if (col < 0)
						return -1;
				}
				adjusted_index = item_index_matrix [row + 1, col];
				break;

			default:
				return -1;
			}

			return items [adjusted_index].DisplayIndex;
		}

#if NET_2_0
		// Used for virtual mode, where items *cannot* be re-arranged
		int GetFixedAdjustedIndex (Keys key)
		{
			int result;

			switch (key) {
				case Keys.Left:
					if (view == View.List)
						result = focused_item_index - rows;
					else
						result = focused_item_index - 1;
					break;
				case Keys.Right:
					if (view == View.List)
						result = focused_item_index + rows;
					else
						result = focused_item_index + 1;
					break;
				case Keys.Up:
					if (view != View.List)
						result = focused_item_index - cols;
					else
						result = focused_item_index - 1;
					break;
				case Keys.Down:
					if (view != View.List)
						result = focused_item_index + cols;
					else
						result = focused_item_index + 1;
					break;
				default:
					return -1;

			}

			if (result < 0 || result >= items.Count)
				result = focused_item_index;

			return result;
		}
#endif

		ListViewItem selection_start;

		private bool SelectItems (ArrayList sel_items)
		{
			bool changed = false;
			foreach (ListViewItem item in SelectedItems)
				if (!sel_items.Contains (item)) {
					item.Selected = false;
					changed = true;
				}
			foreach (ListViewItem item in sel_items)
				if (!item.Selected) {
					item.Selected = true;
					changed = true;
				}
			return changed;
		}

		private void UpdateMultiSelection (int index, bool reselect)
		{
			bool shift_pressed = (XplatUI.State.ModifierKeys & Keys.Shift) != 0;
			bool ctrl_pressed = (XplatUI.State.ModifierKeys & Keys.Control) != 0;
			ListViewItem item = GetItemAtDisplayIndex (index);

			if (shift_pressed && selection_start != null) {
				ArrayList list = new ArrayList ();
				int start_index = selection_start.DisplayIndex;
				int start = Math.Min (start_index, index);
				int end = Math.Max (start_index, index);
				if (View == View.Details) {
					for (int i = start; i <= end; i++)
						list.Add (GetItemAtDisplayIndex (i));
				} else {
					ItemMatrixLocation start_item_matrix_location = items_matrix_location [start];
					ItemMatrixLocation end_item_matrix_location = items_matrix_location [end];
					int left = Math.Min (start_item_matrix_location.Col, end_item_matrix_location.Col);
					int right = Math.Max (start_item_matrix_location.Col, end_item_matrix_location.Col);
					int top = Math.Min (start_item_matrix_location.Row, end_item_matrix_location.Row);
					int bottom = Math.Max (start_item_matrix_location.Row, end_item_matrix_location.Row);

					for (int i = 0; i < items.Count; i++) {
						ItemMatrixLocation item_matrix_loc = items_matrix_location [i];

						if (item_matrix_loc.Row >= top && item_matrix_loc.Row <= bottom &&
								item_matrix_loc.Col >= left && item_matrix_loc.Col <= right)
							list.Add (GetItemAtDisplayIndex (i));
					}
				}
				SelectItems (list);
			} else if (ctrl_pressed) {
				item.Selected = !item.Selected;
				selection_start = item;
			} else {
				if (!reselect) {
					// do not unselect, and reselect the item
					foreach (int itemIndex in SelectedIndices) {
						if (index == itemIndex)
							continue;
						items [itemIndex].Selected = false;
					}
				} else {
					SelectedItems.Clear ();
					item.Selected = true;
				}
				selection_start = item;
			}
		}

		internal override bool InternalPreProcessMessage (ref Message msg)
		{
			if (msg.Msg == (int)Msg.WM_KEYDOWN) {
				Keys key_data = (Keys)msg.WParam.ToInt32();
				
				HandleNavKeys (key_data);
			} 
			
			return base.InternalPreProcessMessage (ref msg);
		}

		bool HandleNavKeys (Keys key_data)
		{
			if (Items.Count == 0 || !item_control.Visible)
				return false;

			if (FocusedItem == null)
				SetFocusedItem (0);

			switch (key_data) {
			case Keys.End:
				SelectIndex (Items.Count - 1);
				break;

			case Keys.Home:
				SelectIndex (0);
				break;

			case Keys.Left:
			case Keys.Right:
			case Keys.Up:
			case Keys.Down:
			case Keys.PageUp:
			case Keys.PageDown:
				SelectIndex (GetAdjustedIndex (key_data));
				break;

			case Keys.Space:
				SelectIndex (focused_item_index);
				ToggleItemsCheckState ();
				break;
			case Keys.Enter:
				if (selected_indices.Count > 0)
					OnItemActivate (EventArgs.Empty);
				break;

			default:
				return false;
			}

			return true;
		}

		void ToggleItemsCheckState ()
		{
			if (!CheckBoxes)
				return;

			// Don't modify check state if StateImageList has less than 2 elements
			if (StateImageList != null && StateImageList.Images.Count < 2)
				return;

			if (SelectedIndices.Count > 0) {
				for (int i = 0; i < SelectedIndices.Count; i++) {
					ListViewItem item = Items [SelectedIndices [i]];
					item.Checked = !item.Checked;
				}
				return;
			} 
			
			if (FocusedItem != null) {
				FocusedItem.Checked = !FocusedItem.Checked;
				SelectIndex (FocusedItem.Index);
			}
		}

		void SelectIndex (int display_index)
		{
			if (display_index == -1)
				return;

			if (MultiSelect)
				UpdateMultiSelection (display_index, true);
			else if (!GetItemAtDisplayIndex (display_index).Selected)
				GetItemAtDisplayIndex (display_index).Selected = true;

			SetFocusedItem (display_index);
			EnsureVisible (GetItemIndex (display_index)); // Index in Items collection, not display index
		}

		private void ListView_KeyDown (object sender, KeyEventArgs ke)
		{
			if (ke.Handled || Items.Count == 0 || !item_control.Visible)
				return;

			if (ke.Alt || ke.Control)
				return;
				
			ke.Handled = KeySearchString (ke);
		}

		private MouseEventArgs TranslateMouseEventArgs (MouseEventArgs args)
		{
			Point loc = PointToClient (Control.MousePosition);
			return new MouseEventArgs (args.Button, args.Clicks, loc.X, loc.Y, args.Delta);
		}

		internal class ItemControl : Control {

			ListView owner;
			ListViewItem clicked_item;
			ListViewItem last_clicked_item;
			bool hover_processed = false;
			bool checking = false;
			ListViewItem prev_hovered_item;
#if NET_2_0
			ListViewItem prev_tooltip_item;
#endif
			int clicks;
			Point drag_begin = new Point (-1, -1);
			internal int dragged_item_index = -1;
			
			ListViewLabelEditTextBox edit_text_box;
			internal ListViewItem edit_item;
			LabelEditEventArgs edit_args;

			public ItemControl (ListView owner)
			{
				this.owner = owner;
				this.SetStyle (ControlStyles.DoubleBuffer, true);
				DoubleClick += new EventHandler(ItemsDoubleClick);
				MouseDown += new MouseEventHandler(ItemsMouseDown);
				MouseMove += new MouseEventHandler(ItemsMouseMove);
				MouseHover += new EventHandler(ItemsMouseHover);
				MouseUp += new MouseEventHandler(ItemsMouseUp);
			}

			void ItemsDoubleClick (object sender, EventArgs e)
			{
				if (owner.activation == ItemActivation.Standard)
					owner.OnItemActivate (EventArgs.Empty);
			}

			enum BoxSelect {
				None,
				Normal,
				Shift,
				Control
			}

			BoxSelect box_select_mode = BoxSelect.None;
			IList prev_selection;
			Point box_select_start;

			Rectangle box_select_rect;
			internal Rectangle BoxSelectRectangle {
				get { return box_select_rect; }
				set {
					if (box_select_rect == value)
						return;

					InvalidateBoxSelectRect ();
					box_select_rect = value;
					InvalidateBoxSelectRect ();
				}
			}

			void InvalidateBoxSelectRect ()
			{
				if (BoxSelectRectangle.Size.IsEmpty)
					return;

				Rectangle edge = BoxSelectRectangle;
				edge.X -= 1;
				edge.Y -= 1;
				edge.Width += 2;
				edge.Height = 2;
				Invalidate (edge);
				edge.Y = BoxSelectRectangle.Bottom - 1;
				Invalidate (edge);
				edge.Y = BoxSelectRectangle.Y - 1;
				edge.Width = 2;
				edge.Height = BoxSelectRectangle.Height + 2;
				Invalidate (edge);
				edge.X = BoxSelectRectangle.Right - 1;
				Invalidate (edge);
			}

			private Rectangle CalculateBoxSelectRectangle (Point pt)
			{
				int left = Math.Min (box_select_start.X, pt.X);
				int right = Math.Max (box_select_start.X, pt.X);
				int top = Math.Min (box_select_start.Y, pt.Y);
				int bottom = Math.Max (box_select_start.Y, pt.Y);
				return Rectangle.FromLTRB (left, top, right, bottom);
			}

			bool BoxIntersectsItem (int index)
			{
				Rectangle r = new Rectangle (owner.GetItemLocation (index), owner.ItemSize);
				if (owner.View != View.Details) {
					r.X += r.Width / 4;
					r.Y += r.Height / 4;
					r.Width /= 2;
					r.Height /= 2;
				}
				return BoxSelectRectangle.IntersectsWith (r);
			}

			bool BoxIntersectsText (int index)
			{
				Rectangle r = owner.GetItemAtDisplayIndex (index).TextBounds;
				return BoxSelectRectangle.IntersectsWith (r);
			}

			ArrayList BoxSelectedItems {
				get {
					ArrayList result = new ArrayList ();
					for (int i = 0; i < owner.Items.Count; i++) {
						bool intersects;
#if NET_2_0
						// Can't iterate over specific items properties in virtualmode
						if (owner.View == View.Details && !owner.FullRowSelect && !owner.VirtualMode)
#else
						if (owner.View == View.Details && !owner.FullRowSelect)
#endif
							intersects = BoxIntersectsText (i);
						else
							intersects = BoxIntersectsItem (i);

						if (intersects)
							result.Add (owner.GetItemAtDisplayIndex (i));
					}
					return result;
				}
			}

			private bool PerformBoxSelection (Point pt)
			{
				if (box_select_mode == BoxSelect.None)
					return false;

				BoxSelectRectangle = CalculateBoxSelectRectangle (pt);
				
				ArrayList box_items = BoxSelectedItems;

				ArrayList items;

				switch (box_select_mode) {

				case BoxSelect.Normal:
					items = box_items;
					break;

				case BoxSelect.Control:
					items = new ArrayList ();
					foreach (int index in prev_selection)
						if (!box_items.Contains (owner.Items [index]))
							items.Add (owner.Items [index]);
					foreach (ListViewItem item in box_items)
						if (!prev_selection.Contains (item.Index))
							items.Add (item);
					break;

				case BoxSelect.Shift:
					items = box_items;
					foreach (ListViewItem item in box_items)
						prev_selection.Remove (item.Index);
					foreach (int index in prev_selection)
						items.Add (owner.Items [index]);
					break;

				default:
					throw new Exception ("Unexpected Selection mode: " + box_select_mode);
				}

				SuspendLayout ();
				owner.SelectItems (items);
				ResumeLayout ();

				return true;
			}

			private void ItemsMouseDown (object sender, MouseEventArgs me)
			{
				owner.OnMouseDown (owner.TranslateMouseEventArgs (me));
				if (owner.items.Count == 0)
					return;

				bool box_selecting = false;
				Size item_size = owner.ItemSize;
				Point pt = new Point (me.X, me.Y);
				for (int i = 0; i < owner.items.Count; i++) {
					Rectangle item_rect = new Rectangle (owner.GetItemLocation (i), item_size);
					if (!item_rect.Contains (pt))
						continue;

					// Actual item in 'i' position
					ListViewItem item = owner.GetItemAtDisplayIndex (i);

					if (item.CheckRectReal.Contains (pt)) {
						// Don't modify check state if we have only one image
						// and if we are in 1.1 profile only take into account
						// double clicks
						if (owner.StateImageList != null && owner.StateImageList.Images.Count < 2 
#if !NET_2_0
								&& me.Clicks == 1
#endif
								)
							return;

						// Generate an extra ItemCheck event when we got two clicks
						// (Match weird .Net behaviour)
						if (me.Clicks == 2)
							item.Checked = !item.Checked;

						item.Checked = !item.Checked;
						checking = true;
						return;
					}
					
					if (owner.View == View.Details) {
						bool over_text = item.TextBounds.Contains (pt);
						if (owner.FullRowSelect) {
							clicked_item = item;
							bool over_item_column = (me.X > owner.Columns[0].X && me.X < owner.Columns[0].X + owner.Columns[0].Width);
							if (!over_text && over_item_column && owner.MultiSelect)
								box_selecting = true;
						} else if (over_text)
							clicked_item = item;
						else
							owner.SetFocusedItem (i);
					} else
						clicked_item = item;

					break;
				}


				if (clicked_item != null) {
					bool changed = !clicked_item.Selected;
					if (me.Button == MouseButtons.Left || (XplatUI.State.ModifierKeys == Keys.None && changed))
						owner.SetFocusedItem (clicked_item.DisplayIndex);

					if (owner.MultiSelect) {
						bool reselect = (!owner.LabelEdit || changed);
						if (me.Button == MouseButtons.Left || (XplatUI.State.ModifierKeys == Keys.None && changed))
							owner.UpdateMultiSelection (clicked_item.DisplayIndex, reselect);
					} else {
						clicked_item.Selected = true;
					}

#if NET_2_0
					if (owner.VirtualMode && changed) {
						// Broken event - It's not fired from Item.Selected also
						ListViewVirtualItemsSelectionRangeChangedEventArgs args = 
							new ListViewVirtualItemsSelectionRangeChangedEventArgs (0, owner.items.Count - 1, false);

						owner.OnVirtualItemsSelectionRangeChanged (args);
					}
#endif
					// Report clicks only if the item was clicked. On MS the
					// clicks are only raised if you click an item
					clicks = me.Clicks;
					if (me.Clicks > 1) {
						if (owner.CheckBoxes)
							clicked_item.Checked = !clicked_item.Checked;
					} else if (me.Clicks == 1) {
						if (owner.LabelEdit && !changed)
							BeginEdit (clicked_item); // this is probably not the correct place to execute BeginEdit
					}

					drag_begin = me.Location;
					dragged_item_index = clicked_item.Index;
				} else {
					if (owner.MultiSelect)
						box_selecting = true;
					else if (owner.SelectedItems.Count > 0)
						owner.SelectedItems.Clear ();
				}

				if (box_selecting) {
					Keys mods = XplatUI.State.ModifierKeys;
					if ((mods & Keys.Shift) != 0)
						box_select_mode = BoxSelect.Shift;
					else if ((mods & Keys.Control) != 0)
						box_select_mode = BoxSelect.Control;
					else
						box_select_mode = BoxSelect.Normal;
					box_select_start = pt; 
					prev_selection = owner.SelectedIndices.List.Clone () as IList;
				}
			}

			private void ItemsMouseMove (object sender, MouseEventArgs me)
			{
				bool done = PerformBoxSelection (new Point (me.X, me.Y));

				owner.OnMouseMove (owner.TranslateMouseEventArgs (me));

				if (done)
					return;
				if ((me.Button != MouseButtons.Left && me.Button != MouseButtons.Right) &&
					!hover_processed && owner.Activation != ItemActivation.OneClick
#if NET_2_0
					&& !owner.ShowItemToolTips
#endif
						)
					return;

				Point pt = PointToClient (Control.MousePosition);
				ListViewItem item = owner.GetItemAt (pt.X, pt.Y);

				if (hover_processed && item != null && item != prev_hovered_item) {
					hover_processed = false;
					XplatUI.ResetMouseHover (Handle);
				}

				// Need to invalidate the item in HotTracking to show/hide the underline style
				if (owner.Activation == ItemActivation.OneClick) {
					if (item == null && owner.HotItemIndex != -1) {
#if NET_2_0
						if (owner.HotTracking)
							Invalidate (owner.Items [owner.HotItemIndex].Bounds); // Previous one
#endif

						Cursor = Cursors.Default;
						owner.HotItemIndex = -1;
					} else if (item != null && owner.HotItemIndex == -1) {
#if NET_2_0
						if (owner.HotTracking)
							Invalidate (item.Bounds);
#endif

						Cursor = Cursors.Hand;
						owner.HotItemIndex = item.Index;
					}
				}

				if (me.Button == MouseButtons.Left || me.Button == MouseButtons.Right) {
					if (drag_begin != new Point (-1, -1)) {
						Rectangle r = new Rectangle (drag_begin, SystemInformation.DragSize);
						if (!r.Contains (me.X, me.Y)) {
							ListViewItem dragged_item  = owner.items [dragged_item_index];
							owner.OnItemDrag (new ItemDragEventArgs (me.Button, dragged_item));

							drag_begin = new Point (-1, -1);
							dragged_item_index = -1;
						}
					}
				}

#if NET_2_0
				if (owner.ShowItemToolTips) {
					if (item == null) {
						owner.item_tooltip.Active = false;
						prev_tooltip_item = null;
					} else if (item != prev_tooltip_item && item.ToolTipText.Length > 0) {
						owner.item_tooltip.Active = true;
						owner.item_tooltip.SetToolTip (owner, item.ToolTipText);
						prev_tooltip_item = item;
					}
				}
#endif

			}

			private void ItemsMouseHover (object sender, EventArgs e)
			{
				if (owner.hover_pending) {
					owner.OnMouseHover (e);
					owner.hover_pending = false;
				}

				if (Capture)
					return;

				hover_processed = true;
				Point pt = PointToClient (Control.MousePosition);
				ListViewItem item = owner.GetItemAt (pt.X, pt.Y);
				if (item == null)
					return;

				prev_hovered_item = item;

				if (owner.HoverSelection) {
					if (owner.MultiSelect)
						owner.UpdateMultiSelection (item.Index, true);
					else
						item.Selected = true;
					
					owner.SetFocusedItem (item.DisplayIndex);
					Select (); // Make sure we have the focus, since MouseHover doesn't give it to us
				}

#if NET_2_0
				owner.OnItemMouseHover (new ListViewItemMouseHoverEventArgs (item));
#endif
			}

			void HandleClicks (MouseEventArgs me)
			{
				// if the click is not on an item,
				// clicks remains as 0
				if (clicks > 1) {
#if !NET_2_0
					owner.OnDoubleClick (EventArgs.Empty);
				} else if (clicks == 1) {
					owner.OnClick (EventArgs.Empty);
#else
					owner.OnDoubleClick (EventArgs.Empty);
					owner.OnMouseDoubleClick (me);
				} else if (clicks == 1) {
					owner.OnClick (EventArgs.Empty);
					owner.OnMouseClick (me);
#endif
				}

				clicks = 0;
			}

			private void ItemsMouseUp (object sender, MouseEventArgs me)
			{
				MouseEventArgs owner_me = owner.TranslateMouseEventArgs (me);
				HandleClicks (owner_me);

				Capture = false;
				if (owner.Items.Count == 0) {
					ResetMouseState ();
					owner.OnMouseUp (owner_me);
					return;
				}

				Point pt = new Point (me.X, me.Y);

				Rectangle rect = Rectangle.Empty;
				if (clicked_item != null) {
					if (owner.view == View.Details && !owner.full_row_select)
						rect = clicked_item.GetBounds (ItemBoundsPortion.Label);
					else
						rect = clicked_item.Bounds;

					if (rect.Contains (pt)) {
						switch (owner.activation) {
						case ItemActivation.OneClick:
							owner.OnItemActivate (EventArgs.Empty);
							break;

						case ItemActivation.TwoClick:
							if (last_clicked_item == clicked_item) {
								owner.OnItemActivate (EventArgs.Empty);
								last_clicked_item = null;
							} else
								last_clicked_item = clicked_item;
							break;
						default:
							// DoubleClick activation is handled in another handler
							break;
						}
					}
				} else if (!checking && owner.SelectedItems.Count > 0 && BoxSelectRectangle.Size.IsEmpty) {
					// Need this to clean up background clicks
					owner.SelectedItems.Clear ();
				}

				ResetMouseState ();
				owner.OnMouseUp (owner_me);
			}

			private void ResetMouseState ()
			{				
				clicked_item = null;
				box_select_start = Point.Empty;
				BoxSelectRectangle = Rectangle.Empty;
				prev_selection = null;
				box_select_mode = BoxSelect.None;
				checking = false;

				// Clean these bits in case the mouse buttons were
				// released before firing ItemDrag
				dragged_item_index = -1;
				drag_begin = new Point (-1, -1);
			}
			
			private void LabelEditFinished (object sender, EventArgs e)
			{
				EndEdit (edit_item);
			}

			private void LabelEditCancelled (object sender, EventArgs e)
			{
				edit_args.SetLabel (null);
				EndEdit (edit_item);
			}

			private void LabelTextChanged (object sender, EventArgs e)
			{
				if (edit_args != null)
					edit_args.SetLabel (edit_text_box.Text);
			}

			internal void BeginEdit (ListViewItem item)
			{
				if (edit_item != null)
					EndEdit (edit_item);
				
				if (edit_text_box == null) {
					edit_text_box = new ListViewLabelEditTextBox ();
					edit_text_box.BorderStyle = BorderStyle.FixedSingle;
					edit_text_box.EditingCancelled += new EventHandler (LabelEditCancelled);
					edit_text_box.EditingFinished += new EventHandler (LabelEditFinished);
					edit_text_box.TextChanged += new EventHandler (LabelTextChanged);
					edit_text_box.Visible = false;
					Controls.Add (edit_text_box);
				}
				
				item.EnsureVisible();
				
				edit_text_box.Reset ();
				
				switch (owner.view) {
					case View.List:
					case View.SmallIcon:
					case View.Details:
						edit_text_box.TextAlign = HorizontalAlignment.Left;
						edit_text_box.Bounds = item.GetBounds (ItemBoundsPortion.Label);
						SizeF sizef = TextRenderer.MeasureString (item.Text, item.Font);
						edit_text_box.Width = (int)sizef.Width + 4;
						edit_text_box.MaxWidth = owner.ClientRectangle.Width - edit_text_box.Bounds.X;
						edit_text_box.WordWrap = false;
						edit_text_box.Multiline = false;
						break;
					case View.LargeIcon:
						edit_text_box.TextAlign = HorizontalAlignment.Center;
						edit_text_box.Bounds = item.GetBounds (ItemBoundsPortion.Label);
						sizef = TextRenderer.MeasureString (item.Text, item.Font);
						edit_text_box.Width = (int)sizef.Width + 4;
						edit_text_box.MaxWidth = item.GetBounds(ItemBoundsPortion.Entire).Width;
						edit_text_box.MaxHeight = owner.ClientRectangle.Height - edit_text_box.Bounds.Y;
						edit_text_box.WordWrap = true;
						edit_text_box.Multiline = true;
						break;
				}

				edit_item = item;

				edit_text_box.Text = item.Text;
				edit_text_box.Font = item.Font;
				edit_text_box.Visible = true;
				edit_text_box.Focus ();
				edit_text_box.SelectAll ();

				edit_args = new LabelEditEventArgs (owner.Items.IndexOf (edit_item));
				owner.OnBeforeLabelEdit (edit_args);

				if (edit_args.CancelEdit)
					EndEdit (item);
			}

			internal void CancelEdit (ListViewItem item)
			{
				// do nothing if there's no item being edited, or if the
				// item being edited is not the one passed in
				if (edit_item == null || edit_item != item)
					return;

				edit_args.SetLabel (null);
				EndEdit (item);
			}

			internal void EndEdit (ListViewItem item)
			{
				// do nothing if there's no item being edited, or if the
				// item being edited is not the one passed in
				if (edit_item == null || edit_item != item)
					return;

				if (edit_text_box != null) {
					if (edit_text_box.Visible)
						edit_text_box.Visible = false;
					// ensure listview gets focus
					owner.Focus ();
				}

				// Same as TreeView.EndEdit: need to have focus in synch
				Application.DoEvents ();

				// 
				// Create a new instance, since we could get a call to BeginEdit
				// from the handler and have fields out of synch
				//
				LabelEditEventArgs args = new LabelEditEventArgs (item.Index, edit_args.Label);
				edit_item = null;

				owner.OnAfterLabelEdit (args);
				if (!args.CancelEdit && args.Label != null)
					item.Text = args.Label;
			}

			internal override void OnPaintInternal (PaintEventArgs pe)
			{
				ThemeEngine.Current.DrawListViewItems (pe.Graphics, pe.ClipRectangle, owner);
			}

			protected override void WndProc (ref Message m)
			{
				switch ((Msg)m.Msg) {
				case Msg.WM_KILLFOCUS:
					owner.Select (false, true);
					break;
				case Msg.WM_SETFOCUS:
					owner.Select (false, true);
					break;
				case Msg.WM_LBUTTONDOWN:
					if (!Focused)
						owner.Select (false, true);
					break;
				case Msg.WM_RBUTTONDOWN:
					if (!Focused)
						owner.Select (false, true);
					break;
				default:
					break;
				}
				base.WndProc (ref m);
			}
		}
		
		internal class ListViewLabelEditTextBox : TextBox
		{
			int max_width = -1;
			int min_width = -1;
			
			int max_height = -1;
			int min_height = -1;
			
			int old_number_lines = 1;
			
			SizeF text_size_one_char;
			
			public ListViewLabelEditTextBox ()
			{
				min_height = DefaultSize.Height;
				text_size_one_char = TextRenderer.MeasureString ("B", Font);
			}
			
			public int MaxWidth {
				set {
					if (value < min_width)
						max_width = min_width;
					else
						max_width = value;
				}
			}
			
			public int MaxHeight {
				set {
					if (value < min_height)
						max_height = min_height;
					else
						max_height = value;
				}
			}
			
			public new int Width {
				get {
					return base.Width;
				}
				set {
					min_width = value;
					base.Width = value;
				}
			}
			
			public override Font Font {
				get {
					return base.Font;
				}
				set {
					base.Font = value;
					text_size_one_char = TextRenderer.MeasureString ("B", Font);
				}
			}
			
			protected override void OnTextChanged (EventArgs e)
			{
				SizeF text_size = TextRenderer.MeasureString (Text, Font);
				
				int new_width = (int)text_size.Width + 8;
				
				if (!Multiline)
					ResizeTextBoxWidth (new_width);
				else {
					if (Width != max_width)
						ResizeTextBoxWidth (new_width);
					
					int number_lines = Lines.Length;
					
					if (number_lines != old_number_lines) {
						int new_height = number_lines * (int)text_size_one_char.Height + 4;
						old_number_lines = number_lines;
						
						ResizeTextBoxHeight (new_height);
					}
				}
				
				base.OnTextChanged (e);
			}
			
			protected override bool IsInputKey (Keys key_data)
			{
				if ((key_data & Keys.Alt) == 0) {
					switch (key_data & Keys.KeyCode) {
						case Keys.Enter:
							return true;
						case Keys.Escape:
							return true;
					}
				}
				return base.IsInputKey (key_data);
			}
			
			protected override void OnKeyDown (KeyEventArgs e)
			{
				if (!Visible)
					return;

				switch (e.KeyCode) {
				case Keys.Return:
					Visible = false;
					e.Handled = true;
					OnEditingFinished (e);
					break;
				case Keys.Escape:
					Visible = false;
					e.Handled = true;
					OnEditingCancelled (e);
					break;
				}
			}
			
			protected override void OnLostFocus (EventArgs e)
			{
				if (Visible) {
					OnEditingFinished (e);
				}
			}

			protected void OnEditingCancelled (EventArgs e)
			{
				EventHandler eh = (EventHandler)(Events [EditingCancelledEvent]);
				if (eh != null)
					eh (this, e);
			}
			
			protected void OnEditingFinished (EventArgs e)
			{
				EventHandler eh = (EventHandler)(Events [EditingFinishedEvent]);
				if (eh != null)
					eh (this, e);
			}
			
			private void ResizeTextBoxWidth (int new_width)
			{
				if (new_width > max_width)
					base.Width = max_width;
				else 
				if (new_width >= min_width)
					base.Width = new_width;
				else
					base.Width = min_width;
			}
			
			private void ResizeTextBoxHeight (int new_height)
			{
				if (new_height > max_height)
					base.Height = max_height;
				else 
				if (new_height >= min_height)
					base.Height = new_height;
				else
					base.Height = min_height;
			}
			
			public void Reset ()
			{
				max_width = -1;
				min_width = -1;
				
				max_height = -1;
				
				old_number_lines = 1;
				
				Text = String.Empty;
				
				Size = DefaultSize;
			}

			static object EditingCancelledEvent = new object ();
			public event EventHandler EditingCancelled {
				add { Events.AddHandler (EditingCancelledEvent, value); }
				remove { Events.RemoveHandler (EditingCancelledEvent, value); }
			}

			static object EditingFinishedEvent = new object ();
			public event EventHandler EditingFinished {
				add { Events.AddHandler (EditingFinishedEvent, value); }
				remove { Events.RemoveHandler (EditingFinishedEvent, value); }
			}
		}

		internal override void OnPaintInternal (PaintEventArgs pe)
		{
			if (updating)
				return;
				
			CalculateScrollBars ();
		}

		void FocusChanged (object o, EventArgs args)
		{
			if (Items.Count == 0)
				return;

			if (FocusedItem == null)
				SetFocusedItem (0);

			ListViewItem focused_item = FocusedItem;

			if (focused_item.ListView != null) {
				focused_item.Invalidate ();
				focused_item.Layout ();
				focused_item.Invalidate ();
			}
		}

		private void ListView_Invalidated (object sender, InvalidateEventArgs e)
		{
			// When the ListView is invalidated, we need to invalidate
			// the child controls.
			header_control.Invalidate ();
			item_control.Invalidate ();
		}

		private void ListView_MouseEnter (object sender, EventArgs args)
		{
			hover_pending = true; // Need a hover event for every Enter/Leave cycle
		}

		private void ListView_MouseWheel (object sender, MouseEventArgs me)
		{
			if (Items.Count == 0)
				return;

			int lines = me.Delta / 120;

			if (lines == 0)
				return;

			switch (View) {
			case View.Details:
			case View.SmallIcon:
				Scroll (v_scroll, -ItemSize.Height * SystemInformation.MouseWheelScrollLines * lines);
				break;
			case View.LargeIcon:
				Scroll (v_scroll, -(ItemSize.Height + ThemeEngine.Current.ListViewVerticalSpacing)  * lines);
				break;
			case View.List:
				Scroll (h_scroll, -ItemSize.Width * lines);
				break;
#if NET_2_0
			case View.Tile:
				if (!Application.VisualStylesEnabled)
					goto case View.LargeIcon;

				Scroll (v_scroll, -(ItemSize.Height + ThemeEngine.Current.ListViewVerticalSpacing) * 2 * lines);
				break;
#endif
			}
		}

		private void ListView_SizeChanged (object sender, EventArgs e)
		{
			Redraw (true);
		}
		
		private void SetFocusedItem (int display_index)
		{
			if (display_index != -1)
				GetItemAtDisplayIndex (display_index).Focused = true;
			else if (focused_item_index != -1 && focused_item_index < items.Count) // Previous focused item
				GetItemAtDisplayIndex (focused_item_index).Focused = false;
			focused_item_index = display_index;
#if NET_2_0
			if (display_index == -1)
				OnUIAFocusedItemChanged ();
				// otherwise the event will have been fired
				// when the ListViewItem's Focused was set
#endif
		}

		private void HorizontalScroller (object sender, EventArgs e)
		{
			item_control.EndEdit (item_control.edit_item);
			
			// Avoid unnecessary flickering, when button is
			// kept pressed at the end
			if (h_marker != h_scroll.Value) {
				
				int pixels = h_marker - h_scroll.Value;
				
				h_marker = h_scroll.Value;
				if (header_control.Visible)
					XplatUI.ScrollWindow (header_control.Handle, pixels, 0, false);

				XplatUI.ScrollWindow (item_control.Handle, pixels, 0, false);
			}
		}

		private void VerticalScroller (object sender, EventArgs e)
		{
			item_control.EndEdit (item_control.edit_item);
			
			// Avoid unnecessary flickering, when button is
			// kept pressed at the end
			if (v_marker != v_scroll.Value) {
				int pixels = v_marker - v_scroll.Value;
				Rectangle area = item_control.ClientRectangle;
				if (header_control.Visible) {
					area.Y += header_control.Height;
					area.Height -= header_control.Height;
				}

				v_marker = v_scroll.Value;
				XplatUI.ScrollWindow (item_control.Handle, area, 0, pixels, false);
			}
		}

		internal override bool IsInputCharInternal (char charCode)
		{
			return true;
		}
		#endregion	// Internal Methods Properties

		#region Protected Methods
		protected override void CreateHandle ()
		{
			base.CreateHandle ();
			is_selection_available = true;
			for (int i = 0; i < SelectedItems.Count; i++)
				OnSelectedIndexChanged (EventArgs.Empty);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				large_image_list = null;
				small_image_list = null;
				state_image_list = null;

				foreach (ColumnHeader col in columns)
					col.SetListView (null);

#if NET_2_0
				if (!virtual_mode) // In virtual mode we don't save the items
#endif
					foreach (ListViewItem item in items)
						item.Owner = null;
			}
			
			base.Dispose (disposing);
		}

		protected override bool IsInputKey (Keys keyData)
		{
			switch (keyData) {
			case Keys.Up:
			case Keys.Down:
			case Keys.PageUp:
			case Keys.PageDown:
			case Keys.Right:
			case Keys.Left:
			case Keys.End:
			case Keys.Home:
				return true;

			default:
				break;
			}
			
			return base.IsInputKey (keyData);
		}

		protected virtual void OnAfterLabelEdit (LabelEditEventArgs e)
		{
			LabelEditEventHandler eh = (LabelEditEventHandler)(Events [AfterLabelEditEvent]);
			if (eh != null)
				eh (this, e);
		}

#if NET_2_0
		protected override void OnBackgroundImageChanged (EventArgs e)
		{
			item_control.BackgroundImage = BackgroundImage;
			base.OnBackgroundImageChanged (e);
		}
#endif

		protected virtual void OnBeforeLabelEdit (LabelEditEventArgs e)
		{
			LabelEditEventHandler eh = (LabelEditEventHandler)(Events [BeforeLabelEditEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnColumnClick (ColumnClickEventArgs e)
		{
			ColumnClickEventHandler eh = (ColumnClickEventHandler)(Events [ColumnClickEvent]);
			if (eh != null)
				eh (this, e);
		}

#if NET_2_0
		protected internal virtual void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
		{
			DrawListViewColumnHeaderEventHandler eh = (DrawListViewColumnHeaderEventHandler)(Events[DrawColumnHeaderEvent]);
			if (eh != null)
				eh(this, e);
		}

		protected internal virtual void OnDrawItem(DrawListViewItemEventArgs e)
		{
			DrawListViewItemEventHandler eh = (DrawListViewItemEventHandler)(Events[DrawItemEvent]);
			if (eh != null)
				eh(this, e);
		}

		protected internal virtual void OnDrawSubItem(DrawListViewSubItemEventArgs e)
		{
			DrawListViewSubItemEventHandler eh = (DrawListViewSubItemEventHandler)(Events[DrawSubItemEvent]);
			if (eh != null)
				eh(this, e);
		}

#else
		protected override void OnEnabledChanged (EventArgs e)
		{
			base.OnEnabledChanged (e);
		}
#endif

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
			Redraw (true);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
			CalculateListView (alignment);
#if NET_2_0
			if (!virtual_mode) // Sorting is not allowed in virtual mode
#endif
				Sort ();
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected virtual void OnItemActivate (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ItemActivateEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnItemCheck (ItemCheckEventArgs ice)
		{
			ItemCheckEventHandler eh = (ItemCheckEventHandler)(Events [ItemCheckEvent]);
			if (eh != null)
				eh (this, ice);
		}

#if NET_2_0
		protected internal virtual void OnItemChecked (ItemCheckedEventArgs e)
		{
			ItemCheckedEventHandler eh = (ItemCheckedEventHandler)(Events [ItemCheckedEvent]);
			if (eh != null)
				eh (this, e);
		}
#endif

		protected virtual void OnItemDrag (ItemDragEventArgs e)
		{
			ItemDragEventHandler eh = (ItemDragEventHandler)(Events [ItemDragEvent]);
			if (eh != null)
				eh (this, e);
		}

#if NET_2_0
		protected virtual void OnItemMouseHover (ListViewItemMouseHoverEventArgs e)
		{
			ListViewItemMouseHoverEventHandler eh = (ListViewItemMouseHoverEventHandler)(Events [ItemMouseHoverEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnItemSelectionChanged (ListViewItemSelectionChangedEventArgs e)
		{
			ListViewItemSelectionChangedEventHandler eh = 
				(ListViewItemSelectionChangedEventHandler) Events [ItemSelectionChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected override void OnMouseHover (EventArgs e)
		{
			base.OnMouseHover (e);
		}

		protected override void OnParentChanged (EventArgs e)
		{
			base.OnParentChanged (e);
		}
#endif

		protected virtual void OnSelectedIndexChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [SelectedIndexChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnSystemColorsChanged (EventArgs e)
		{
			base.OnSystemColorsChanged (e);
		}

#if NET_2_0
		protected internal virtual void OnCacheVirtualItems (CacheVirtualItemsEventArgs e)
		{
			CacheVirtualItemsEventHandler eh = (CacheVirtualItemsEventHandler)Events [CacheVirtualItemsEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnRetrieveVirtualItem (RetrieveVirtualItemEventArgs e)
		{
			RetrieveVirtualItemEventHandler eh = (RetrieveVirtualItemEventHandler)Events [RetrieveVirtualItemEvent];
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnRightToLeftLayoutChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)Events[RightToLeftLayoutChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnSearchForVirtualItem (SearchForVirtualItemEventArgs e)
		{
			SearchForVirtualItemEventHandler eh = (SearchForVirtualItemEventHandler) Events [SearchForVirtualItemEvent];
			if (eh != null)
				eh (this, e);
		}
		
		protected virtual void OnVirtualItemsSelectionRangeChanged (ListViewVirtualItemsSelectionRangeChangedEventArgs e)
		{
			ListViewVirtualItemsSelectionRangeChangedEventHandler eh = 
				(ListViewVirtualItemsSelectionRangeChangedEventHandler) Events [VirtualItemsSelectionRangeChangedEvent];
			if (eh != null)
				eh (this, e);
		}
#endif

		protected void RealizeProperties ()
		{
			// FIXME: TODO
		}

		protected void UpdateExtendedStyles ()
		{
			// FIXME: TODO
		}

		bool refocusing = false;

		protected override void WndProc (ref Message m)
		{
			switch ((Msg)m.Msg) {
			case Msg.WM_KILLFOCUS:
				Control receiver = Control.FromHandle (m.WParam);
				if (receiver == item_control) {
					has_focus = false;
					refocusing = true;
					return;
				}
				break;
			case Msg.WM_SETFOCUS:
				if (refocusing) {
					has_focus = true;
					refocusing = false;
					return;
				}
				break;
			default:
				break;
			}
			base.WndProc (ref m);
		}
		#endregion // Protected Methods

		#region Public Instance Methods
		public void ArrangeIcons ()
		{
			ArrangeIcons (this.alignment);
		}

		public void ArrangeIcons (ListViewAlignment value)
		{
			// Icons are arranged only if view is set to LargeIcon or SmallIcon
			if (view == View.LargeIcon || view == View.SmallIcon)
				Redraw (true);
		}

#if NET_2_0
		public void AutoResizeColumn (int columnIndex, ColumnHeaderAutoResizeStyle headerAutoResize)
		{
			if (columnIndex < 0 || columnIndex >= columns.Count)
				throw new ArgumentOutOfRangeException ("columnIndex");

			columns [columnIndex].AutoResize (headerAutoResize);
		}

		public void AutoResizeColumns (ColumnHeaderAutoResizeStyle headerAutoResize)
		{
			BeginUpdate ();
			foreach (ColumnHeader col in columns) 
				col.AutoResize (headerAutoResize);
			EndUpdate ();
		}
#endif

		public void BeginUpdate ()
		{
			// flag to avoid painting
			updating = true;
		}

		public void Clear ()
		{
			columns.Clear ();
			items.Clear ();	// Redraw (true) called here
		}

		public void EndUpdate ()
		{
			// flag to avoid painting
			updating = false;

			// probably, now we need a redraw with recalculations
			this.Redraw (true);
		}

		public void EnsureVisible (int index)
		{
			if (index < 0 || index >= items.Count || scrollable == false || updating)
				return;

			Rectangle view_rect = item_control.ClientRectangle;
#if NET_2_0
			// Avoid direct access to items in virtual mode, and use item bounds otherwise, since we could have reordered items
			Rectangle bounds = virtual_mode ? new Rectangle (GetItemLocation (index), ItemSize) : items [index].Bounds;
#else
			Rectangle bounds = items [index].Bounds;
#endif

			if (view == View.Details && header_style != ColumnHeaderStyle.None) {
				view_rect.Y += header_control.Height;
				view_rect.Height -= header_control.Height;
			}

			if (view_rect.Contains (bounds))
				return;

			if (View != View.Details) {
				if (bounds.Left < 0)
					h_scroll.Value += bounds.Left;
				else if (bounds.Right > view_rect.Right)
					h_scroll.Value += (bounds.Right - view_rect.Right);
			}

			if (bounds.Top < view_rect.Y)
				v_scroll.Value += bounds.Top - view_rect.Y;
			else if (bounds.Bottom > view_rect.Bottom)
				v_scroll.Value += (bounds.Bottom - view_rect.Bottom);
		}

#if NET_2_0
		public ListViewItem FindItemWithText (string text)
		{
			if (items.Count == 0)
				return null;

			return FindItemWithText (text, true, 0, true);
		}

		public ListViewItem FindItemWithText (string text, bool includeSubItemsInSearch, int startIndex)
		{
			return FindItemWithText (text, includeSubItemsInSearch, startIndex, true, false);
		}

		public ListViewItem FindItemWithText (string text, bool includeSubItemsInSearch, int startIndex, bool isPrefixSearch)
		{
			return FindItemWithText (text, includeSubItemsInSearch, startIndex, isPrefixSearch, false);
		}
#endif
		
		internal ListViewItem FindItemWithText (string text, bool includeSubItemsInSearch, int startIndex, bool isPrefixSearch, bool roundtrip)
		{
			if (startIndex < 0 || startIndex >= items.Count)
				throw new ArgumentOutOfRangeException ("startIndex");

			if (text == null)
				throw new ArgumentNullException ("text");

#if NET_2_0
			if (virtual_mode) {
				SearchForVirtualItemEventArgs args = new SearchForVirtualItemEventArgs (true,
						isPrefixSearch, includeSubItemsInSearch, text, Point.Empty, 
						SearchDirectionHint.Down, startIndex);

				OnSearchForVirtualItem (args);
				int idx = args.Index;
				if (idx >= 0 && idx < virtual_list_size)
					return items [idx];

				return null;
			}
#endif

			int i = startIndex;
			while (true) {
				ListViewItem lvi = items [i];

				if (isPrefixSearch) { // prefix search
					if (CultureInfo.CurrentCulture.CompareInfo.IsPrefix (lvi.Text, text, CompareOptions.IgnoreCase))
					       	return lvi;
				} else if (String.Compare (lvi.Text, text, true) == 0) // match
					return lvi;

				if (i + 1 >= items.Count) {
					if (!roundtrip)
						break;

					i = 0;
				} else 
					i++;

				if (i == startIndex)
					break;
			}

			// Subitems have a minor priority, so we have to do a second linear search
			// Also, we don't need to to a roundtrip search for them by now
			if (includeSubItemsInSearch) {
				for (i = startIndex; i < items.Count; i++) {
					ListViewItem lvi = items [i];
					foreach (ListViewItem.ListViewSubItem sub_item in lvi.SubItems)
						if (isPrefixSearch) {
							if (CultureInfo.CurrentCulture.CompareInfo.IsPrefix (sub_item.Text, 
								text, CompareOptions.IgnoreCase))
								return lvi;
						} else if (String.Compare (sub_item.Text, text, true) == 0)
							return lvi;
				}
			}

			return null;
		}

#if NET_2_0
		public ListViewItem FindNearestItem (SearchDirectionHint searchDirection, int x, int y)
		{
			return FindNearestItem (searchDirection, new Point (x, y));
		}

		public ListViewItem FindNearestItem (SearchDirectionHint dir, Point point)
		{
			if (dir < SearchDirectionHint.Left || dir > SearchDirectionHint.Down)
				throw new ArgumentOutOfRangeException ("searchDirection");

			if (view != View.LargeIcon && view != View.SmallIcon)
				throw new InvalidOperationException ();

			if (virtual_mode) {
				SearchForVirtualItemEventArgs args = new SearchForVirtualItemEventArgs (false,
						false, false, String.Empty, point, 
						dir, 0);

				OnSearchForVirtualItem (args);
				int idx = args.Index;
				if (idx >= 0 && idx < virtual_list_size)
					return items [idx];

				return null;
			}

			ListViewItem item = null;
			int min_dist = Int32.MaxValue;

			//
			// It looks like .Net does a previous adjustment
			//
			switch (dir) {
				case SearchDirectionHint.Up:
					point.Y -= item_size.Height;
					break;
				case SearchDirectionHint.Down:
					point.Y += item_size.Height;
					break;
				case SearchDirectionHint.Left:
					point.X -= item_size.Width;
					break;
				case SearchDirectionHint.Right:
					point.X += item_size.Width;
					break;
			}

			for (int i = 0; i < items.Count; i++) {
				Point item_loc = GetItemLocation (i);

				if (dir == SearchDirectionHint.Up) {
					if (point.Y < item_loc.Y)
						continue;
				} else if (dir == SearchDirectionHint.Down) {
					if (point.Y > item_loc.Y)
						continue;
				} else if (dir == SearchDirectionHint.Left) {
					if (point.X < item_loc.X)
						continue;
				} else if (dir == SearchDirectionHint.Right) {
					if (point.X > item_loc.X)
						continue;
				}

				int x_dist = point.X - item_loc.X;
				int y_dist = point.Y - item_loc.Y;

				int dist = x_dist * x_dist  + y_dist * y_dist;
				if (dist < min_dist) {
					item = items [i];
					min_dist = dist;
				}
			}

			return item;
		}
#endif
		
		public ListViewItem GetItemAt (int x, int y)
		{
			Size item_size = ItemSize;
			for (int i = 0; i < items.Count; i++) {
				Point item_location = GetItemLocation (i);
				Rectangle item_rect = new Rectangle (item_location, item_size);
				if (item_rect.Contains (x, y))
					return items [i];
			}

			return null;
		}

		public Rectangle GetItemRect (int index)
		{
			return GetItemRect (index, ItemBoundsPortion.Entire);
		}

		public Rectangle GetItemRect (int index, ItemBoundsPortion portion)
		{
			if (index < 0 || index >= items.Count)
				throw new IndexOutOfRangeException ("index");

			return items [index].GetBounds (portion);
		}

#if NET_2_0
		public ListViewHitTestInfo HitTest (Point point)
		{
			return HitTest (point.X, point.Y);
		}

		public ListViewHitTestInfo HitTest (int x, int y)
		{
			if (x < 0)
				throw new ArgumentOutOfRangeException ("x");
			if (y < 0)
				throw new ArgumentOutOfRangeException ("y");

			ListViewItem item = GetItemAt (x, y);
			if (item == null)
				return new ListViewHitTestInfo (null, null, ListViewHitTestLocations.None);

			ListViewHitTestLocations locations = 0;
			if (item.GetBounds (ItemBoundsPortion.Label).Contains (x, y))
				locations |= ListViewHitTestLocations.Label;
			else if (item.GetBounds (ItemBoundsPortion.Icon).Contains (x, y))
				locations |= ListViewHitTestLocations.Image;
			else if (item.CheckRectReal.Contains (x, y))
				locations |= ListViewHitTestLocations.StateImage;

			ListViewItem.ListViewSubItem subitem = null;
			if (view == View.Details)
				foreach (ListViewItem.ListViewSubItem si in item.SubItems)
					if (si.Bounds.Contains (x, y)) {
						subitem = si;
						break;
					}

			return new ListViewHitTestInfo (item, subitem, locations);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public void RedrawItems (int startIndex, int endIndex, bool invalidateOnly)
		{
			if (startIndex < 0 || startIndex >= items.Count)
				throw new ArgumentOutOfRangeException ("startIndex");
			if (endIndex < 0 || endIndex >= items.Count)
				throw new ArgumentOutOfRangeException ("endIndex");
			if (startIndex > endIndex)
				throw new ArgumentException ("startIndex");

			if (updating)
				return;

			for (int i = startIndex; i <= endIndex; i++)
				items [i].Invalidate ();

			if (!invalidateOnly)
				Update ();
		}
#endif

		public void Sort ()
		{
#if NET_2_0
			if (virtual_mode)
				throw new InvalidOperationException ();
#endif

			Sort (true);
		}

		// we need this overload to reuse the logic for sorting, while allowing
		// redrawing to be done by caller or have it done by this method when
		// sorting is really performed
		//
		// ListViewItemCollection's Add and AddRange methods call this overload
		// with redraw set to false, as they take care of redrawing themselves
		// (they even want to redraw the listview if no sort is performed, as 
		// an item was added), while ListView.Sort () only wants to redraw if 
		// sorting was actually performed
		private void Sort (bool redraw)
		{
			if (!IsHandleCreated || item_sorter == null) {
				return;
			}
			
			items.Sort (item_sorter);
			if (redraw)
				this.Redraw (true);
		}

		public override string ToString ()
		{
			int count = this.Items.Count;

			if (count == 0)
				return string.Format ("System.Windows.Forms.ListView, Items.Count: 0");
			else
				return string.Format ("System.Windows.Forms.ListView, Items.Count: {0}, Items[0]: {1}", count, this.Items [0].ToString ());
		}
		#endregion	// Public Instance Methods


		#region Subclasses

		internal class HeaderControl : Control {

			ListView owner;
			bool column_resize_active = false;
			ColumnHeader resize_column;
			ColumnHeader clicked_column;
			ColumnHeader drag_column;
			int drag_x;
			int drag_to_index = -1;
			ColumnHeader entered_column_header;

			public HeaderControl (ListView owner)
			{
				this.owner = owner;
				this.SetStyle (ControlStyles.DoubleBuffer, true);
				MouseDown += new MouseEventHandler (HeaderMouseDown);
				MouseMove += new MouseEventHandler (HeaderMouseMove);
				MouseUp += new MouseEventHandler (HeaderMouseUp);
				MouseLeave += new EventHandler (OnMouseLeave);
			}

			internal ColumnHeader EnteredColumnHeader {
				get { return entered_column_header; }
				private set {
					if (entered_column_header == value)
						return;
					if (ThemeEngine.Current.ListViewHasHotHeaderStyle) {
						Region region_to_invalidate = new Region ();
						region_to_invalidate.MakeEmpty ();
						if (entered_column_header != null)
							region_to_invalidate.Union (GetColumnHeaderInvalidateArea (entered_column_header));
						entered_column_header = value;
						if (entered_column_header != null)
							region_to_invalidate.Union (GetColumnHeaderInvalidateArea (entered_column_header));
						Invalidate (region_to_invalidate);
						region_to_invalidate.Dispose ();
					} else
						entered_column_header = value;
				}
			}

			void OnMouseLeave (object sender, EventArgs e)
			{
				EnteredColumnHeader = null;
			}

			private ColumnHeader ColumnAtX (int x)
			{
				Point pt = new Point (x, 0);
				ColumnHeader result = null;
				foreach (ColumnHeader col in owner.Columns) {
					if (col.Rect.Contains (pt)) {
						result = col;
						break;
					}
				}
				return result;
			}

			private int GetReorderedIndex (ColumnHeader col)
			{
				if (owner.reordered_column_indices == null)
					return col.Index;
				else
					for (int i = 0; i < owner.Columns.Count; i++)
						if (owner.reordered_column_indices [i] == col.Index)
							return i;
				throw new Exception ("Column index missing from reordered array");
			}

			private void HeaderMouseDown (object sender, MouseEventArgs me)
			{
				if (resize_column != null) {
					column_resize_active = true;
					Capture = true;
					return;
				}

				clicked_column = ColumnAtX (me.X + owner.h_marker);

				if (clicked_column != null) {
					Capture = true;
					if (owner.AllowColumnReorder) {
						drag_x = me.X;
						drag_column = (ColumnHeader) (clicked_column as ICloneable).Clone ();
						drag_column.Rect = clicked_column.Rect;
						drag_to_index = GetReorderedIndex (clicked_column);
					}
					clicked_column.Pressed = true;
					Invalidate (clicked_column);
					return;
				}
			}

			void Invalidate (ColumnHeader columnHeader)
			{
				Invalidate (GetColumnHeaderInvalidateArea (columnHeader));
			}

			Rectangle GetColumnHeaderInvalidateArea (ColumnHeader columnHeader)
			{
				Rectangle bounds = columnHeader.Rect;
				bounds.X -= owner.h_marker;
				return bounds;
			}

			void StopResize ()
			{
				column_resize_active = false;
				resize_column = null;
				Capture = false;
				Cursor = Cursors.Default;
			}
			
			private void HeaderMouseMove (object sender, MouseEventArgs me)
			{
				Point pt = new Point (me.X + owner.h_marker, me.Y);

				if (column_resize_active) {
					int width = pt.X - resize_column.X;
					if (width < 0)
						width = 0;

					if (!owner.CanProceedWithResize (resize_column, width)){
						StopResize ();
						return;
					}
					resize_column.Width = width;
					return;
				}

				resize_column = null;

				if (clicked_column != null) {
					if (owner.AllowColumnReorder) {
						Rectangle r;

						r = drag_column.Rect;
						r.X = clicked_column.Rect.X + me.X - drag_x;
						drag_column.Rect = r;

						int x = me.X + owner.h_marker;
						ColumnHeader over = ColumnAtX (x);
						if (over == null)
							drag_to_index = owner.Columns.Count;
						else if (x < over.X + over.Width / 2)
							drag_to_index = GetReorderedIndex (over);
						else
							drag_to_index = GetReorderedIndex (over) + 1;
						Invalidate ();
					} else {
						ColumnHeader over = ColumnAtX (me.X + owner.h_marker);
						bool pressed = clicked_column.Pressed;
						clicked_column.Pressed = over == clicked_column;
						if (clicked_column.Pressed ^ pressed)
							Invalidate (clicked_column);
					}
					return;
				}

				for (int i = 0; i < owner.Columns.Count; i++) {
					Rectangle zone = owner.Columns [i].Rect;
					if (zone.Contains (pt))
						EnteredColumnHeader = owner.Columns [i];
					zone.X = zone.Right - 5;
					zone.Width = 10;
					if (zone.Contains (pt)) {
						if (i < owner.Columns.Count - 1 && owner.Columns [i + 1].Width == 0)
							i++;
						resize_column = owner.Columns [i];
						break;
					}
				}

				if (resize_column == null)
					Cursor = Cursors.Default;
				else
					Cursor = Cursors.VSplit;
			}

			void HeaderMouseUp (object sender, MouseEventArgs me)
			{
				Capture = false;

				if (column_resize_active) {
					int column_idx = resize_column.Index;
					StopResize ();
					owner.RaiseColumnWidthChanged (column_idx);
					return;
				}

				if (clicked_column != null && clicked_column.Pressed) {
					clicked_column.Pressed = false;
					Invalidate (clicked_column);
					owner.OnColumnClick (new ColumnClickEventArgs (clicked_column.Index));
				}

				if (drag_column != null && owner.AllowColumnReorder) {
					drag_column = null;
					if (drag_to_index > GetReorderedIndex (clicked_column))
						drag_to_index--;
					if (owner.GetReorderedColumn (drag_to_index) != clicked_column)
						owner.ReorderColumn (clicked_column, drag_to_index, true);
					drag_to_index = -1;
					Invalidate ();
				}

				clicked_column = null;
			}

			internal override void OnPaintInternal (PaintEventArgs pe)
			{
				if (owner.updating)
					return;
				
				Theme theme = ThemeEngine.Current;
				theme.DrawListViewHeader (pe.Graphics, pe.ClipRectangle, this.owner);

				if (drag_column == null)
					return;

				int target_x;
				if (drag_to_index == owner.Columns.Count)
					target_x = owner.GetReorderedColumn (drag_to_index - 1).Rect.Right - owner.h_marker;
				else
					target_x = owner.GetReorderedColumn (drag_to_index).Rect.X - owner.h_marker;
				theme.DrawListViewHeaderDragDetails (pe.Graphics, owner, drag_column, target_x);
			}

			protected override void WndProc (ref Message m)
			{
				switch ((Msg)m.Msg) {
				case Msg.WM_SETFOCUS:
					owner.Focus ();
					break;
				default:
					base.WndProc (ref m);
					break;
				}
			}
		}

		private class ItemComparer : IComparer {
			readonly SortOrder sort_order;

			public ItemComparer (SortOrder sortOrder)
			{
				sort_order = sortOrder;
			}

			public int Compare (object x, object y)
			{
				ListViewItem item_x = x as ListViewItem;
				ListViewItem item_y = y as ListViewItem;
				if (sort_order == SortOrder.Ascending)
					return String.Compare (item_x.Text, item_y.Text);
				else
					return String.Compare (item_y.Text, item_x.Text);
			}
		}

#if NET_2_0
		[ListBindable (false)]
#endif
		public class CheckedIndexCollection : IList, ICollection, IEnumerable
		{
			private readonly ListView owner;

			#region Public Constructor
			public CheckedIndexCollection (ListView owner)
			{
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get { return owner.CheckedItems.Count; }
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public int this [int index] {
				get {
					int [] indices = GetIndices ();
					if (index < 0 || index >= indices.Length)
						throw new ArgumentOutOfRangeException ("index");
					return indices [index];
				}
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return true; }
			}

			object IList.this [int index] {
				get { return this [index]; }
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
			public bool Contains (int checkedIndex)
			{
				int [] indices = GetIndices ();
				for (int i = 0; i < indices.Length; i++) {
					if (indices [i] == checkedIndex)
						return true;
				}
				return false;
			}

			public IEnumerator GetEnumerator ()
			{
				int [] indices = GetIndices ();
				return indices.GetEnumerator ();
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				int [] indices = GetIndices ();
				Array.Copy (indices, 0, dest, index, indices.Length);
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ("Add operation is not supported.");
			}

			void IList.Clear ()
			{
				throw new NotSupportedException ("Clear operation is not supported.");
			}

			bool IList.Contains (object checkedIndex)
			{
				if (!(checkedIndex is int))
					return false;
				return Contains ((int) checkedIndex);
			}

			int IList.IndexOf (object checkedIndex)
			{
				if (!(checkedIndex is int))
					return -1;
				return IndexOf ((int) checkedIndex);
			}

			void IList.Insert (int index, object value)
			{
				throw new NotSupportedException ("Insert operation is not supported.");
			}

			void IList.Remove (object value)
			{
				throw new NotSupportedException ("Remove operation is not supported.");
			}

			void IList.RemoveAt (int index)
			{
				throw new NotSupportedException ("RemoveAt operation is not supported.");
			}

			public int IndexOf (int checkedIndex)
			{
				int [] indices = GetIndices ();
				for (int i = 0; i < indices.Length; i++) {
					if (indices [i] == checkedIndex)
						return i;
				}
				return -1;
			}
			#endregion	// Public Methods

			private int [] GetIndices ()
			{
				ArrayList checked_items = owner.CheckedItems.List;
				int [] indices = new int [checked_items.Count];
				for (int i = 0; i < checked_items.Count; i++) {
					ListViewItem item = (ListViewItem) checked_items [i];
					indices [i] = item.Index;
				}
				return indices;
			}
		}	// CheckedIndexCollection

#if NET_2_0
		[ListBindable (false)]
#endif
		public class CheckedListViewItemCollection : IList, ICollection, IEnumerable
		{
			private readonly ListView owner;
			private ArrayList list;

			#region Public Constructor
			public CheckedListViewItemCollection (ListView owner)
			{
				this.owner = owner;
				this.owner.Items.Changed += new CollectionChangedHandler (
					ItemsCollection_Changed);
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get {
					if (!owner.CheckBoxes)
						return 0;
					return List.Count;
				}
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public ListViewItem this [int index] {
				get {
#if NET_2_0
					if (owner.VirtualMode)
						throw new InvalidOperationException ();
#endif
					ArrayList checked_items = List;
					if (index < 0 || index >= checked_items.Count)
						throw new ArgumentOutOfRangeException ("index");
					return (ListViewItem) checked_items [index];
				}
			}

#if NET_2_0
			public virtual ListViewItem this [string key] {
				get {
					int idx = IndexOfKey (key);
					return idx == -1 ? null : (ListViewItem) List [idx];
				}
			}
#endif

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return true; }
			}

			object IList.this [int index] {
				get { return this [index]; }
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
			public bool Contains (ListViewItem item)
			{
				if (!owner.CheckBoxes)
					return false;
				return List.Contains (item);
			}

#if NET_2_0
			public virtual bool ContainsKey (string key)
			{
				return IndexOfKey (key) != -1;
			}
#endif

			public void CopyTo (Array dest, int index)
			{
#if NET_2_0
				if (owner.VirtualMode)
					throw new InvalidOperationException ();
#endif
				if (!owner.CheckBoxes)
					return;
				List.CopyTo (dest, index);
			}

			public IEnumerator GetEnumerator ()
			{
#if NET_2_0
				if (owner.VirtualMode)
					throw new InvalidOperationException ();
#endif
				if (!owner.CheckBoxes)
					return (new ListViewItem [0]).GetEnumerator ();
				return List.GetEnumerator ();
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ("Add operation is not supported.");
			}

			void IList.Clear ()
			{
				throw new NotSupportedException ("Clear operation is not supported.");
			}

			bool IList.Contains (object item)
			{
				if (!(item is ListViewItem))
					return false;
				return Contains ((ListViewItem) item);
			}

			int IList.IndexOf (object item)
			{
				if (!(item is ListViewItem))
					return -1;
				return IndexOf ((ListViewItem) item);
			}

			void IList.Insert (int index, object value)
			{
				throw new NotSupportedException ("Insert operation is not supported.");
			}

			void IList.Remove (object value)
			{
				throw new NotSupportedException ("Remove operation is not supported.");
			}

			void IList.RemoveAt (int index)
			{
				throw new NotSupportedException ("RemoveAt operation is not supported.");
			}

			public int IndexOf (ListViewItem item)
			{
#if NET_2_0
				if (owner.VirtualMode)
					throw new InvalidOperationException ();
#endif
				if (!owner.CheckBoxes)
					return -1;
				return List.IndexOf (item);
			}

#if NET_2_0
			public virtual int IndexOfKey (string key)
			{
#if NET_2_0
				if (owner.VirtualMode)
					throw new InvalidOperationException ();
#endif
				if (key == null || key.Length == 0)
					return -1;

				ArrayList checked_items = List;
				for (int i = 0; i < checked_items.Count; i++) {
					ListViewItem item = (ListViewItem) checked_items [i];
					if (String.Compare (key, item.Name, true) == 0)
						return i;
				}

				return -1;
			}
#endif
			#endregion	// Public Methods

			internal ArrayList List {
				get {
					if (list == null) {
						list = new ArrayList ();
						foreach (ListViewItem item in owner.Items) {
							if (item.Checked)
								list.Add (item);
						}
					}
					return list;
				}
			}

			internal void Reset ()
			{
				// force re-population of list
				list = null;
			}

			private void ItemsCollection_Changed ()
			{
				Reset ();
			}
		}	// CheckedListViewItemCollection

#if NET_2_0
		[ListBindable (false)]
#endif
		public class ColumnHeaderCollection : IList, ICollection, IEnumerable
		{
			internal ArrayList list;
			private ListView owner;

			#region UIA Framework Events 
#if NET_2_0
			//NOTE:
			//	We are using Reflection to add/remove internal events.
			//	Class ListViewProvider uses the events when View is Details.
			//
			//Event used to generate UIA StructureChangedEvent
			static object UIACollectionChangedEvent = new object ();

			internal event CollectionChangeEventHandler UIACollectionChanged {
				add { 
					if (owner != null)
						owner.Events.AddHandler (UIACollectionChangedEvent, value); 
				}
				remove { 
					if (owner != null)
						owner.Events.RemoveHandler (UIACollectionChangedEvent, value); 
				}
			}

			internal void OnUIACollectionChangedEvent (CollectionChangeEventArgs args)
			{
				if (owner == null)
					return;

				CollectionChangeEventHandler eh
					= (CollectionChangeEventHandler) owner.Events [UIACollectionChangedEvent];
				if (eh != null)
					eh (owner, args);
			}

#endif
			#endregion UIA Framework Events 

			#region Public Constructor
			public ColumnHeaderCollection (ListView owner)
			{
				list = new ArrayList ();
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get { return list.Count; }
			}

			public bool IsReadOnly {
				get { return false; }
			}

			public virtual ColumnHeader this [int index] {
				get {
					if (index < 0 || index >= list.Count)
						throw new ArgumentOutOfRangeException ("index");
					return (ColumnHeader) list [index];
				}
			}

#if NET_2_0
			public virtual ColumnHeader this [string key] {
				get {
					int idx = IndexOfKey (key);
					if (idx == -1)
						return null;

					return (ColumnHeader) list [idx];
				}
			}
#endif

			bool ICollection.IsSynchronized {
				get { return true; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return list.IsFixedSize; }
			}

			object IList.this [int index] {
				get { return this [index]; }
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
			public virtual int Add (ColumnHeader value)
			{
				int idx = list.Add (value);
				owner.AddColumn (value, idx, true);

#if NET_2_0
				//UIA Framework event: Item Added
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, value));
#endif

				return idx;
			}

#if NET_2_0
			public virtual ColumnHeader Add (string text, int width, HorizontalAlignment textAlign)
			{
				string str = text;
#else
			public virtual ColumnHeader Add (string str, int width, HorizontalAlignment textAlign)
			{
#endif
				ColumnHeader colHeader = new ColumnHeader (this.owner, str, textAlign, width);
				this.Add (colHeader);
				return colHeader;
			}

#if NET_2_0
			public virtual ColumnHeader Add (string text)
			{
				return Add (String.Empty, text);
			}

			public virtual ColumnHeader Add (string text, int width)
			{
				return Add (String.Empty, text, width);
			}

			public virtual ColumnHeader Add (string key, string text)
			{
				ColumnHeader colHeader = new ColumnHeader ();
				colHeader.Name = key;
				colHeader.Text = text;
				Add (colHeader);
				return colHeader;
			}

			public virtual ColumnHeader Add (string key, string text, int width)
			{
				return Add (key, text, width, HorizontalAlignment.Left, -1);
			}

			public virtual ColumnHeader Add (string key, string text, int width, HorizontalAlignment textAlign, int imageIndex)
			{
				ColumnHeader colHeader = new ColumnHeader (key, text, width, textAlign);
				colHeader.ImageIndex = imageIndex;
				Add (colHeader);
				return colHeader;
			}

			public virtual ColumnHeader Add (string key, string text, int width, HorizontalAlignment textAlign, string imageKey)
			{
				ColumnHeader colHeader = new ColumnHeader (key, text, width, textAlign);
				colHeader.ImageKey = imageKey;
				Add (colHeader);
				return colHeader;
			}
#endif

			public virtual void AddRange (ColumnHeader [] values)
			{
				foreach (ColumnHeader colHeader in values) {
					int idx = list.Add (colHeader);
					owner.AddColumn (colHeader, idx, false);
				}
				
				owner.Redraw (true);
			}

			public virtual void Clear ()
			{
				foreach (ColumnHeader col in list)
					col.SetListView (null);
				list.Clear ();
				owner.ReorderColumns (new int [0], true);

#if NET_2_0
				//UIA Framework event: Items cleared
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Refresh, null));
#endif

			}

			public bool Contains (ColumnHeader value)
			{
				return list.Contains (value);
			}

#if NET_2_0
			public virtual bool ContainsKey (string key)
			{
				return IndexOfKey (key) != -1;
			}
#endif

			public IEnumerator GetEnumerator ()
			{
				return list.GetEnumerator ();
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
			}

			int IList.Add (object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException ("Not of type ColumnHeader", "value");
				}

				return this.Add ((ColumnHeader) value);
			}

			bool IList.Contains (object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException ("Not of type ColumnHeader", "value");
				}

				return this.Contains ((ColumnHeader) value);
			}

			int IList.IndexOf (object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException ("Not of type ColumnHeader", "value");
				}

				return this.IndexOf ((ColumnHeader) value);
			}

			void IList.Insert (int index, object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException ("Not of type ColumnHeader", "value");
				}

				this.Insert (index, (ColumnHeader) value);
			}

			void IList.Remove (object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException ("Not of type ColumnHeader", "value");
				}

				this.Remove ((ColumnHeader) value);
			}

			public int IndexOf (ColumnHeader value)
			{
				return list.IndexOf (value);
			}

#if NET_2_0
			public virtual int IndexOfKey (string key)
			{
				if (key == null || key.Length == 0)
					return -1;

				for (int i = 0; i < list.Count; i++) {
					ColumnHeader col = (ColumnHeader) list [i];
					if (String.Compare (key, col.Name, true) == 0)
						return i;
				}

				return -1;
			}
#endif

			public void Insert (int index, ColumnHeader value)
			{
				// LAMESPEC: MSDOCS say greater than or equal to the value of the Count property
				// but it's really only greater.
				if (index < 0 || index > list.Count)
					throw new ArgumentOutOfRangeException ("index");

				list.Insert (index, value);
				owner.AddColumn (value, index, true);

#if NET_2_0
				//UIA Framework event: Item added
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, value));
#endif
			}

#if NET_2_0
			public void Insert (int index, string text)
			{
				Insert (index, String.Empty, text);
			}

			public void Insert (int index, string text, int width)
			{
				Insert (index, String.Empty, text, width);
			}

			public void Insert (int index, string key, string text)
			{
				ColumnHeader colHeader = new ColumnHeader ();
				colHeader.Name = key;
				colHeader.Text = text;
				Insert (index, colHeader);
			}

			public void Insert (int index, string key, string text, int width)
			{
				ColumnHeader colHeader = new ColumnHeader (key, text, width, HorizontalAlignment.Left);
				Insert (index, colHeader);
			}

			public void Insert (int index, string key, string text, int width, HorizontalAlignment textAlign, int imageIndex)
			{
				ColumnHeader colHeader = new ColumnHeader (key, text, width, textAlign);
				colHeader.ImageIndex = imageIndex;
				Insert (index, colHeader);
			}

			public void Insert (int index, string key, string text, int width, HorizontalAlignment textAlign, string imageKey)
			{
				ColumnHeader colHeader = new ColumnHeader (key, text, width, textAlign);
				colHeader.ImageKey = imageKey;
				Insert (index, colHeader);
			}
#endif

#if NET_2_0
			public void Insert (int index, string text, int width, HorizontalAlignment textAlign)
			{
				string str = text;
#else
			public void Insert (int index, string str, int width, HorizontalAlignment textAlign)
			{
#endif
				ColumnHeader colHeader = new ColumnHeader (this.owner, str, textAlign, width);
				this.Insert (index, colHeader);
			}

			public virtual void Remove (ColumnHeader column)
			{
				if (!Contains (column))
					return;

				list.Remove (column);
				column.SetListView (null);

				int rem_display_index = column.InternalDisplayIndex;
				int [] display_indices = new int [list.Count];
				for (int i = 0; i < display_indices.Length; i++) {
					ColumnHeader col = (ColumnHeader) list [i];
					int display_index = col.InternalDisplayIndex;
					if (display_index < rem_display_index) {
						display_indices [i] = display_index;
					} else {
						display_indices [i] = (display_index - 1);
					}
				}

				column.InternalDisplayIndex = -1;
				owner.ReorderColumns (display_indices, true);

#if NET_2_0
				//UIA Framework event: Item Removed
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Remove, column));
#endif
			}

#if NET_2_0
			public virtual void RemoveByKey (string key)
			{
				int idx = IndexOfKey (key);
				if (idx != -1)
					RemoveAt (idx);
			}
#endif

			public virtual void RemoveAt (int index)
			{
				if (index < 0 || index >= list.Count)
					throw new ArgumentOutOfRangeException ("index");

				ColumnHeader col = (ColumnHeader) list [index];
				Remove (col);
			}
			#endregion	// Public Methods
			

		}	// ColumnHeaderCollection

#if NET_2_0
		[ListBindable (false)]
#endif
		public class ListViewItemCollection : IList, ICollection, IEnumerable
		{
			private readonly ArrayList list;
			private ListView owner;
#if NET_2_0
			private ListViewGroup group;
#endif

			#region UIA Framework Events 
#if NET_2_0
			//NOTE:
			//	We are using Reflection to add/remove internal events.
			//	Class ListViewProvider uses the events.
			//
			//Event used to generate UIA StructureChangedEvent
			static object UIACollectionChangedEvent = new object ();

			internal event CollectionChangeEventHandler UIACollectionChanged {
				add { 
					if (owner != null)
						owner.Events.AddHandler (UIACollectionChangedEvent, value); 
				}
				remove { 
					if (owner != null)
						owner.Events.RemoveHandler (UIACollectionChangedEvent, value); 
				}
			}

			internal void OnUIACollectionChangedEvent (CollectionChangeEventArgs args)
			{
				if (owner == null)
					return;

				CollectionChangeEventHandler eh
					= (CollectionChangeEventHandler) owner.Events [UIACollectionChangedEvent];
				if (eh != null)
					eh (owner, args);
			}

#endif
			#endregion UIA Framework Events 

			// The collection can belong to a ListView (main) or to a ListViewGroup (sub-collection)
			// In the later case ListViewItem.ListView never gets modified
			private bool is_main_collection = true;

			#region Public Constructor
			public ListViewItemCollection (ListView owner)
			{
				list = new ArrayList (0);
				this.owner = owner;
			}
			#endregion	// Public Constructor

#if NET_2_0
			internal ListViewItemCollection (ListView owner, ListViewGroup group) : this (owner)
			{
				this.group = group;
				is_main_collection = false;
			}
#endif

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get {
#if NET_2_0
					if (owner != null && owner.VirtualMode)
						return owner.VirtualListSize;
#endif

					return list.Count; 
				}
			}

			public bool IsReadOnly {
				get { return false; }
			}

#if NET_2_0
			public virtual ListViewItem this [int index] {
#else
			public virtual ListViewItem this [int displayIndex] {
#endif
				get {
#if !NET_2_0
					int index = displayIndex;
#endif

					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("index");

#if NET_2_0
					if (owner != null && owner.VirtualMode)
						return RetrieveVirtualItemFromOwner (index);
#endif
					return (ListViewItem) list [index];
				}

				set {
#if !NET_2_0
					int index = displayIndex;
#endif

					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("index");

#if NET_2_0
					if (owner != null && owner.VirtualMode)
						throw new InvalidOperationException ();
#endif

					if (list.Contains (value))
						throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "value");

					if (value.ListView != null && value.ListView != owner)
						throw new ArgumentException ("Cannot add or insert the item '" + value.Text + "' in more than one place. You must first remove it from its current location or clone it.", "value");

					if (is_main_collection)
						value.Owner = owner;
#if NET_2_0
					else {
						if (value.Group != null)
							value.Group.Items.Remove (value);

						value.SetGroup (group);
					}
#endif

#if NET_2_0
					//UIA Framework event: Item Replaced
					OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Remove, list [index]));
#endif

					list [index] = value;

					CollectionChanged (true);

#if NET_2_0
					//UIA Framework event: Item Replaced
					OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, value));
#endif

				}
			}

#if NET_2_0
			public virtual ListViewItem this [string key] {
				get {
					int idx = IndexOfKey (key);
					if (idx == -1)
						return null;

					return this [idx];
				}
			}
#endif

			bool ICollection.IsSynchronized {
				get { return true; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return list.IsFixedSize; }
			}

			object IList.this [int index] {
				get { return this [index]; }
				set {
#if NET_2_0
					//UIA Framework event: Item Replaced
					OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Remove, this [index]));
#endif

					if (value is ListViewItem)
						this [index] = (ListViewItem) value;
					else
						this [index] = new ListViewItem (value.ToString ());

					OnChange ();
#if NET_2_0
					//UIA Framework event: Item Replaced
					OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, value));
#endif
				}
			}
			#endregion	// Public Properties

			#region Public Methods
			public virtual ListViewItem Add (ListViewItem value)
			{
#if NET_2_0
				if (owner != null && owner.VirtualMode)
					throw new InvalidOperationException ();
#endif

				AddItem (value);

				// Item is ignored until it has been added to the ListView
				if (is_main_collection || value.ListView != null)
					CollectionChanged (true);

#if NET_2_0
				//UIA Framework event: Item Added
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, value));
#endif

				return value;
			}

			public virtual ListViewItem Add (string text)
			{
				ListViewItem item = new ListViewItem (text);
				return this.Add (item);
			}

			public virtual ListViewItem Add (string text, int imageIndex)
			{
				ListViewItem item = new ListViewItem (text, imageIndex);
				return this.Add (item);
			}

#if NET_2_0
			public virtual ListViewItem Add (string text, string imageKey)
			{
				ListViewItem item = new ListViewItem (text, imageKey);
				return this.Add (item);
			}

			public virtual ListViewItem Add (string key, string text, int imageIndex)
			{
				ListViewItem item = new ListViewItem (text, imageIndex);
				item.Name = key;
				return this.Add (item);
			}

			public virtual ListViewItem Add (string key, string text, string imageKey)
			{
				ListViewItem item = new ListViewItem (text, imageKey);
				item.Name = key;
				return this.Add (item);
			}
#endif

#if NET_2_0
			public void AddRange (ListViewItem [] items)
			{
#else
			public void AddRange (ListViewItem [] values)
			{
				ListViewItem [] items = values;
#endif
				if (items == null)
					throw new ArgumentNullException ("Argument cannot be null!", "items");
#if NET_2_0
				if (owner != null && owner.VirtualMode)
					throw new InvalidOperationException ();
#endif

				owner.BeginUpdate ();
				
				foreach (ListViewItem item in items) {
					AddItem (item);

#if NET_2_0
					//UIA Framework event: Item Added
					OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, item));
#endif
				}

				owner.EndUpdate ();
				
				CollectionChanged (true);
			}

#if NET_2_0
			public void AddRange (ListViewItemCollection items)
			{
				if (items == null)
					throw new ArgumentNullException ("Argument cannot be null!", "items");

				ListViewItem[] itemArray = new ListViewItem[items.Count];
				items.CopyTo (itemArray,0);
				this.AddRange (itemArray);
			}
#endif

			public virtual void Clear ()
			{
#if NET_2_0
				if (owner != null && owner.VirtualMode)
					throw new InvalidOperationException ();
#endif
				if (is_main_collection && owner != null) {
					owner.SetFocusedItem (-1);
					owner.h_scroll.Value = owner.v_scroll.Value = 0;

#if NET_2_0
					// first remove any item in the groups that *are* part of this LV too
					foreach (ListViewGroup group in owner.groups)
						group.Items.ClearItemsWithSameListView ();
#endif
				
					foreach (ListViewItem item in list) {
						owner.item_control.CancelEdit (item);
						item.Owner = null;
					}
				}
#if NET_2_0
				else
					foreach (ListViewItem item in list)
						item.SetGroup (null);
#endif

				list.Clear ();
				CollectionChanged (false);

#if NET_2_0
				//UIA Framework event: Items Removed
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Refresh, null));
#endif

			}

#if NET_2_0
			// This method is intended to be used from ListViewGroup.Items, not from ListView.Items,
			// added for performance reasons (avoid calling manually Remove for every item on ListViewGroup.Items)
			void ClearItemsWithSameListView ()
			{
				if (is_main_collection)
					return;

				int counter = list.Count - 1;
				while (counter >= 0) {
					ListViewItem item = list [counter] as ListViewItem;

					// remove only if the items in group have being added to the ListView too
					if (item.ListView == group.ListView) {
						list.RemoveAt (counter);
						item.SetGroup (null);
					}
						
					counter--;
				}
			}
#endif

			public bool Contains (ListViewItem item)
			{
				return IndexOf (item) != -1;
			}

#if NET_2_0
			public virtual bool ContainsKey (string key)
			{
				return IndexOfKey (key) != -1;
			}
#endif

			public void CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
			}

#if NET_2_0
			public ListViewItem [] Find (string key, bool searchAllSubItems)
			{
				if (key == null)
					return new ListViewItem [0];

				List<ListViewItem> temp_list = new List<ListViewItem> ();
				
				for (int i = 0; i < list.Count; i++) {
					ListViewItem lvi = (ListViewItem) list [i];
					if (String.Compare (key, lvi.Name, true) == 0)
						temp_list.Add (lvi);
				}

				ListViewItem [] retval = new ListViewItem [temp_list.Count];
				temp_list.CopyTo (retval);

				return retval;
			}
#endif

			public IEnumerator GetEnumerator ()
			{
#if NET_2_0
				if (owner != null && owner.VirtualMode)
					throw new InvalidOperationException ();
#endif

				// This enumerator makes a copy of the collection so
				// it can be deleted from in a foreach
				return new Control.ControlCollection.ControlCollectionEnumerator (list);
			}

			int IList.Add (object item)
			{
				int result;
				ListViewItem li;

#if NET_2_0
				if (owner != null && owner.VirtualMode)
					throw new InvalidOperationException ();
#endif

				if (item is ListViewItem) {
					li = (ListViewItem) item;
					if (list.Contains (li))
						throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "item");

					if (li.ListView != null && li.ListView != owner)
						throw new ArgumentException ("Cannot add or insert the item '" + li.Text + "' in more than one place. You must first remove it from its current location or clone it.", "item");
				}
				else
					li = new ListViewItem (item.ToString ());

				li.Owner = owner;
				
				
				result = list.Add (li);
				CollectionChanged (true);

#if NET_2_0
				//UIA Framework event: Item Added
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, li));
#endif

				return result;
			}

			bool IList.Contains (object item)
			{
				return Contains ((ListViewItem) item);
			}

			int IList.IndexOf (object item)
			{
				return IndexOf ((ListViewItem) item);
			}

			void IList.Insert (int index, object item)
			{
				if (item is ListViewItem)
					this.Insert (index, (ListViewItem) item);
				else
					this.Insert (index, item.ToString ());

#if NET_2_0
				//UIA Framework event: Item Added
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, this [index]));
#endif
			}

			void IList.Remove (object item)
			{
				Remove ((ListViewItem) item);
			}

			public int IndexOf (ListViewItem item)
			{
#if NET_2_0
				if (owner != null && owner.VirtualMode) {
					for (int i = 0; i < Count; i++)
						if (RetrieveVirtualItemFromOwner (i) == item)
							return i;

					return -1;
				}
#endif
				
				return list.IndexOf (item);
			}

#if NET_2_0
			public virtual int IndexOfKey (string key)
			{
				if (key == null || key.Length == 0)
					return -1;

				for (int i = 0; i < Count; i++) {
					ListViewItem lvi = this [i];
					if (String.Compare (key, lvi.Name, true) == 0)
						return i;
				}

				return -1;
			}
#endif

			public ListViewItem Insert (int index, ListViewItem item)
			{
				if (index < 0 || index > list.Count)
					throw new ArgumentOutOfRangeException ("index");

#if NET_2_0
				if (owner != null && owner.VirtualMode)
					throw new InvalidOperationException ();
#endif

				if (list.Contains (item))
					throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "item");

				if (item.ListView != null && item.ListView != owner)
					throw new ArgumentException ("Cannot add or insert the item '" + item.Text + "' in more than one place. You must first remove it from its current location or clone it.", "item");

				if (is_main_collection)
					item.Owner = owner;
#if NET_2_0
				else {
					if (item.Group != null)
						item.Group.Items.Remove (item);

					item.SetGroup (group);
				}
#endif

				list.Insert (index, item);

				if (is_main_collection || item.ListView != null)
					CollectionChanged (true);

#if NET_2_0
				//UIA Framework event: Item Added
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, item));
#endif

				return item;
			}

			public ListViewItem Insert (int index, string text)
			{
				return this.Insert (index, new ListViewItem (text));
			}

			public ListViewItem Insert (int index, string text, int imageIndex)
			{
				return this.Insert (index, new ListViewItem (text, imageIndex));
			}

#if NET_2_0
			public ListViewItem Insert (int index, string text, string imageKey)
			{
				ListViewItem lvi = new ListViewItem (text, imageKey);
				return Insert (index, lvi);
			}

			public virtual ListViewItem Insert (int index, string key, string text, int imageIndex)
			{
				ListViewItem lvi = new ListViewItem (text, imageIndex);
				lvi.Name = key;
				return Insert (index, lvi);
			}

			public virtual ListViewItem Insert (int index, string key, string text, string imageKey)
			{
				ListViewItem lvi = new ListViewItem (text, imageKey);
				lvi.Name = key;
				return Insert (index, lvi);
			}
#endif

			public virtual void Remove (ListViewItem item)
			{
#if NET_2_0
				if (owner != null && owner.VirtualMode)
					throw new InvalidOperationException ();
#endif

				int idx = list.IndexOf (item);
				if (idx != -1)
					RemoveAt (idx);
			}

			public virtual void RemoveAt (int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("index");

#if NET_2_0
				if (owner != null && owner.VirtualMode)
					throw new InvalidOperationException ();
#endif

				ListViewItem item = (ListViewItem) list [index];

				bool selection_changed = false;
				if (is_main_collection && owner != null) {

					int display_index = item.DisplayIndex;
					if (item.Focused && display_index + 1 == Count) // Last item
						owner.SetFocusedItem (display_index == 0 ? -1 : display_index - 1);

					selection_changed = owner.SelectedIndices.Contains (index);
					owner.item_control.CancelEdit (item);
				}

				list.RemoveAt (index);

#if NET_2_0
				if (is_main_collection) {
					item.Owner = null;
					if (item.Group != null)
						item.Group.Items.Remove (item);
				} else
					item.SetGroup (null);
#else
				item.Owner = null;
#endif

				CollectionChanged (false);
				if (selection_changed && owner != null)
					owner.OnSelectedIndexChanged (EventArgs.Empty);


#if NET_2_0
				//UIA Framework event: Item Removed 
				OnUIACollectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Remove, item));
#endif
			}

#if NET_2_0
			public virtual void RemoveByKey (string key)
			{
				int idx = IndexOfKey (key);
				if (idx != -1)
					RemoveAt (idx);
			}
#endif

			#endregion	// Public Methods

			internal ListView Owner {
				get {
					return owner;
				}
				set {
					owner = value;
				}
			}

#if NET_2_0
			internal ListViewGroup Group {
				get {
					return group;
				}
				set {
					group = value;
				}
			}
#endif

			void AddItem (ListViewItem value)
			{
				if (list.Contains (value))
					throw new ArgumentException ("An item cannot be added more than once. To add an item again, you need to clone it.", "value");

				if (value.ListView != null && value.ListView != owner)
					throw new ArgumentException ("Cannot add or insert the item '" + value.Text + "' in more than one place. You must first remove it from its current location or clone it.", "value");
				if (is_main_collection)
					value.Owner = owner;
#if NET_2_0
				else {
					if (value.Group != null)
						value.Group.Items.Remove (value);

					value.SetGroup (group);
				}
#endif

				list.Add (value);

			}

			void CollectionChanged (bool sort)
			{
				if (owner != null) {
				        if (sort)
				                owner.Sort (false);

					OnChange ();
					owner.Redraw (true);
				}
			}

#if NET_2_0
			ListViewItem RetrieveVirtualItemFromOwner (int displayIndex)
			{
				RetrieveVirtualItemEventArgs args = new RetrieveVirtualItemEventArgs (displayIndex);

				owner.OnRetrieveVirtualItem (args);
				ListViewItem retval = args.Item;
				retval.Owner = owner;
				retval.DisplayIndex = displayIndex;
				retval.Layout ();

				return retval;
			}
#endif

			internal event CollectionChangedHandler Changed;

			internal void Sort (IComparer comparer)
			{
				list.Sort (comparer);
				OnChange ();
			}

			internal void OnChange ()
			{
				if (Changed != null)
					Changed ();
			}
		}	// ListViewItemCollection

			
		// In normal mode, the selection information resides in the Items,
		// making SelectedIndexCollection.List read-only
		//
		// In virtual mode, SelectedIndexCollection directly saves the selection
		// information, instead of getting it from Items, making List read-and-write
#if NET_2_0
		[ListBindable (false)]
#endif
		public class SelectedIndexCollection : IList, ICollection, IEnumerable
		{
			private readonly ListView owner;
			private ArrayList list;

			#region Public Constructor
			public SelectedIndexCollection (ListView owner)
			{
				this.owner = owner;
				owner.Items.Changed += new CollectionChangedHandler (ItemsCollection_Changed);
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get {
					if (!owner.is_selection_available)
						return 0;

					return List.Count;
				}
			}

			public bool IsReadOnly {
				get { 
#if NET_2_0
					return false;
#else
					return true; 
#endif
				}
			}

			public int this [int index] {
				get {
					if (!owner.is_selection_available || index < 0 || index >= List.Count)
						throw new ArgumentOutOfRangeException ("index");

					return (int) List [index];
				}
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { 
#if NET_2_0
					return false;
#else
					return true;
#endif
				}
			}

			object IList.this [int index] {
				get { return this [index]; }
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
#if NET_2_0
			public int Add (int itemIndex)
			{
				if (itemIndex < 0 || itemIndex >= owner.Items.Count)
					throw new ArgumentOutOfRangeException ("index");

				if (owner.virtual_mode && !owner.is_selection_available)
					return -1;

				owner.Items [itemIndex].Selected = true;

				if (!owner.is_selection_available)
					return 0;

				return List.Count;
			}
#endif

#if NET_2_0
			public 
#else
			internal
#endif	
			void Clear ()
			{
				if (!owner.is_selection_available)
					return;

				int [] indexes = (int []) List.ToArray (typeof (int));
				foreach (int index in indexes)
					owner.Items [index].Selected = false;
			}

			public bool Contains (int selectedIndex)
			{
				return IndexOf (selectedIndex) != -1;
			}

			public void CopyTo (Array dest, int index)
			{
				List.CopyTo (dest, index);
			}

			public IEnumerator GetEnumerator ()
			{
				return List.GetEnumerator ();
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ("Add operation is not supported.");
			}

			void IList.Clear ()
			{
				Clear ();
			}

			bool IList.Contains (object selectedIndex)
			{
				if (!(selectedIndex is int))
					return false;
				return Contains ((int) selectedIndex);
			}

			int IList.IndexOf (object selectedIndex)
			{
				if (!(selectedIndex is int))
					return -1;
				return IndexOf ((int) selectedIndex);
			}

			void IList.Insert (int index, object value)
			{
				throw new NotSupportedException ("Insert operation is not supported.");
			}

			void IList.Remove (object value)
			{
				throw new NotSupportedException ("Remove operation is not supported.");
			}

			void IList.RemoveAt (int index)
			{
				throw new NotSupportedException ("RemoveAt operation is not supported.");
			}

			public int IndexOf (int selectedIndex)
			{
				if (!owner.is_selection_available)
					return -1;

				return List.IndexOf (selectedIndex);
			}

#if NET_2_0
			public void Remove (int itemIndex)
			{
				if (itemIndex < 0 || itemIndex >= owner.Items.Count)
					throw new ArgumentOutOfRangeException ("itemIndex");

				owner.Items [itemIndex].Selected = false;
			}
#endif
			#endregion	// Public Methods

			internal ArrayList List {
				get {
					if (list == null) {
						list = new ArrayList ();
#if NET_2_0
						if (!owner.VirtualMode)
#endif
						for (int i = 0; i < owner.Items.Count; i++) {
							if (owner.Items [i].Selected)
								list.Add (i);
						}
					}
					return list;
				}
			}

			internal void Reset ()
			{
				// force re-population of list
				list = null;
			}

			private void ItemsCollection_Changed ()
			{
				Reset ();
			}

#if NET_2_0
			internal void RemoveIndex (int index)
			{
				int idx = List.BinarySearch (index);
				if (idx != -1)
					List.RemoveAt (idx);
			}

			// actually store index in the collection
			// also, keep the collection sorted, as .Net does
			internal void InsertIndex (int index)
			{
				int iMin = 0;
				int iMax = List.Count - 1;
				while (iMin <= iMax) {
					int iMid = (iMin + iMax) / 2;
					int current_index = (int) List [iMid];

					if (current_index == index)
						return; // Already added
					if (current_index > index)
						iMax = iMid - 1;
					else
						iMin = iMid + 1;
				}

				List.Insert (iMin, index);
			}
#endif

		}	// SelectedIndexCollection

#if NET_2_0
		[ListBindable (false)]
#endif
		public class SelectedListViewItemCollection : IList, ICollection, IEnumerable
		{
			private readonly ListView owner;

			#region Public Constructor
			public SelectedListViewItemCollection (ListView owner)
			{
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get {
					return owner.SelectedIndices.Count;
				}
			}

			public bool IsReadOnly {
				get { return true; }
			}

			public ListViewItem this [int index] {
				get {
					if (!owner.is_selection_available || index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("index");

					int item_index = owner.SelectedIndices [index];
					return owner.Items [item_index];
				}
			}

#if NET_2_0
			public virtual ListViewItem this [string key] {
				get {
					int idx = IndexOfKey (key);
					if (idx == -1)
						return null;

					return this [idx];
				}
			}
#endif

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return true; }
			}

			object IList.this [int index] {
				get { return this [index]; }
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
			public void Clear ()
			{
				owner.SelectedIndices.Clear ();
			}

			public bool Contains (ListViewItem item)
			{
				return IndexOf (item) != -1;
			}

#if NET_2_0
			public virtual bool ContainsKey (string key)
			{
				return IndexOfKey (key) != -1;
			}
#endif

			public void CopyTo (Array dest, int index)
			{
				if (!owner.is_selection_available)
					return;
				if (index > Count) // Throws ArgumentException instead of IOOR exception
					throw new ArgumentException ("index");

				for (int i = 0; i < Count; i++)
					dest.SetValue (this [i], index++);
			}

			public IEnumerator GetEnumerator ()
			{
				if (!owner.is_selection_available)
					return (new ListViewItem [0]).GetEnumerator ();

				ListViewItem [] items = new ListViewItem [Count];
				for (int i = 0; i < Count; i++)
					items [i] = this [i];

				return items.GetEnumerator ();
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ("Add operation is not supported.");
			}

			bool IList.Contains (object item)
			{
				if (!(item is ListViewItem))
					return false;
				return Contains ((ListViewItem) item);
			}

			int IList.IndexOf (object item)
			{
				if (!(item is ListViewItem))
					return -1;
				return IndexOf ((ListViewItem) item);
			}

			void IList.Insert (int index, object value)
			{
				throw new NotSupportedException ("Insert operation is not supported.");
			}

			void IList.Remove (object value)
			{
				throw new NotSupportedException ("Remove operation is not supported.");
			}

			void IList.RemoveAt (int index)
			{
				throw new NotSupportedException ("RemoveAt operation is not supported.");
			}

			public int IndexOf (ListViewItem item)
			{
				if (!owner.is_selection_available)
					return -1;

				for (int i = 0; i < Count; i++)
					if (this [i] == item)
						return i;

				return -1;
			}

#if NET_2_0
			public virtual int IndexOfKey (string key)
			{
				if (!owner.is_selection_available || key == null || key.Length == 0)
					return -1;

				for (int i = 0; i < Count; i++) {
					ListViewItem item = this [i];
					if (String.Compare (item.Name, key, true) == 0)
						return i;
				}

				return -1;
			}
#endif
			#endregion	// Public Methods

		}	// SelectedListViewItemCollection

		internal delegate void CollectionChangedHandler ();

		struct ItemMatrixLocation
		{
			int row;
			int col;

			public ItemMatrixLocation (int row, int col)
			{
				this.row = row;
				this.col = col;
		
			}
		
			public int Col {
				get {
					return col;
				}
				set {
					col = value;
				}
			}

			public int Row {
				get {
					return row;
				}
				set {
					row = value;
				}
			}
	
		}

		#endregion // Subclasses
#if NET_2_0
		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);
		}

		//
		// ColumnReorder event
		//
		static object ColumnReorderedEvent = new object ();
		public event ColumnReorderedEventHandler ColumnReordered {
			add { Events.AddHandler (ColumnReorderedEvent, value); }
			remove { Events.RemoveHandler (ColumnReorderedEvent, value); }
		}

		protected virtual void OnColumnReordered (ColumnReorderedEventArgs e)
		{
			ColumnReorderedEventHandler creh = (ColumnReorderedEventHandler) (Events [ColumnReorderedEvent]);

			if (creh != null)
				creh (this, e);
		}

		//
		// ColumnWidthChanged
		//
		static object ColumnWidthChangedEvent = new object ();
		public event ColumnWidthChangedEventHandler ColumnWidthChanged {
			add { Events.AddHandler (ColumnWidthChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnWidthChangedEvent, value); }
		}

		protected virtual void OnColumnWidthChanged (ColumnWidthChangedEventArgs e)
		{
			ColumnWidthChangedEventHandler eh = (ColumnWidthChangedEventHandler) (Events[ColumnWidthChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
		
		void RaiseColumnWidthChanged (int resize_column)
		{
			ColumnWidthChangedEventArgs n = new ColumnWidthChangedEventArgs (resize_column);

			OnColumnWidthChanged (n);
		}
		
		//
		// ColumnWidthChanging
		//
		static object ColumnWidthChangingEvent = new object ();
		public event ColumnWidthChangingEventHandler ColumnWidthChanging {
			add { Events.AddHandler (ColumnWidthChangingEvent, value); }
			remove { Events.RemoveHandler (ColumnWidthChangingEvent, value); }
		}

		protected virtual void OnColumnWidthChanging (ColumnWidthChangingEventArgs e)
		{
			ColumnWidthChangingEventHandler cwceh = (ColumnWidthChangingEventHandler) (Events[ColumnWidthChangingEvent]);
			if (cwceh != null)
				cwceh (this, e);
		}
		
		//
		// 2.0 profile based implementation
		//
		bool CanProceedWithResize (ColumnHeader col, int width)
		{
			ColumnWidthChangingEventHandler cwceh = (ColumnWidthChangingEventHandler) (Events[ColumnWidthChangingEvent]);
			if (cwceh == null)
				return true;
			
			ColumnWidthChangingEventArgs changing = new ColumnWidthChangingEventArgs (col.Index, width);
			cwceh (this, changing);
			return !changing.Cancel;
		}
#else
		//
		// 1.0 profile based implementation
		//
		bool CanProceedWithResize (ColumnHeader col, int width)
		{
			return true;
		}

		void RaiseColumnWidthChanged (int resize_column)
		{
		}
#endif

		internal void RaiseColumnWidthChanged (ColumnHeader column)
		{
			int index = Columns.IndexOf (column);
			RaiseColumnWidthChanged (index);
		}

#if NET_2_0
		
		#region UIA Framework: Methods, Properties and Events
		
		static object UIALabelEditChangedEvent = new object ();
		static object UIAShowGroupsChangedEvent = new object ();
		static object UIAMultiSelectChangedEvent = new object ();
		static object UIAViewChangedEvent = new object ();
		static object UIACheckBoxesChangedEvent = new object ();
		static object UIAFocusedItemChangedEvent = new object ();

		internal Rectangle UIAHeaderControl {
			get { return header_control.Bounds; }
		}

		internal int UIAColumns {
			get { return cols; }
		}

		internal int UIARows {
			get { return rows; }
		}

		internal ListViewGroup UIADefaultListViewGroup 
		{
			get { return groups.DefaultGroup; }
		}

		internal ScrollBar UIAHScrollBar {
			get { return h_scroll; }
		}

		internal ScrollBar UIAVScrollBar {
			get { return v_scroll; }
		}

		internal event EventHandler UIAShowGroupsChanged {
			add { Events.AddHandler (UIAShowGroupsChangedEvent, value); }
			remove { Events.RemoveHandler (UIAShowGroupsChangedEvent, value); }
		}

		internal event EventHandler UIACheckBoxesChanged {
			add { Events.AddHandler (UIACheckBoxesChangedEvent, value); }
			remove { Events.RemoveHandler (UIACheckBoxesChangedEvent, value); }
		}

		internal event EventHandler UIAMultiSelectChanged {
			add { Events.AddHandler (UIAMultiSelectChangedEvent, value); }
			remove { Events.RemoveHandler (UIAMultiSelectChangedEvent, value); }
		}

		internal event EventHandler UIALabelEditChanged {
			add { Events.AddHandler (UIALabelEditChangedEvent, value); }
			remove { Events.RemoveHandler (UIALabelEditChangedEvent, value); }
		}

		internal event EventHandler UIAViewChanged {
			add { Events.AddHandler (UIAViewChangedEvent, value); }
			remove { Events.RemoveHandler (UIAViewChangedEvent, value); }
		}

		internal event EventHandler UIAFocusedItemChanged {
			add { Events.AddHandler (UIAFocusedItemChangedEvent, value); }
			remove { Events.RemoveHandler (UIAFocusedItemChangedEvent, value); }
		}

		internal Rectangle UIAGetHeaderBounds (ListViewGroup group)
		{
			return group.HeaderBounds;
		}

		internal int UIAItemsLocationLength
		{
			get { return items_location.Length; }
		}

		private void OnUIACheckBoxesChanged ()
		{
			EventHandler eh = (EventHandler) Events [UIACheckBoxesChangedEvent];
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		private void OnUIAShowGroupsChanged ()
		{
			EventHandler eh = (EventHandler) Events [UIAShowGroupsChangedEvent];
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		private void OnUIAMultiSelectChanged ()
		{
			EventHandler eh = (EventHandler) Events [UIAMultiSelectChangedEvent];
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		private void OnUIALabelEditChanged ()
		{
			EventHandler eh = (EventHandler) Events [UIALabelEditChangedEvent];
			if (eh != null)
				eh (this, EventArgs.Empty);
		}
		
		private void OnUIAViewChanged ()
		{
			EventHandler eh = (EventHandler) Events [UIAViewChangedEvent];
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		internal void OnUIAFocusedItemChanged ()
		{
			EventHandler eh = (EventHandler) Events [UIAFocusedItemChangedEvent];
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		#endregion // UIA Framework: Methods, Properties and Events

#endif
	}
}
