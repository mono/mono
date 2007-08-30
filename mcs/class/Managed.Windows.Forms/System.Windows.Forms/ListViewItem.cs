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
// Copyright (c) 2004 Novell, Inc. (http://www.novell.com)
//
// Author:
//      Ravindra (rkumar@novell.com)
//      Mike Kestner <mkestner@novell.com>
//      Daniel Nauck (dna(at)mono-project(dot)de)

using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Serialization;

namespace System.Windows.Forms
{
	[DefaultProperty ("Text")]
	[DesignTimeVisible (false)]
	[Serializable]
	[ToolboxItem (false)]
	[TypeConverter (typeof (ListViewItemConverter))]
	public class ListViewItem : ICloneable, ISerializable
	{
		#region Instance Variables
		private int image_index = -1;
		private bool is_checked = false;
		private bool is_focused = false;
		private int state_image_index = -1;
		private ListViewSubItemCollection sub_items;
		private object tag;
		private bool use_item_style = true;
#if NET_2_0
		private ListViewGroup group = null;
		private string name = String.Empty;
		private string image_key = String.Empty;
		string tooltip_text = String.Empty;
		int index;			// cached index for VirtualMode
#endif
		Rectangle bounds;
		Rectangle checkbox_rect;	// calculated by CalcListViewItem method
		Rectangle icon_rect;
		Rectangle item_rect;
		Rectangle label_rect;
		ListView owner;
		Font font;
#if NET_2_0
		Font hot_font;			// cached font for hot tracking
#endif
		bool selected;

		internal int row;
		internal int col;

		#endregion Instance Variables

		#region Public Constructors
		public ListViewItem () : this (string.Empty)
		{
		}

		public ListViewItem (string text) : this (text, -1)
		{
		}

		public ListViewItem (string [] items) : this (items, -1)
		{
		}

		public ListViewItem (ListViewItem.ListViewSubItem [] subItems, int imageIndex)
		{
			this.sub_items = new ListViewSubItemCollection (this);
			for (int i = 0; i < subItems.Length; i++)
				sub_items.Add (subItems [i]);
			this.image_index = imageIndex;
		}

		public ListViewItem (string text, int imageIndex)
		{
			this.image_index = imageIndex;
			this.sub_items = new ListViewSubItemCollection (this);
			this.sub_items.Add (text);
		}

		public ListViewItem (string [] items, int imageIndex)
		{
			this.sub_items = new ListViewSubItemCollection (this);
			if (items != null) {
				for (int i = 0; i < items.Length; i++)
					sub_items.Add (new ListViewSubItem (this, items [i]));
			}
			this.image_index = imageIndex;
		}

		public ListViewItem (string [] items, int imageIndex, Color foreColor, 
				     Color backColor, Font font) : this (items, imageIndex)
		{
			ForeColor = foreColor;
			BackColor = backColor;
			this.font = font;
		}

#if NET_2_0
		public ListViewItem(string[] items, string imageKey) : this(items)
		{
			this.ImageKey = imageKey;
		}

		public ListViewItem(string text, string imageKey) : this(text)
		{
			this.ImageKey = imageKey;
		}

		public ListViewItem(ListViewSubItem[] subItems, string imageKey)
		{
			this.sub_items = new ListViewSubItemCollection (this);
			for (int i = 0; i < subItems.Length; i++)
				this.sub_items.Add (subItems [i]);
			this.ImageKey = imageKey;
		}

		public ListViewItem(string[] items, string imageKey, Color foreColor,
					Color backColor, Font font) : this(items, imageKey)
		{
			ForeColor = foreColor;
			BackColor = backColor;
			this.font = font;
		}

		public ListViewItem(ListViewGroup group) : this()
		{
			this.group = group;
		}

		public ListViewItem(string text, ListViewGroup group) : this(text)
		{
			this.group = group;
		}

		public ListViewItem(string[] items, ListViewGroup group) : this(items)
		{
			this.group = group;
		}

		public ListViewItem(ListViewSubItem[] subItems, int imageIndex, ListViewGroup group)
			: this(subItems, imageIndex)
		{
			this.group = group;
		}

		public ListViewItem(ListViewSubItem[] subItems, string imageKey, ListViewGroup group)
			: this(subItems, imageKey)
		{
			this.group = group;
		}

