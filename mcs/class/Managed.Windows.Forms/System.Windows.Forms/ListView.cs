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
//	Ravindra (rkumar@novell.com)
//
// $Revision: 1.2 $
// $Modtime: $
// $Log: ListView.cs,v $
// Revision 1.2  2004/10/02 11:32:01  ravindra
// Added attributes.
//
// Revision 1.1  2004/09/30 13:24:25  ravindra
// Initial implementation.
//
//
// NOT COMPLETE
//

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[DefaultEvent ("SelectedIndexChanged")]
	[DefaultProperty ("Items")]
	[Designer ("System.Windows.Forms.Design.ListViewDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	public class ListView : Control
	{
		private ItemActivation activation = ItemActivation.Standard;
		private ListViewAlignment alignment = ListViewAlignment.Top;
		private bool allowColumnReorder = false;
		private bool autoArrange = true;
		private BorderStyle borderStyle = BorderStyle.Fixed3D;
		private bool checkBoxes = false;
		private CheckedIndexCollection checkedIndices;
		private CheckedListViewItemCollection checkedItems;
		private ColumnHeaderCollection columns;
		private ListViewItem focusedItem;
		private bool fullRowSelect = false;
		private bool gridLines = false;
		private ColumnHeaderStyle headerStyle = ColumnHeaderStyle.Clickable;
		private bool hideSelection = true;
		private bool hoverSelection = false;
		private ListViewItemCollection items;
		private bool labelEdit = false;
		private bool labelWrap = true;
		internal ImageList largeImageList;
		private IComparer itemSorter;
		private bool multiselect = true;
		private bool redraw = true;
		private bool scrollable = true;
		private SelectedIndexCollection selectedIndices;
		private SelectedListViewItemCollection selectedItems;
		internal ImageList smallImageList;
		private SortOrder sortOrder = SortOrder.None;
		private ImageList stateImageList;
		private bool updating = false;
		private View view = View.LargeIcon;

		#region Events
		public event LabelEditEventHandler AfterLabelEdit;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged;

		public event LabelEditEventHandler BeforeLabelEdit;
		public event ColumnClickEventHandler ColumnClick;
		public event EventHandler ItemActivate;
		public event ItemCheckEventHandler ItemCheck;
		public event ItemDragEventHandler ItemDrag;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint;

		public event EventHandler SelectedIndexChanged;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged;
		#endregion // Events

		#region Public Constructors
		public ListView ()
		{
			checkedIndices = new CheckedIndexCollection (this);
			checkedItems = new CheckedListViewItemCollection (this);
			columns = new ColumnHeaderCollection (this);
			items = new ListViewItemCollection (this);
			selectedIndices = new SelectedIndexCollection (this);
			selectedItems = new SelectedListViewItemCollection (this);
		}
		#endregion	// Public Constructors

		#region	 Protected Properties
		protected override CreateParams CreateParams {
			get { return base.CreateParams; }
		}

		protected override Size DefaultSize {
			get { return new Size (121, 97); }
		}
		#endregion	// Protected Properties

		#region Public Instance Properties
		[DefaultValue (ItemActivation.Standard)]
		public ItemActivation Activation {
			get { return activation; }
			set { activation = value; }
		}

		[DefaultValue (ListViewAlignment.Top)]
		[Localizable (true)]
		public ListViewAlignment Alignment {
			get { return alignment; }
			set { alignment = value; }
		}

		[DefaultValue (false)]
		public bool AllowColumnReorder {
			get { return allowColumnReorder; }
			set { allowColumnReorder = value; }
		}

		[DefaultValue (true)]
		public bool AutoArrange {
			get { return autoArrange; }
			set { autoArrange = value; }
		}

		public override Color BackColor {
			get { return base.BackColor; }
			set { base.BackColor = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get { return background_image; }
			set {
				if (value == background_image)
					return;

				background_image = value;
				if (BackgroundImageChanged != null)
					BackgroundImageChanged (this, new EventArgs ());
			}
		}

		[DefaultValue (BorderStyle.Fixed3D)]
		[DispId (-504)]
		public BorderStyle BorderStyle {
			get { return borderStyle; }
			set { borderStyle = value; }
		}

		[DefaultValue (false)]
		public bool CheckBoxes {
			get { return checkBoxes; }
			set { checkBoxes = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public CheckedIndexCollection CheckedIndices {
			get { return checkedIndices; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public CheckedListViewItemCollection CheckedItems {
			get { return checkedItems; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Localizable (true)]
		[MergableProperty (false)]
		public ColumnHeaderCollection Columns {
			get { return columns; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ListViewItem FocusedItem {
			get { return focusedItem; }
		}

		public override Color ForeColor {
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}

		[DefaultValue (false)]
		public bool FullRowSelect {
			get { return fullRowSelect; }
			set { fullRowSelect = value; }
		}

		[DefaultValue (false)]
		public bool GridLines {
			get { return gridLines; }
			set { gridLines = value; }
		}

		[DefaultValue (ColumnHeaderStyle.Clickable)]
		public ColumnHeaderStyle HeaderStyle {
			get { return headerStyle; }
			set { headerStyle = value; }
		}

		[DefaultValue (true)]
		public bool HideSelection {
			get { return hideSelection; }
			set { hideSelection = value; }
		}

		[DefaultValue (false)]
		public bool HoverSelection {
			get { return hoverSelection; }
			set { hoverSelection = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Localizable (true)]
		[MergableProperty (false)]		
		public ListViewItemCollection Items {
			get { return items; }
		}

		[DefaultValue (false)]
		public bool LabelEdit {
			get { return labelEdit; }
			set { labelEdit = value; }
		}

		[DefaultValue (false)]
		[Localizable (true)]
		public bool LabelWrap {
			get { return labelWrap; }
			set { labelWrap = value; }
		}

		[DefaultValue (null)]
		public ImageList LargeImageList {
			get { return largeImageList; }
			set { largeImageList = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public IComparer ListViewItemSorter {
			get { return itemSorter; }
			set { itemSorter = value; }
		}

		[DefaultValue (true)]
		public bool MultiSelect {
			get { return multiselect; }
			set { multiselect = value; }
		}

		[DefaultValue (true)]
		public bool Scrollable {
			get { return scrollable; }
			set { scrollable = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public SelectedIndexCollection SelectedIndices {
			get { return selectedIndices; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public SelectedListViewItemCollection SelectedItems {
			get { return selectedItems; }
		}

		[DefaultValue (null)]
		public ImageList SmallImageList {
			get { return smallImageList; }
			set { smallImageList = value; }
		}

		[DefaultValue (SortOrder.None)]
		public SortOrder Sorting {
			get { return sortOrder; }
			set { sortOrder = value; }
		}

		[DefaultValue (null)]
		public ImageList StateImageList {
			get { return stateImageList; }
			set { stateImageList = value; }
		}

		[Bindable (false)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string Text {
			get { return text; } 
			set {
				if (value == text)
					return;

				text = value;
				if (TextChanged != null)
					TextChanged (this, new EventArgs ());
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ListViewItem TopItem {
			get { return items [0]; }
		}

		[DefaultValue (View.LargeIcon)]
		public View View {
			get { return view; }
			set { view = value; }
		}
		#endregion	// Public Instance Properties

		#region Internal Methods
		internal void Redraw (bool recalculate)
		{
			if (recalculate)
				CalculateListView ();

			redraw = true;
		}

		private void CalculateListView ()
		{
			// FIXME: TODO
		}
		#endregion	// Internal Methods

		#region Protected Methods
		protected override void CreateHandle ()
		{
			base.CreateHandle ();
		}

		protected override void Dispose (bool disposing)
		{
			Clear ();
		}

		protected override bool IsInputKey (Keys keyData)
		{
			return base.IsInputKey (keyData);
		}

		protected virtual void OnAfterLabelEdit (LabelEditEventArgs e)
		{
			if (AfterLabelEdit != null)
				AfterLabelEdit (this, e);
		}

		protected virtual void OnBeforeLabelEdit (LabelEditEventArgs e)
		{
			if (BeforeLabelEdit != null)
				BeforeLabelEdit (this, e);
		}

		protected virtual void OnColumnClick (ColumnClickEventArgs e)
		{
			if (ColumnClick != null)
				ColumnClick (this, e);
		}

		protected override void OnEnabledChanged (EventArgs e)
		{
			base.OnEnabledChanged (e);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected virtual void OnItemActivate (EventArgs e)
		{
			if (ItemActivate != null)
				ItemActivate (this, e);
		}

		protected virtual void OnItemCheck (ItemCheckEventArgs ice)
		{
			if (ItemCheck != null)
				ItemCheck (this, ice);
		}

		protected virtual void OnItemDrag (ItemDragEventArgs e)
		{
			if (ItemDrag != null)
				ItemDrag (this, e);
		}

		protected virtual void OnSelectedIndexChanged (EventArgs e)
		{
			if (SelectedIndexChanged != null)
				SelectedIndexChanged (this, e);
		}

		protected override void OnSystemColorsChanged (EventArgs e)
		{
			base.OnSystemColorsChanged (e);
		}

		protected void RealizeProperties ()
		{
			// FIXME: TODO
		}

		protected void UpdateExtendedStyles ()
		{
			// FIXME: TODO
		}

		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}
		#endregion // Protected Methods

		#region Public Instance Methods
		public void ArrangeIcons ()
		{
			ArrangeIcons (ListViewAlignment.Default);
		}

		public void ArrangeIcons (ListViewAlignment alignment)
		{
			// Icons are arranged only if view is set to LargeIcon or SmallIcon
			if (view == View.LargeIcon || view == View.SmallIcon) {
				// FIXME: TODO
			}
		}

		public void BeginUpdate ()
		{
			// flag to avoid painting
			updating = true;
		}

		public void Clear ()
		{
			columns.Clear ();
			items.Clear ();
		}

		public void EndUpdate ()
		{
			// flag to avoid painting
			updating = false;
		}

		public void EnsureVisible (int index)
		{
			// FIXME: TODO
		}
		
		public ListViewItem GetItemAt (int x, int y)
		{
			foreach (ListViewItem item in items) {
				if (item.Bounds.Contains (x, y))
					return item;
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
				throw new IndexOutOfRangeException ("Invalid Index");

			return items [index].GetBounds (portion);
		}

		public void Sort ()
		{
			if (sortOrder != SortOrder.None)
				items.list.Sort (itemSorter);

			if (sortOrder == SortOrder.Descending)
				items.list.Reverse ();
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
		public class CheckedIndexCollection : IList, ICollection, IEnumerable
		{
			internal ArrayList list;
			private ListView owner;

			#region Public Constructor
			public CheckedIndexCollection (ListView owner)
			{
				list = new ArrayList ();
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public virtual int Count {
				get { return list.Count; }
			}

			public virtual bool IsReadOnly {
				get { return true; }
			}

			public int this [int index] {
				get {
					if (index < 0 || index >= list.Count)
						throw new ArgumentOutOfRangeException ("Index out of range.");
					return (int) list [index];
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
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
			public bool Contains (int checkedIndex)
			{
				return list.Contains (checkedIndex);
			}

			public virtual IEnumerator GetEnumerator ()
			{
				return list.GetEnumerator ();
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
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
				return list.Contains (checkedIndex);
			}

			int IList.IndexOf (object checkedIndex)
			{
				return list.IndexOf (checkedIndex);
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
				return list.IndexOf (checkedIndex);
			}
			#endregion	// Public Methods

		}	// CheckedIndexCollection

		public class CheckedListViewItemCollection : IList, ICollection, IEnumerable
		{
			internal ArrayList list;
			private ListView owner;

			#region Public Constructor
			public CheckedListViewItemCollection (ListView owner)
			{
				list = new ArrayList ();
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public virtual int Count {
				get { return list.Count; }
			}

			public virtual bool IsReadOnly {
				get { return true; }
			}

			public ListViewItem this [int index] {
				get {
					if (index < 0 || index >= list.Count)
						throw new ArgumentOutOfRangeException ("Index out of range.");
					return (ListViewItem) list [index];
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
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
			public bool Contains (ListViewItem item)
			{
				return list.Contains (item);
			}

			public virtual void CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
			}

			public virtual IEnumerator GetEnumerator ()
			{
				return list.GetEnumerator ();
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
				return list.Contains (item);
			}

			int IList.IndexOf (object item)
			{
				return list.IndexOf (item);
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
				return list.IndexOf (item);
			}
			#endregion	// Public Methods

		}	// CheckedListViewItemCollection

		public class ColumnHeaderCollection : IList, ICollection, IEnumerable
		{
			internal ArrayList list;
			private ListView owner;

			#region Public Constructor
			public ColumnHeaderCollection (ListView owner)
			{
				list = new ArrayList ();
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public virtual int Count {
				get { return list.Count; }
			}

			public virtual bool IsReadOnly {
				get { return false; }
			}

			public virtual ColumnHeader this [int index] {
				get {
					if (index < 0 || index >= list.Count)
						throw new ArgumentOutOfRangeException ("Index out of range.");
					return (ColumnHeader) list [index];
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
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
			public virtual int Add (ColumnHeader value)
			{
				return list.Add (value);
			}

			public virtual ColumnHeader Add (string str, int width, HorizontalAlignment textAlign)
			{
				ColumnHeader colHeader = new ColumnHeader (this.owner, str, textAlign, width);
				this.Add (colHeader);

				return colHeader;
			}

			public virtual void AddRange (ColumnHeader [] values)
			{
				foreach (ColumnHeader colHeader in values)
					this.Add (colHeader);
			}

			public virtual void Clear ()
			{
				list.Clear ();
			}

			public bool Contains (ColumnHeader value)
			{
				return list.Contains (value);
			}

			public virtual IEnumerator GetEnumerator ()
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
					throw new ArgumentException("Not of type ColumnHeader", "value");
				}

				return this.Add ((ColumnHeader) value);
			}

			bool IList.Contains (object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException("Not of type ColumnHeader", "value");
				}

				return this.Contains ((ColumnHeader) value);
			}

			int IList.IndexOf (object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException("Not of type ColumnHeader", "value");
				}

				return this.IndexOf ((ColumnHeader) value);
			}

			void IList.Insert (int index, object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException("Not of type ColumnHeader", "value");
				}

				this.Insert (index, (ColumnHeader) value);
			}

			void IList.Remove (object value)
			{
				if (! (value is ColumnHeader)) {
					throw new ArgumentException("Not of type ColumnHeader", "value");
				}

				this.Remove ((ColumnHeader) value);
			}

			public int IndexOf (ColumnHeader value)
			{
				return list.IndexOf (value);
			}

			public void Insert (int index, ColumnHeader value)
			{
				if (index < 0 || index >= list.Count)
					throw new ArgumentOutOfRangeException ("Index out of range.");

				list.Insert (index, value);
			}

			public void Insert (int index, string str, int width, HorizontalAlignment textAlign)
			{
				ColumnHeader colHeader = new ColumnHeader (this.owner, str, textAlign, width);
				this.Insert (index, colHeader);
			}

			public virtual void Remove (ColumnHeader column)
			{
				list.Remove (column);
			}

			public virtual void RemoveAt (int index)
			{
				if (index < 0 || index >= list.Count)
					throw new ArgumentOutOfRangeException ("Index out of range.");

				list.RemoveAt (index);
			}
			#endregion	// Public Methods

		}	// ColumnHeaderCollection

		public class ListViewItemCollection : IList, ICollection, IEnumerable
		{
			internal ArrayList list;
			private ListView owner;

			#region Public Constructor
			public ListViewItemCollection (ListView owner)
			{
				list = new ArrayList ();
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public virtual int Count {
				get { return list.Count; }
			}

			public virtual bool IsReadOnly {
				get { return false; }
			}

			public virtual ListViewItem this [int displayIndex] {
				get {
					if (displayIndex < 0 || displayIndex >= list.Count)
						throw new ArgumentOutOfRangeException ("Index out of range.");
					return (ListViewItem) list [displayIndex];
				}

				set {
					if (displayIndex < 0 || displayIndex >= list.Count)
						throw new ArgumentOutOfRangeException ("Index out of range.");
					value.owner = this.owner;
					list [displayIndex] = value;

					owner.Redraw (true);
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
					if (value is ListViewItem)
						this [index] = (ListViewItem) value;
					else
						this [index] = new ListViewItem (value.ToString ());
				}
			}
			#endregion	// Public Properties

			#region Public Methods
			public virtual ListViewItem Add (ListViewItem value)
			{
				value.owner = this.owner;
				list.Add (value);

				if (owner.Sorting != SortOrder.None)
					owner.Sort ();

				owner.Redraw (true);

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

			public void AddRange (ListViewItem [] values)
			{
				list.Clear ();

				foreach (ListViewItem item in values) {
					item.owner = this.owner;
					list.Add (item);
				}

				if (owner.Sorting != SortOrder.None)
					owner.Sort ();

				owner.Redraw (true);
			}

			public virtual void Clear ()
			{
				list.Clear ();
				owner.Redraw (true);
			}

			public bool Contains (ListViewItem item)
			{
				return list.Contains (item);
			}

			public virtual void CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
			}

			public virtual IEnumerator GetEnumerator ()
			{
				return list.GetEnumerator ();
			}

			int IList.Add (object item)
			{
				int result;
				ListViewItem li;

				if (item is ListViewItem)
					li = (ListViewItem) item;
				else
					li = new ListViewItem (item.ToString ());

				li.owner = this.owner;
				result = list.Add (li);
				owner.Redraw (true);

				return result;
			}

			bool IList.Contains (object item)
			{
				return list.Contains (item);
			}

			int IList.IndexOf (object item)
			{
				return list.IndexOf (item);
			}

			void IList.Insert (int index, object item)
			{
				if (item is ListViewItem)
					this.Insert (index, (ListViewItem) item);
				else
					this.Insert (index, item.ToString ());
			}

			void IList.Remove (object item)
			{
				if (list.Contains (item)) {
					list.Remove (item);
					owner.Redraw (true);
				}
			}

			public int IndexOf (ListViewItem item)
			{
				return list.IndexOf (item);
			}

			public ListViewItem Insert (int index, ListViewItem item)
			{
				if (index < 0 || index >= list.Count)
					throw new ArgumentOutOfRangeException ("Index out of range.");

				item.owner = this.owner;
				list.Insert (index, item);
				owner.Redraw (true);
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

			public virtual void Remove (ListViewItem item)
			{
				if (list.Contains (item)) {
					list.Remove (item);
					owner.Redraw (true);
				}
			}

			public virtual void RemoveAt (int index)
			{
				if (index < 0 || index >= list.Count)
					throw new ArgumentOutOfRangeException ("Index out of range.");

				list.RemoveAt (index);
				owner.Redraw (false);
			}
			#endregion	// Public Methods

		}	// ListViewItemCollection

		public class SelectedIndexCollection : IList, ICollection, IEnumerable
		{
			internal ArrayList list;
			private ListView owner;

			#region Public Constructor
			public SelectedIndexCollection (ListView owner)
			{
				list = new ArrayList ();
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public virtual int Count {
				get { return list.Count; }
			}

			public virtual bool IsReadOnly {
				get { return true; }
			}

			public int this [int index] {
				get {
					if (index < 0 || index >= list.Count)
						throw new ArgumentOutOfRangeException ("Index out of range.");
					return (int) list [index];
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
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
			public bool Contains (int selectedIndex)
			{
				return list.Contains (selectedIndex);
			}

			public virtual void CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
			}

			public virtual IEnumerator GetEnumerator ()
			{
				return list.GetEnumerator ();
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ("Add operation is not supported.");
			}

			void IList.Clear ()
			{
				throw new NotSupportedException ("Clear operation is not supported.");
			}

			bool IList.Contains (object selectedIndex)
			{
				return list.Contains (selectedIndex);
			}

			int IList.IndexOf (object selectedIndex)
			{
				return list.IndexOf (selectedIndex);
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
				return list.IndexOf (selectedIndex);
			}
			#endregion	// Public Methods

		}	// SelectedIndexCollection

		public class SelectedListViewItemCollection : IList, ICollection, IEnumerable
		{
			internal ArrayList list;
			private ListView owner;

			#region Public Constructor
			public SelectedListViewItemCollection (ListView owner)
			{
				list = new ArrayList ();
				this.owner = owner;
			}
			#endregion	// Public Constructor

			#region Public Properties
			[Browsable (false)]
			public virtual int Count {
				get { return list.Count; }
			}

			public virtual bool IsReadOnly {
				get { return true; }
			}

			public ListViewItem this [int index] {
				get {
					if (index < 0 || index >= list.Count)
						throw new ArgumentOutOfRangeException ("Index out of range.");
					return (ListViewItem) list [index];
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
				set { throw new NotSupportedException ("SetItem operation is not supported."); }
			}
			#endregion	// Public Properties

			#region Public Methods
			public virtual void Clear ()
			{
				list.Clear ();
			}

			public bool Contains (ListViewItem item)
			{
				return list.Contains (item);
			}

			public virtual void CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
			}

			public virtual IEnumerator GetEnumerator ()
			{
				return list.GetEnumerator ();
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ("Add operation is not supported.");
			}

			bool IList.Contains (object item)
			{
				return list.Contains (item);
			}

			int IList.IndexOf (object item)
			{
				return list.IndexOf (item);
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
				return list.IndexOf (item);
			}
			#endregion	// Public Methods

		}	// SelectedListViewItemCollection

		#endregion // Subclasses
	}
}
