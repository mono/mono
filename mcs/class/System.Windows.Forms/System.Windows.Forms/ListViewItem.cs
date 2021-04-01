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
		private int state_image_index = -1;
		private ListViewSubItemCollection sub_items;
		private object tag;
		private bool use_item_style = true;
		int display_index = -1;			// actual position in ListView
		private ListViewGroup group = null;
		private string name = String.Empty;
		private string image_key = String.Empty;
		string tooltip_text = String.Empty;
		int indent_count;
		Point position = new Point (-1, -1);		// cached to mimic .Net behaviour	
		Rectangle bounds = Rectangle.Empty;
		Rectangle checkbox_rect;	// calculated by CalcListViewItem method
		Rectangle icon_rect;
		Rectangle item_rect;
		Rectangle label_rect;
		ListView owner;
		Font font;
		Font hot_font;			// cached font for hot tracking
		bool selected;

		internal int row;
		internal int col;

	
		#region UIA Framework: Methods, Properties and Events

		internal event EventHandler UIATextChanged;
	
		internal event LabelEditEventHandler UIASubItemTextChanged;

		internal void OnUIATextChanged ()
		{
			if (UIATextChanged != null)
				UIATextChanged (this, EventArgs.Empty);
		}

		internal void OnUIASubItemTextChanged (LabelEditEventArgs args)
		{
			//If our index is 0 we also generate TextChanged for the ListViewItem
			//because ListViewItem.Text is the same as ListViewItem.SubItems [0].Text
			if (args.Item == 0)
				OnUIATextChanged ();

			if (UIASubItemTextChanged != null)
				UIASubItemTextChanged (this, args);
		}

		#endregion // UIA Framework: Methods, Properties and Events


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
			this.sub_items = new ListViewSubItemCollection (this, null);
			for (int i = 0; i < subItems.Length; i++)
				sub_items.Add (subItems [i]);
			this.image_index = imageIndex;
		}

		public ListViewItem (string text, int imageIndex)
		{
			this.image_index = imageIndex;
			this.sub_items = new ListViewSubItemCollection (this, text);
		}

		public ListViewItem (string [] items, int imageIndex)
		{
			this.sub_items = new ListViewSubItemCollection (this, null);
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
			this.sub_items = new ListViewSubItemCollection (this, null);
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
			Group = group;
		}

		public ListViewItem(string text, ListViewGroup group) : this(text)
		{
			Group = group;
		}

		public ListViewItem(string[] items, ListViewGroup group) : this(items)
		{
			Group = group;
		}

		public ListViewItem(ListViewSubItem[] subItems, int imageIndex, ListViewGroup group)
			: this(subItems, imageIndex)
		{
			Group = group;
		}

		public ListViewItem(ListViewSubItem[] subItems, string imageKey, ListViewGroup group)
			: this(subItems, imageKey)
		{
			Group = group;
		}

		public ListViewItem(string text, int imageIndex, ListViewGroup group)
			: this(text, imageIndex)
		{
			Group = group;
		}

		public ListViewItem(string text, string imageKey, ListViewGroup group)
			: this(text, imageKey)
		{
			Group = group;
		}

		public ListViewItem(string[] items, int imageIndex, ListViewGroup group)
			: this(items, imageIndex)
		{
			Group = group;
		}

		public ListViewItem(string[] items, string imageKey, ListViewGroup group)
			: this(items, imageKey)
		{
			Group = group;
		}

		public ListViewItem(string[] items, int imageIndex, Color foreColor, Color backColor,
				Font font, ListViewGroup group)
			: this(items, imageIndex, foreColor, backColor, font)
		{
			Group = group;
		}

		public ListViewItem(string[] items, string imageKey, Color foreColor, Color backColor,
				Font font, ListViewGroup group)
			: this(items, imageKey, foreColor, backColor, font)
		{
			Group = group;
		}
		#endregion	// Public Constructors

		protected ListViewItem (SerializationInfo info, StreamingContext context)
		{
			Deserialize (info, context);
		}

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
					// consumers can update NewValue (e.g. to prevent checking an entry)
					new_value = icea.NewValue;

					if (new_value != current_value) {
						// force re-population of list
						owner.CheckedItems.Reset ();
						is_checked = new_value == CheckState.Checked;
						Invalidate ();

						ItemCheckedEventArgs args = new ItemCheckedEventArgs (this);
						owner.OnItemChecked (args);
					}
				} else
					is_checked = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool Focused {
			get { 
				if (owner == null)
					return false;

				// In virtual mode the checks are always done using indexes
				if (owner.VirtualMode)
					return Index == owner.focused_item_index;

				// Light check
				return owner.FocusedItem == this;

			}
			set { 	
				if (owner == null)
					return;

				if (Focused == value)
					return;

				ListViewItem prev_focused_item = owner.FocusedItem;
				if (prev_focused_item != null)
					prev_focused_item.UpdateFocusedState ();
					
				owner.focused_item_index = value ? Index : -1;
				if (value)
					owner.OnUIAFocusedItemChanged ();

				UpdateFocusedState ();
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
				hot_font = null;

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
		[RefreshProperties (RefreshProperties.Repaint)]
		[TypeConverter (typeof (NoneExcludedImageIndexConverter))]
		public int ImageIndex {
			get { return image_index; }
			set {
				if (value < -1)
					throw new ArgumentException ("Invalid ImageIndex. It must be greater than or equal to -1.");
				
				image_index = value;
				image_key = String.Empty;

				if (owner != null)
					Layout ();
				Invalidate ();
			}
		}

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

		[DefaultValue (0)]
		public int IndentCount {
			get {
				return indent_count;
			}
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value");

				if (value == indent_count)
					return;

				indent_count = value;
				Invalidate ();
			}
		}

		[Browsable (false)]
		public int Index {
			get {
				if (owner == null)
					return -1;
				if (owner.VirtualMode)
					return display_index;

				if (display_index == -1)
					return owner.Items.IndexOf (this);

				return owner.GetItemIndex (display_index);
			}
		}

		[Browsable (false)]
		public ListView ListView {
			get { return owner; }
		}

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

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public Point Position {
			get {
				if (owner != null && owner.VirtualMode)
					return owner.GetItemLocation (display_index);

				if (owner != null && !owner.IsHandleCreated)
					return new Point (-1, -1);

				return position;
			}
			set {
				if (owner == null || owner.View == View.Details || owner.View == View.List)
					return;

				if (owner.VirtualMode)
					throw new InvalidOperationException ();

				owner.ChangeItemLocation (display_index, value);
			}
		}

		// When ListView uses VirtualMode, selection state info
		// lives in the ListView, not in the item
		// Also, in VirtualMode we can't Reset() the selection
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool Selected {
			get { 
				if (owner != null && owner.VirtualMode)
					return owner.SelectedIndices.Contains (Index);

				return selected; 
			}
			set {
				if (selected == value && owner != null && !owner.VirtualMode)
					return;

				SetSelectedCore (value);
			}
		}

		// Expose this method as internal so we can force an update in the selection.
		internal void SetSelectedCore (bool value)
		{
			if (owner != null) {
				if (value && !owner.MultiSelect)
					owner.SelectedIndices.Clear ();
				if (owner.VirtualMode) {
					if (value)
						owner.SelectedIndices.InsertIndex (Index);
					else
						owner.SelectedIndices.RemoveIndex (Index);
				} else {
					selected = value;
					owner.SelectedIndices.Reset (); // force re-population of list
				}

				owner.OnItemSelectionChanged (new ListViewItemSelectionChangedEventArgs (this, Index, value));
				owner.OnSelectedIndexChanged ();
				Invalidate ();
			} else
				selected = value;
		}

		[DefaultValue (-1)]
		[Editor ("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design,
			 typeof (System.Drawing.Design.UITypeEditor))]
		[Localizable (true)]
		[RefreshProperties (RefreshProperties.Repaint)]
		[RelatedImageListAttribute ("ListView.StateImageList")]
		[TypeConverter (typeof (NoneExcludedImageIndexConverter))]
		public int StateImageIndex {
			get { return state_image_index; }
			set {
				if (value < -1 || value > 14)
					throw new ArgumentOutOfRangeException ("Invalid StateImageIndex. It must be in the range of [-1, 14].");

				state_image_index = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Editor ("System.Windows.Forms.Design.ListViewSubItemCollectionEditor, " + Consts.AssemblySystem_Design,
			 typeof (System.Drawing.Design.UITypeEditor))]
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

				//UIA Framework: Generates Text changed
				OnUIATextChanged ();

			}
		}

		[DefaultValue (true)]
		public bool UseItemStyleForSubItems {
			get { return use_item_style; }
			set { use_item_style = value; }
		}

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
			clone.selected = this.selected;
			clone.font = this.font;
			clone.state_image_index = this.state_image_index;
			clone.sub_items = new ListViewSubItemCollection (this, null);
			
			foreach (ListViewSubItem subItem in this.sub_items)
				clone.sub_items.Add (subItem.Text, subItem.ForeColor,
						     subItem.BackColor, subItem.Font);
			clone.tag = this.tag;
			clone.use_item_style = this.use_item_style;
			clone.owner = null;
			clone.name = name;
			clone.tooltip_text = tooltip_text;

			return clone;
		}

		public virtual void EnsureVisible ()
		{
			if (this.owner != null) {
				owner.EnsureVisible (owner.Items.IndexOf (this));
			}
		}

		public ListViewItem FindNearestItem (SearchDirectionHint searchDirection)
		{
			if (owner == null)
				return null;

			Point loc = owner.GetItemLocation (display_index);
			return owner.FindNearestItem (searchDirection, loc);
		}

		public Rectangle GetBounds (ItemBoundsPortion portion)
		{
			if (owner == null)
				return Rectangle.Empty;
				
			Rectangle rect;

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

			Point item_loc = owner.GetItemLocation (DisplayIndex);
			rect.X += item_loc.X;
			rect.Y += item_loc.Y;
			return rect;
		}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			Serialize (info, context);
		}

		public ListViewSubItem GetSubItemAt (int x, int y)
		{
			if (owner != null && owner.View != View.Details)
				return null;

			foreach (ListViewSubItem sub_item in sub_items)
				if (sub_item.Bounds.Contains (x, y))
					return sub_item;

			return null;
		}

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
			sub_items = new ListViewSubItemCollection (this, null);
			int sub_items_count = 0;

			foreach (SerializationEntry entry in info) {
				switch (entry.Name) {
					case "Text":
						sub_items.Add ((string)entry.Value);
						break;
					case "Font":
						font = (Font)entry.Value;
						break;
					case "Checked":
						is_checked = (bool)entry.Value;
						break;
					case "ImageIndex":
						image_index = (int)entry.Value;
						break;
					case "StateImageIndex":
						state_image_index = (int)entry.Value;
						break;
					case "UseItemStyleForSubItems":
						use_item_style = (bool)entry.Value;
						break;
					case "SubItemCount":
						sub_items_count = (int)entry.Value;
						break;
					case "Group":
						group = (ListViewGroup)entry.Value;
						break;
					case "ImageKey":
						if (image_index == -1)
							image_key = (string)entry.Value;
						break;
				}
			}

			Type subitem_type = typeof (ListViewSubItem);
			if (sub_items_count > 0) {
				sub_items.Clear (); // .net fixup
				Text = info.GetString ("Text");
				for (int i = 0; i < sub_items_count - 1; i++)
					sub_items.Add ((ListViewSubItem)info.GetValue ("SubItem" + (i + 1), subitem_type));
			}

			// After any sub item has been added.
			ForeColor = (Color)info.GetValue ("ForeColor", typeof (Color));
			BackColor = (Color)info.GetValue ("BackColor", typeof (Color));
		}

		protected virtual void Serialize (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("Text", Text);
			info.AddValue ("Font", Font);
			info.AddValue ("ImageIndex", image_index);
			info.AddValue ("Checked", is_checked);
			info.AddValue ("StateImageIndex", state_image_index);
			info.AddValue ("UseItemStyleForSubItems", use_item_style);
			info.AddValue ("BackColor", BackColor);
			info.AddValue ("ForeColor", ForeColor);
			info.AddValue ("ImageKey", image_key);
			if (group != null)
				info.AddValue ("Group", group);
			if (sub_items.Count > 1) {
				info.AddValue ("SubItemCount", sub_items.Count);
				for (int i = 1; i < sub_items.Count; i++) {
					info.AddValue ("SubItem" + i, sub_items [i]);
				}
			}
		}
		#endregion	// Protected Methods

		#region Private Internal Methods
		internal Rectangle CheckRectReal {
			get {
				Rectangle rect = checkbox_rect;
				Point item_loc = owner.GetItemLocation (DisplayIndex);
				rect.X += item_loc.X;
				rect.Y += item_loc.Y;
				return rect;
			}
		}
		
		Rectangle text_bounds;
		internal Rectangle TextBounds {
			get {
				// Call Layout() if it hasn't been called before.
				if (owner.VirtualMode && bounds == new Rectangle (-1, -1, -1, -1))
					Layout ();
				Rectangle result = text_bounds;
				Point loc = owner.GetItemLocation (DisplayIndex);
				result.X += loc.X;
				result.Y += loc.Y;
				return result;
			}
		}

		internal int DisplayIndex {
			get {
				// Special case for Details view
				// and no columns (which means no Layout at all)
				if (display_index == -1)
					return owner.Items.IndexOf (this);

				return display_index;
			}
			set {
				display_index = value;
			}
		}

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

		internal ListView Owner {
			set {
				if (owner == value)
					return;

				owner = value;
			}
		}

		internal void SetGroup (ListViewGroup group)
		{
			this.group = group;
		}

		internal void SetPosition (Point position)
		{
			this.position = position;
		}

		// When focus changed, we need to invalidate area
		// with previous layout and with the new one
		void UpdateFocusedState ()
		{
			if (owner != null) {
				Invalidate ();
				Layout ();
				Invalidate ();
			}
		}

		internal void Invalidate ()
		{
			if (owner == null || owner.item_control == null || owner.updating)
				return;

			// Add some padding to bounds (focused extra space, selection)
			Rectangle rect = Bounds;
			rect.Inflate (1, 1);
			owner.item_control.Invalidate (rect);
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

				int x_offset = 0;
				if (owner.SmallImageList != null)
					x_offset = indent_count * owner.SmallImageList.ImageSize.Width;

				// Handle reordered column
				if (owner.Columns.Count > 0)
					checkbox_rect.X = owner.Columns[0].Rect.X + x_offset;

				icon_rect = label_rect = Rectangle.Empty;
				icon_rect.X = checkbox_rect.Right + 2;
				item_ht = owner.ItemSize.Height;

				if (owner.SmallImageList != null)
					icon_rect.Width = owner.SmallImageList.ImageSize.Width;

				label_rect.Height = icon_rect.Height = item_ht;
				checkbox_rect.Y = item_ht - checkbox_rect.Height;

				label_rect.X = icon_rect.Width > 0 ? icon_rect.Right + 1 : icon_rect.Right;

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
			case View.Tile:
				if (!Application.VisualStylesEnabled)
					goto case View.LargeIcon;

				label_rect = icon_rect = Rectangle.Empty;

				if (owner.LargeImageList != null) {
					icon_rect.Width = owner.LargeImageList.ImageSize.Width;
					icon_rect.Height = owner.LargeImageList.ImageSize.Height;
				}

				int separation = 2;
				SizeF tsize = TextRenderer.MeasureString (Text, Font);
				int main_item_height = (int) Math.Ceiling (tsize.Height);
				int main_item_width = (int) Math.Ceiling (tsize.Width);
				sub_items [0].bounds.Height = main_item_height;

				// Set initial values for subitem's layout
				int total_height = main_item_height;
				int max_subitem_width = main_item_width;
			
				int count = Math.Min (owner.Columns.Count, sub_items.Count);
				for (int i = 1; i < count; i++) { // Ignore first column and first subitem
					ListViewSubItem sub_item = sub_items [i];
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

				max_subitem_width = Math.Min (max_subitem_width, owner.TileSize.Width - (icon_rect.Width + 4));
				label_rect.X = icon_rect.Right + 4;
				label_rect.Y = owner.TileSize.Height / 2 - total_height / 2;
				label_rect.Width = max_subitem_width;
				label_rect.Height = total_height;
			
				// Main item - always set bounds for it
				sub_items [0].SetBounds (label_rect.X, label_rect.Y, max_subitem_width, sub_items [0].bounds.Height);

				// Second pass to assign bounds for every sub item
				int current_y = sub_items [0].bounds.Bottom + separation;
				for (int j = 1; j < count; j++) {
					ListViewSubItem sub_item = sub_items [j];
					if (sub_item.Text == null || sub_item.Text.Length == 0)
						continue;

					sub_item.SetBounds (label_rect.X, current_y, max_subitem_width, sub_item.bounds.Height);
					current_y += sub_item.Bounds.Height + separation;
				}
				
				item_rect = Rectangle.Union (icon_rect, label_rect);
				bounds.Size = item_rect.Size;
				break;
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
			[NonSerialized]
			internal ListViewItem owner;
			private string text = string.Empty;
			private string name;
			private object userData;
			private SubItemStyle style;
			[NonSerialized]
			internal Rectangle bounds;

		
			#region UIA Framework: Methods, Properties and Events
		
			[field:NonSerialized]
			internal event EventHandler UIATextChanged;

			private void OnUIATextChanged ()
			{
				if (UIATextChanged != null)
					UIATextChanged (this, EventArgs.Empty);
			}

			#endregion // UIA Framework: Methods, Properties and Events

			
			#region Public Constructors
			public ListViewSubItem ()
				: this (null, string.Empty, Color.Empty,
					Color.Empty, null)
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
				this.style = new SubItemStyle (foreColor,
					backColor, font);
			}
			#endregion // Public Constructors

			#region Public Instance Properties
			public Color BackColor {
				get {
					if (style.backColor != Color.Empty)
						return style.backColor;
					if (this.owner != null && this.owner.ListView != null)
						return this.owner.ListView.BackColor;
					return ThemeEngine.Current.ColorWindow;
				}
				set { 
					style.backColor = value;
					Invalidate ();
				}
			}

			[Browsable (false)]
			public Rectangle Bounds {
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
					if (style.font != null)
						return style.font;
					else if (owner != null)
						return owner.Font;
					return ThemeEngine.Current.DefaultFont;
				}
				set {
					if (style.font == value)
						return;
					style.font = value; 
					Invalidate ();
				}
			}

			public Color ForeColor {
				get {
					if (style.foreColor != Color.Empty)
						return style.foreColor;
					if (this.owner != null && this.owner.ListView != null)
						return this.owner.ListView.ForeColor;
					return ThemeEngine.Current.ColorWindowText;
				}
				set {
					style.foreColor = value;
					Invalidate ();
				}
			}

			[Localizable (true)]
			public string Name {
				get {
					if (name == null)
						return string.Empty;
					return name;
				}
				set {
					name = value;
				}
			}

			[TypeConverter (typeof (StringConverter))]
			[BindableAttribute (true)]
			[DefaultValue (null)]
			[Localizable (false)]
			public object Tag {
				get {
					return userData;
				}
				set {
					userData = value;
				}
			}

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

					// UIA Framework: Generates SubItem TextChanged
					OnUIATextChanged ();
				    }
			}
			#endregion // Public Instance Properties

			#region Public Methods
			public void ResetStyle ()
			{
				style.Reset ();
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

			[OnDeserialized]
			void OnDeserialized (StreamingContext context)
			{
				name = null;
				userData = null;
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

			[Serializable]
			class SubItemStyle
			{
				public SubItemStyle ()
				{
				}

				public SubItemStyle (Color foreColor, Color backColor, Font font)
				{
					this.foreColor = foreColor;
					this.backColor = backColor;
					this.font = font;
				}

				public void Reset ()
				{
					foreColor = Color.Empty;
					backColor = Color.Empty;
					font = null;
				}

				public Color backColor;
				public Color foreColor;
				public Font font;
			}
		}

		public class ListViewSubItemCollection : IList, ICollection, IEnumerable
		{
			private ArrayList list;
			internal ListViewItem owner;

			#region Public Constructors
			public ListViewSubItemCollection (ListViewItem owner) : this (owner, owner.Text)
			{
 			}
			#endregion // Public Constructors

			internal ListViewSubItemCollection (ListViewItem owner, string text)
			{
				this.owner = owner;
				this.list = new ArrayList ();
				if (text != null)
					Add (text);
			}
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
					value.owner = owner;
					list [index] = value;
					owner.Layout ();
					owner.Invalidate ();
				}
			}

			public virtual ListViewSubItem this [string key] {
				get {
					int idx = IndexOfKey (key);
					if (idx == -1)
						return null;

					return (ListViewSubItem) list [idx];
				}
			}

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
				AddSubItem (item);
				owner.Layout ();
				owner.Invalidate ();
				return item;
			}

			public ListViewSubItem Add (string text)
			{
				ListViewSubItem item = new ListViewSubItem (owner, text);
				return Add (item);
			}

			public ListViewSubItem Add (string text, Color foreColor,
						    Color backColor, Font font)
			{
				ListViewSubItem item = new ListViewSubItem (owner, text,
									    foreColor, backColor, font);
				return Add (item);
			}

			public void AddRange (ListViewSubItem [] items)
			{
				if (items == null)
					throw new ArgumentNullException ("items");

				foreach (ListViewSubItem item in items) {
					if (item == null)
						continue;
					AddSubItem (item);
				}
				owner.Layout ();
				owner.Invalidate ();
			}

			public void AddRange (string [] items)
			{
				if (items == null)
					throw new ArgumentNullException ("items");

				foreach (string item in items) {
					if (item == null)
						continue;
					AddSubItem (new ListViewSubItem (owner, item));
				}
				owner.Layout ();
				owner.Invalidate ();
			}

			public void AddRange (string [] items, Color foreColor,
					      Color backColor, Font font)
			{
				if (items == null)
					throw new ArgumentNullException ("items");

				foreach (string item in items) {
					if (item == null)
						continue;

					AddSubItem (new ListViewSubItem (owner, item, foreColor, backColor, font));
				}
				owner.Layout ();
				owner.Invalidate ();
			}

			void AddSubItem (ListViewSubItem subItem)
			{
				subItem.owner = owner;
				list.Add (subItem);

				//UIA Framework
				subItem.UIATextChanged += OnUIASubItemTextChanged;
			}

			public void Clear ()
			{
				list.Clear ();
			}

			public bool Contains (ListViewSubItem subItem)
			{
				return list.Contains (subItem);
			}

			public virtual bool ContainsKey (string key)
			{
				return IndexOfKey (key) != -1;
			}

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
				//UIA Framework
				sub_item.UIATextChanged += OnUIASubItemTextChanged;
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

			public void Insert (int index, ListViewSubItem item)
			{
				item.owner = this.owner;
				list.Insert (index, item);
				owner.Layout ();
				owner.Invalidate ();

				//UIA Framework
				item.UIATextChanged += OnUIASubItemTextChanged;
			}

			public void Remove (ListViewSubItem item)
			{
				list.Remove (item);
				owner.Layout ();
				owner.Invalidate ();

				//UIA Framework
				item.UIATextChanged -= OnUIASubItemTextChanged;
			}

			public virtual void RemoveByKey (string key)
			{
				int idx = IndexOfKey (key);
				if (idx != -1)
					RemoveAt (idx);
			}

			public void RemoveAt (int index)
			{
				//UIA Framework
				if (index >= 0 && index < list.Count)
					((ListViewSubItem) list [index]).UIATextChanged -= OnUIASubItemTextChanged;

				list.RemoveAt (index);

			}
			#endregion // Public Methods
			#region UIA Event Handler
			
			private void OnUIASubItemTextChanged (object sender, EventArgs args)
			{
				owner.OnUIASubItemTextChanged (new LabelEditEventArgs (list.IndexOf (sender)));
			}

			#endregion


		}
		#endregion // Subclasses
	}
}