		public ListViewItem(string text, int imageIndex, ListViewGroup group)
			: this(text, imageIndex)
		{
			this.group = group;
		}

		public ListViewItem(string text, string imageKey, ListViewGroup group)
			: this(text, imageKey)
		{
			this.group = group;
		}

		public ListViewItem(string[] items, int imageIndex, ListViewGroup group)
			: this(items, imageIndex)
		{
			this.group = group;
		}

		public ListViewItem(string[] items, string imageKey, ListViewGroup group)
			: this(items, imageKey)
		{
			this.group = group;
		}

		public ListViewItem(string[] items, int imageIndex, Color foreColor, Color backColor,
				Font font, ListViewGroup group)
			: this(items, imageIndex, foreColor, backColor, font)
		{
			this.group = group;
		}

		public ListViewItem(string[] items, string imageKey, Color foreColor, Color backColor,
				Font font, ListViewGroup group)
			: this(items, imageKey, foreColor, backColor, font)
		{
			this.group = group;
		}
#endif
		#endregion	// Public Constructors

		#region Public Instance Properties
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Color BackColor {
			get {
				if (sub_items.Count > 0)
					return sub_items[0].BackColor;

				if (owner != null)
					return owner.BackColor;
				
				return ThemeEngine.Current.ColorWindow;
			}
			set { SubItems [0].BackColor = value; }
		}

		[Browsable (false)]
		public Rectangle Bounds {
			get {
				return GetBounds (ItemBoundsPortion.Entire);
			}
		}

		[DefaultValue (false)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public bool Checked {
			get { return is_checked; }
			set { 
				if (is_checked == value)
					return;
				
				if (owner != null) {
					CheckState current_value = is_checked ? CheckState.Checked : CheckState.Unchecked;
					CheckState new_value = value ? CheckState.Checked : CheckState.Unchecked;

					ItemCheckEventArgs icea = new ItemCheckEventArgs (Index,
							new_value, current_value);
					owner.OnItemCheck (icea);

					if (new_value != current_value) {
						// force re-population of list
						owner.CheckedItems.Reset ();
						is_checked = new_value == CheckState.Checked;
						Invalidate ();

#if NET_2_0
						ItemCheckedEventArgs args = new ItemCheckedEventArgs (this);
						owner.OnItemChecked (args);
#endif
					}
				} else
					is_checked = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool Focused {
			get { 
#if NET_2_0
				// As well as selection state in VirtualMode,
				// focus state is stored in the ListView
				if (owner != null && owner.VirtualMode)
					return Index == owner.focused_item_index;
#endif
				return is_focused; 
			}
			set { 	
				if (is_focused == value)
					return;

				if (owner != null) {
					if (owner.FocusedItem != null)
						owner.FocusedItem.UpdateFocusedState (false);

					owner.focused_item_index = value ? Index : -1;
				}

				UpdateFocusedState (value);
			}
		}

		[Localizable (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Font Font {
			get {
				if (font != null)
					return font;
				else if (owner != null)
					return owner.Font;

				return ThemeEngine.Current.DefaultFont;
			}
			set { 	
				if (font == value)
					return;

				font = value; 
#if NET_2_0
				hot_font = null;
#endif

				if (owner != null)
					Layout ();
				Invalidate ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Color ForeColor {
			get {
				if (sub_items.Count > 0)
					return sub_items[0].ForeColor;

				if (owner != null)
					return owner.ForeColor;

				return ThemeEngine.Current.ColorWindowText;
			}
			set { SubItems [0].ForeColor = value; }
		}

		[DefaultValue (-1)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Editor ("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design,
			 typeof (System.Drawing.Design.UITypeEditor))]
		[Localizable (true)]
#if NET_2_0
		[RefreshProperties (RefreshProperties.Repaint)]
		[TypeConverter (typeof (NoneExcludedImageIndexConverter))]
#else
		[TypeConverter (typeof (ImageIndexConverter))]
#endif
		public int ImageIndex {
			get { return image_index; }
			set {
				if (value < -1)
					throw new ArgumentException ("Invalid ImageIndex. It must be greater than or equal to -1.");
				
				image_index = value;
#if NET_2_0
				image_key = String.Empty;
#endif

				if (owner != null)
					Layout ();
				Invalidate ();
			}
		}

#if NET_2_0
		[DefaultValue ("")]
		[LocalizableAttribute (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Editor ("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design,
			 typeof (System.Drawing.Design.UITypeEditor))]
		[RefreshProperties (RefreshProperties.Repaint)]
		[TypeConverter (typeof (ImageKeyConverter))]
		public string ImageKey {
			get {
				return image_key;
			}
			set {
				image_key = value == null ? String.Empty : value;
				image_index = -1;

				if (owner != null)
					Layout ();
				Invalidate ();
			}
		}
#endif

		[Browsable (false)]
		public ImageList ImageList {
			get {
				if (owner == null)
					return null;
				else if (owner.View == View.LargeIcon)
					return owner.large_image_list;
				else
					return owner.small_image_list;
			}
		}

		[Browsable (false)]
		public int Index {
			get {
				if (owner == null)
					return -1;
#if NET_2_0
				if (owner.VirtualMode)
					return index;
#endif
				else
					return owner.Items.IndexOf (this);
			}
		}

		[Browsable (false)]
		public ListView ListView {
			get { return owner; }
		}

#if NET_2_0
		[Browsable (false)]
		[Localizable (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Name {
			get {
				return name;
			}
			set {
				name = value == null ? String.Empty : value;
			}
		}
#endif

		// When ListView uses VirtualMode, selection state info
		// lives in the ListView, not in the item
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool Selected {
			get { 
#if NET_2_0
				if (owner != null && owner.VirtualMode)
					return owner.SelectedIndices.Contains (Index);
#endif

				return selected; 
			}
			set {
#if NET_2_0
				if (selected == value && owner != null && !owner.VirtualMode)
#else
				if (selected == value)
#endif
					return;

				if (owner != null) {
					if (value && !owner.MultiSelect)
						owner.SelectedIndices.Clear ();
#if NET_2_0
					if (owner.VirtualMode)
						if (value)
							owner.SelectedIndices.InsertIndex (Index);
						else
							owner.SelectedIndices.RemoveIndex (Index);
					else
#endif
						selected = value;
						
					// force re-population of list
					owner.SelectedIndices.Reset ();
#if NET_2_0
					owner.OnItemSelectionChanged (new ListViewItemSelectionChangedEventArgs (this, Index, value));
#endif
					owner.OnSelectedIndexChanged ();
				} else {
					selected = value;
				}
				Invalidate ();
			}
		}

		[DefaultValue (-1)]
		[Editor ("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design,
			 typeof (System.Drawing.Design.UITypeEditor))]
		[Localizable (true)]
#if NET_2_0
		[RefreshProperties (RefreshProperties.Repaint)]
		[RelatedImageListAttribute ("ListView.StateImageList")]
		[TypeConverter (typeof (NoneExcludedImageIndexConverter))]
#else
		[TypeConverter (typeof (ImageIndexConverter))]
#endif
		public int StateImageIndex {
			get { return state_image_index; }
			set {
				if (value < -1 || value > 14)
					throw new ArgumentOutOfRangeException ("Invalid StateImageIndex. It must be in the range of [-1, 14].");

				state_image_index = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#if NET_2_0
		[Editor ("System.Windows.Forms.Design.ListViewSubItemCollectionEditor, " + Consts.AssemblySystem_Design,
			 typeof (System.Drawing.Design.UITypeEditor))]
#endif
		public ListViewSubItemCollection SubItems {
			get {
				if (sub_items.Count == 0)
					this.sub_items.Add (string.Empty);
				return sub_items;
			}
		}

		[Bindable (true)]
		[DefaultValue (null)]
		[Localizable (false)]
		[TypeConverter (typeof (StringConverter))]
		public object Tag {
			get { return tag; }
			set { tag = value; }
		}

		[Localizable (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Text {
			get {
				if (this.sub_items.Count > 0)
					return this.sub_items [0].Text;
				else
					return string.Empty;
			}
			set { 
				if (SubItems [0].Text == value)
					return;

				sub_items [0].Text = value; 

				if (owner != null)
					Layout ();
				Invalidate ();
			}
		}

		[DefaultValue (true)]
		public bool UseItemStyleForSubItems {
			get { return use_item_style; }
			set { use_item_style = value; }
		}

#if NET_2_0
		[LocalizableAttribute(true)]
		[DefaultValue (null)]
		public ListViewGroup Group {
			get { return this.group; }
			set {
				if (group != value) {
					if (value == null)
						group.Items.Remove (this);
					else
						value.Items.Add (this);
				
					group = value;
				}
			}
		}

		[DefaultValue ("")]
		public string ToolTipText {
			get {
				return tooltip_text;
			}
			set {
				if (value == null)
					value = String.Empty;

				tooltip_text = value;
			}
		}
#endif

		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public void BeginEdit ()
		{
			if (owner != null && owner.LabelEdit) {
				owner.item_control.BeginEdit (this);
			}
			// FIXME: TODO
			// if (owner != null && owner.LabelEdit 
			//    && owner.Activation == ItemActivation.Standard)
			// allow editing
			// else
			// throw new InvalidOperationException ();
		}

		public virtual object Clone ()
		{
			ListViewItem clone = new ListViewItem ();
			clone.image_index = this.image_index;
			clone.is_checked = this.is_checked;
			clone.is_focused = this.is_focused;
			clone.selected = this.selected;
			clone.font = this.font;
			clone.state_image_index = this.state_image_index;
			clone.sub_items = new ListViewSubItemCollection (this);
			
			foreach (ListViewSubItem subItem in this.sub_items)
				clone.sub_items.Add (subItem.Text, subItem.ForeColor,
						     subItem.BackColor, subItem.Font);
			clone.tag = this.tag;
			clone.use_item_style = this.use_item_style;
			clone.owner = null;
#if NET_2_0
			clone.name = name;
			clone.tooltip_text = tooltip_text;
#endif

			return clone;
		}

		public virtual void EnsureVisible ()
		{
			if (this.owner != null) {
				owner.EnsureVisible (owner.Items.IndexOf (this));
			}
		}

		public Rectangle GetBounds (ItemBoundsPortion portion)
		{
			if (owner == null)
				return Rectangle.Empty;
				
			Rectangle rect;

#if NET_2_0
			// Can't cache bounds in Virtual mode,
			// since we can get different item instances at each invocation
			if (owner.VirtualMode)
				Layout ();
#endif
			switch (portion) {
			case ItemBoundsPortion.Icon:
				rect = icon_rect;
				break;

			case ItemBoundsPortion.Label:
				rect = label_rect;
				break;

			case ItemBoundsPortion.ItemOnly:
				rect = item_rect;
				break;

			case ItemBoundsPortion.Entire:
				rect = bounds;
				break;

			default:
				throw new ArgumentException ("Invalid value for portion.");
			}

			Point item_loc = owner.GetItemLocation (Index);
			rect.X += item_loc.X;
			rect.Y += item_loc.Y;
			return rect;
		}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			// FIXME: TODO
		}

#if NET_2_0
		public ListViewSubItem GetSubItemAt (int x, int y)
		{
			if (owner != null && owner.View != View.Details)
				return null;

			foreach (ListViewSubItem sub_item in sub_items)
				if (sub_item.Bounds.Contains (x, y))
					return sub_item;

			return null;
		}
#endif

		public virtual void Remove ()
		{
			if (owner == null)
				return;

			owner.item_control.CancelEdit (this);
			owner.Items.Remove (this);
			owner = null;
		}

		public override string ToString ()
		{
			return string.Format ("ListViewItem: {0}", this.Text);
		}
		#endregion	// Public Instance Methods

		#region Protected Methods
		protected virtual void Deserialize (SerializationInfo info, StreamingContext context)
		{
			// FIXME: TODO
		}

		protected virtual void Serialize (SerializationInfo info, StreamingContext context)
		{
			// FIXME: TODO
		}
		#endregion	// Protected Methods

		#region Private Internal Methods
		internal Rectangle CheckRectReal {
			get {
				Rectangle rect = checkbox_rect;
				Point item_loc = owner.GetItemLocation (Index);
				rect.X += item_loc.X;
				rect.Y += item_loc.Y;
				return rect;
			}
		}
		
		Rectangle text_bounds;
		internal Rectangle TextBounds {
			get {
				Rectangle result = text_bounds;
				Point loc = owner.GetItemLocation (Index);
				result.X += loc.X;
				result.Y += loc.Y;
				return result;
			}
		}

#if NET_2_0
		internal bool Hot {
			get {
				return Index == owner.HotItemIndex;
			}
		}

		internal Font HotFont {
			get {
				if (hot_font == null)
					hot_font = new Font (Font, Font.Style | FontStyle.Underline);

				return hot_font;
			}
		}
#endif

		internal ListView Owner {
			set {
				if (owner == value)
					return;

				owner = value;
			}
		}

#if NET_2_0
		internal void SetIndex (int index)
		{
			this.index = index;
		}

		internal void SetGroup (ListViewGroup group)
		{
			this.group = group;
		}
#endif

		void UpdateFocusedState (bool is_focused)
		{
			this.is_focused = is_focused;
			if (owner != null) {
				Invalidate ();
				Layout ();
				Invalidate ();
			}
		}

		internal void Invalidate ()
		{
			if (owner == null || owner.item_control == null)
				return;

			owner.item_control.Invalidate (Bounds);
		}

		internal void Layout ()
		{
			if (owner == null)
				return;
			int item_ht;
			Rectangle total;
			Size text_size = owner.text_size;
			
			checkbox_rect = Rectangle.Empty;
			if (owner.CheckBoxes)
				checkbox_rect.Size = owner.CheckBoxSize;
			switch (owner.View) {
			case View.Details:
				// LAMESPEC: MSDN says, "In all views except the details
				// view of the ListView, this value specifies the same
				// bounding rectangle as the Entire value." Actually, it
				// returns same bounding rectangles for Item and Entire
				// values in the case of Details view.

				// Handle reordered column
				if (owner.Columns.Count > 0)
					checkbox_rect.X = owner.Columns[0].Rect.X;

				icon_rect = label_rect = Rectangle.Empty;
				icon_rect.X = checkbox_rect.Right + 2;
				item_ht = owner.ItemSize.Height;

				if (owner.SmallImageList != null)
					icon_rect.Width = owner.SmallImageList.ImageSize.Width;

				label_rect.Height = icon_rect.Height = item_ht;
				checkbox_rect.Y = item_ht - checkbox_rect.Height;

				label_rect.X = icon_rect.Right + 1;

				if (owner.Columns.Count > 0)
					label_rect.Width = owner.Columns[0].Wd - label_rect.X + checkbox_rect.X;
				else
					label_rect.Width = text_size.Width;

				SizeF text_sz = TextRenderer.MeasureString (Text, Font);
				text_bounds = label_rect;
				text_bounds.Width = (int) text_sz.Width;

				item_rect = total = Rectangle.Union
					(Rectangle.Union (checkbox_rect, icon_rect), label_rect);
				bounds.Size = total.Size;

				item_rect.Width = 0;
				bounds.Width = 0;
				for (int i = 0; i < owner.Columns.Count; i++) {
					item_rect.Width += owner.Columns [i].Wd;
					bounds.Width += owner.Columns [i].Wd;
				}

				// Bounds for sub items
				int n = Math.Min (owner.Columns.Count, sub_items.Count);
				for (int i = 0; i < n; i++) {
					Rectangle col_rect = owner.Columns [i].Rect;
					sub_items [i].SetBounds (col_rect.X, 0, col_rect.Width, item_ht);
				}
				break;

			case View.LargeIcon:
				label_rect = icon_rect = Rectangle.Empty;

				SizeF sz = TextRenderer.MeasureString (Text, Font);
				if ((int) sz.Width > text_size.Width) {
					if (Focused && owner.InternalContainsFocus) {
						int text_width = text_size.Width;
						StringFormat format = new StringFormat ();
						format.Alignment = StringAlignment.Center;
						sz = TextRenderer.MeasureString (Text, Font, text_width, format);
						text_size.Height = (int) sz.Height;
					} else
						text_size.Height = 2 * (int) sz.Height;
				}

				if (owner.LargeImageList != null) {
					icon_rect.Width = owner.LargeImageList.ImageSize.Width;
					icon_rect.Height = owner.LargeImageList.ImageSize.Height;
				}

				if (checkbox_rect.Height > icon_rect.Height)
					icon_rect.Y = checkbox_rect.Height - icon_rect.Height;
				else
					checkbox_rect.Y = icon_rect.Height - checkbox_rect.Height;

				if (text_size.Width <= icon_rect.Width) {
			 		icon_rect.X = checkbox_rect.Width + 1;
					label_rect.X = icon_rect.X + (icon_rect.Width - text_size.Width) / 2;
					label_rect.Y = icon_rect.Bottom + 2;
					label_rect.Size = text_size;
				} else {
					int centerX = text_size.Width / 2;
					icon_rect.X = checkbox_rect.Width + 1 + centerX - icon_rect.Width / 2;
					label_rect.X = checkbox_rect.Width + 1;
					label_rect.Y = icon_rect.Bottom + 2;
					label_rect.Size = text_size;
				}

				item_rect = Rectangle.Union (icon_rect, label_rect);
				total = Rectangle.Union (item_rect, checkbox_rect);
				bounds.Size = total.Size;
				break;

			case View.List:
			case View.SmallIcon:
				label_rect = icon_rect = Rectangle.Empty;
				icon_rect.X = checkbox_rect.Width + 1;
				item_ht = Math.Max (owner.CheckBoxSize.Height, text_size.Height);

				if (owner.SmallImageList != null) {
					item_ht = Math.Max (item_ht, owner.SmallImageList.ImageSize.Height);
					icon_rect.Width = owner.SmallImageList.ImageSize.Width;
					icon_rect.Height = owner.SmallImageList.ImageSize.Height;
				}

				checkbox_rect.Y = item_ht - checkbox_rect.Height;
				label_rect.X = icon_rect.Right + 1;
				label_rect.Width = text_size.Width;
				label_rect.Height = icon_rect.Height = item_ht;

				item_rect = Rectangle.Union (icon_rect, label_rect);
				total = Rectangle.Union (item_rect, checkbox_rect);
				bounds.Size = total.Size;
				break;
#if NET_2_0
			case View.Tile:
				label_rect = icon_rect = Rectangle.Empty;

				if (owner.LargeImageList != null) {
					icon_rect.Width = owner.LargeImageList.ImageSize.Width;
					icon_rect.Height = owner.LargeImageList.ImageSize.Height;
				}

				int separation = 2;
				SizeF tsize = TextRenderer.MeasureString (Text, Font);

				// Set initial values for subitem's layout
				int total_height = (int)Math.Ceiling (tsize.Height);
				int max_subitem_width = (int)Math.Ceiling (tsize.Width);
				SubItems [0].bounds.Height = total_height;
			
				int count = Math.Min (owner.Columns.Count, SubItems.Count);
				for (int i = 1; i < count; i++) { // Ignore first column and first subitem
					ListViewSubItem sub_item = SubItems [i];
					if (sub_item.Text == null || sub_item.Text.Length == 0)
						continue;

					tsize = TextRenderer.MeasureString (sub_item.Text, sub_item.Font);
				
					int width = (int)Math.Ceiling (tsize.Width);
				
					if (width > max_subitem_width)
						max_subitem_width = width;
				
					int height = (int)Math.Ceiling (tsize.Height);
					total_height += height + separation;
				
					sub_item.bounds.Height = height;
			
				}

				label_rect.X = icon_rect.Right + 4;
				label_rect.Y = owner.TileSize.Height / 2 - total_height / 2;
				label_rect.Width = max_subitem_width;
				label_rect.Height = total_height;
			
				// Second pass for assigning bounds. This time take first subitem into account.
				int current_y = label_rect.Y;
				for (int j = 0; j < count; j++) {
					ListViewSubItem sub_item = SubItems [j];
					if (sub_item.Text == null || sub_item.Text.Length == 0)
						continue;

					sub_item.SetBounds (label_rect.X, current_y, max_subitem_width, sub_item.bounds.Height);
					current_y += sub_item.Bounds.Height + separation;
				}
				
				item_rect = Rectangle.Union (icon_rect, label_rect);
				bounds.Size = item_rect.Size;
				break;
#endif
			}
			
		}
		#endregion	// Private Internal Methods

		#region Subclasses

		[DefaultProperty ("Text")]
		[DesignTimeVisible (false)]
		[Serializable]
		[ToolboxItem (false)]
		[TypeConverter (typeof(ListViewSubItemConverter))]
		public class ListViewSubItem
		{
			private Color back_color;
			private Font font;
			private Color fore_color;
			internal ListViewItem owner;
			private string text = string.Empty;
#if NET_2_0
			private string name = String.Empty;
			private object tag;
#endif
			internal Rectangle bounds;
			
			#region Public Constructors
			public ListViewSubItem ()
			{
			}

			public ListViewSubItem (ListViewItem owner, string text)
				: this (owner, text, Color.Empty,
					Color.Empty, null)
			{
			}

			public ListViewSubItem (ListViewItem owner, string text, Color foreColor,
						Color backColor, Font font)
			{
				this.owner = owner;
				Text = text;
				this.fore_color = foreColor;
				this.back_color = backColor;
				this.font = font;
			}
			#endregion // Public Constructors

			#region Public Instance Properties
			public Color BackColor {
				get {
					if (this.back_color != Color.Empty)
						return this.back_color;
					if (this.owner != null && this.owner.ListView != null)
						return this.owner.ListView.BackColor;
					return ThemeEngine.Current.ColorWindow;
				}
				set { 
					back_color = value; 
					Invalidate ();
				}
			}

#if NET_2_0
			[Browsable (false)]
			public 
#else
				
			internal
#endif
			Rectangle Bounds {
				get {
					Rectangle retval = bounds;
					if (owner != null) {
						retval.X += owner.Bounds.X;
						retval.Y += owner.Bounds.Y;
					}

					return retval;
				}
			}

			[Localizable (true)]
			public Font Font {
				get {
					if (font != null)
						return font;
					else if (owner != null)
						return owner.Font;
					return ThemeEngine.Current.DefaultFont;
				}
				set { 
					if (font == value)
						return;
					font = value; 
					Invalidate ();
				    }
			}

			public Color ForeColor {
				get {
					if (this.fore_color != Color.Empty)
						return this.fore_color;
					if (this.owner != null && this.owner.ListView != null)
						return this.owner.ListView.ForeColor;
					return ThemeEngine.Current.ColorWindowText;
				}
				set { 
					fore_color = value; 
					Invalidate ();
				}
			}

#if NET_2_0
			[Localizable (true)]
			public string Name {
				get {
					return name;
				}
				set {
					name = value == null ? String.Empty : value;
				}
			}

			[TypeConverter (typeof (StringConverter))]
			[BindableAttribute (true)]
			[DefaultValue (null)]
			[Localizable (false)]
			public object Tag {
				get {
					return tag;
				}
				set {
					tag = value;
				}
			}
#endif

			[Localizable (true)]
			public string Text {
				get { return text; }
				set { 
					if(text == value)
						return;

					if(value == null)
						text = string.Empty;
					else
					      	text = value; 

					Invalidate ();
				    }
			}
			#endregion // Public Instance Properties

			#region Public Methods
			public void ResetStyle ()
			{
				font = ThemeEngine.Current.DefaultFont;
				back_color = ThemeEngine.Current.DefaultControlBackColor;
				fore_color = ThemeEngine.Current.DefaultControlForeColor;
				Invalidate ();
			}

			public override string ToString ()
			{
				return string.Format ("ListViewSubItem {{0}}", text);
			}
			#endregion // Public Methods

			
			#region Private Methods
			private void Invalidate ()
			{
				if (owner == null || owner.owner == null)
					return;

				owner.Invalidate ();
			}

			internal int Height {
				get {
					return bounds.Height;
				}
			}

			internal void SetBounds (int x, int y, int width, int height)
			{
				bounds = new Rectangle (x, y, width, height);
			}
			#endregion // Private Methods
		}

		public class ListViewSubItemCollection : IList, ICollection, IEnumerable
		{
			private ArrayList list;
			internal ListViewItem owner;

			#region Public Constructors
			public ListViewSubItemCollection (ListViewItem owner)
			{
				this.owner = owner;
				this.list = new ArrayList ();
 			}
			#endregion // Public Constructors

			#region Public Properties
			[Browsable (false)]
			public int Count {
				get { return list.Count; }
			}

			public bool IsReadOnly {
				get { return false; }
			}

			public ListViewSubItem this [int index] {
				get { return (ListViewSubItem) list [index]; }
				set { 
					value.owner = this.owner;
					list [index] = value;
				}
			}

#if NET_2_0
			public virtual ListViewSubItem this [string key] {
				get {
					int idx = IndexOfKey (key);
					if (idx == -1)
						return null;

					return (ListViewSubItem) list [idx];
				}
			}
#endif

			bool ICollection.IsSynchronized {
				get { return list.IsSynchronized; }
			}

			object ICollection.SyncRoot {
				get { return list.SyncRoot; }
			}

			bool IList.IsFixedSize {
				get { return list.IsFixedSize; }
			}

			object IList.this [int index] {
				get { return this [index]; }
				set {
					if (! (value is ListViewSubItem))
						throw new ArgumentException ("Not of type ListViewSubItem", "value");
					this [index] = (ListViewSubItem) value;
				}
			}
			#endregion // Public Properties

			#region Public Methods
			public ListViewSubItem Add (ListViewSubItem item)
			{
				item.owner = this.owner;
				list.Add (item);
				return item;
			}

			public ListViewSubItem Add (string text)
			{
				ListViewSubItem item = new ListViewSubItem (this.owner, text);
				list.Add (item);
				return item;
			}

			public ListViewSubItem Add (string text, Color foreColor,
						    Color backColor, Font font)
			{
				ListViewSubItem item = new ListViewSubItem (this.owner, text,
									    foreColor, backColor, font);
				list.Add (item);
				return item;
			}

			public void AddRange (ListViewSubItem [] items)
			{
				if (items == null)
					throw new ArgumentNullException ("items");

				foreach (ListViewSubItem item in items) {
					if (item == null)
						continue;
					this.Add (item);
				}
			}

			public void AddRange (string [] items)
			{
				if (items == null)
					throw new ArgumentNullException ("items");

				foreach (string item in items) {
					if (item == null)
						continue;
					this.Add (item);
				}
			}

			public void AddRange (string [] items, Color foreColor,
					      Color backColor, Font font)
			{
				if (items == null)
					throw new ArgumentNullException ("items");

				foreach (string item in items) {
					if (item == null)
						continue;
					this.Add (item, foreColor, backColor, font);
				}
			}

			public void Clear ()
			{
				list.Clear ();
			}

			public bool Contains (ListViewSubItem item)
			{
				return list.Contains (item);
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

			int IList.Add (object item)
			{
				if (! (item is ListViewSubItem)) {
					throw new ArgumentException ("Not of type ListViewSubItem", "item");
				}

				ListViewSubItem sub_item = (ListViewSubItem) item;
				sub_item.owner = this.owner;
				return list.Add (sub_item);
			}

			bool IList.Contains (object subItem)
			{
				if (! (subItem is ListViewSubItem)) {
					throw new ArgumentException ("Not of type ListViewSubItem", "subItem");
				}

				return this.Contains ((ListViewSubItem) subItem);
			}

			int IList.IndexOf (object subItem)
			{
				if (! (subItem is ListViewSubItem)) {
					throw new ArgumentException ("Not of type ListViewSubItem", "subItem");
				}

				return this.IndexOf ((ListViewSubItem) subItem);
			}

			void IList.Insert (int index, object item)
			{
				if (! (item is ListViewSubItem)) {
					throw new ArgumentException ("Not of type ListViewSubItem", "item");
				}

				this.Insert (index, (ListViewSubItem) item);
			}

			void IList.Remove (object item)
			{
				if (! (item is ListViewSubItem)) {
					throw new ArgumentException ("Not of type ListViewSubItem", "item");
				}

				this.Remove ((ListViewSubItem) item);
			}

			public int IndexOf (ListViewSubItem subItem)
			{
				return list.IndexOf (subItem);
			}

#if NET_2_0
			public virtual int IndexOfKey (string key)
			{
				if (key == null || key.Length == 0)
					return -1;

				for (int i = 0; i < list.Count; i++) {
					ListViewSubItem l = (ListViewSubItem) list [i];
					if (String.Compare (l.Name, key, true) == 0)
						return i;
				}

				return -1;
			}
#endif

			public void Insert (int index, ListViewSubItem item)
			{
				item.owner = this.owner;
				list.Insert (index, item);
			}

			public void Remove (ListViewSubItem item)
			{
				list.Remove (item);
			}

#if NET_2_0
			public virtual void RemoveByKey (string key)
			{
				int idx = IndexOfKey (key);
				if (idx != -1)
					RemoveAt (idx);
			}
#endif

			public void RemoveAt (int index)
			{
				list.RemoveAt (index);
			}
			#endregion // Public Methods
		}
		#endregion // Subclasses
	}
}
