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
//
// $Revision: 1.2 $
// $Modtime: $
// $Log: ListViewItem.cs,v $
// Revision 1.2  2004/10/02 11:57:56  ravindra
// Added attributes.
//
// Revision 1.1  2004/09/30 13:24:45  ravindra
// Initial implementation.
//
//
// NOT COMPLETE
//

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
		private Color backColor;
		private Font font = ThemeEngine.Current.DefaultFont;
		private Color foreColor;
		private int imageIndex = -1;
		private bool isChecked = false;
		private bool isFocused = false;
		internal ListView owner;
		private bool selected;
		private int stateImageIndex = -1;
		private ListViewSubItemCollection subItems;
		private object tag;
		private string text;
		private bool useItemStyle = true;
		#endregion Instance Variables

		#region Public Constructors
		public ListViewItem ()
		{
			this.subItems = new ListViewSubItemCollection (this);
		}

		public ListViewItem (string text) : this (text, -1)
		{
		}

		public ListViewItem (string [] items) : this (items, -1)
		{
		}

		public ListViewItem (ListViewItem.ListViewSubItem [] subItems, int imageIndex)
		{
			this.subItems = new ListViewSubItemCollection (this);
			this.subItems.AddRange (subItems);
			this.imageIndex = imageIndex;
		}

		public ListViewItem (string text, int imageIndex)
		{
			this.text = text;
			this.imageIndex = imageIndex;
			this.subItems = new ListViewSubItemCollection (this);
		}

		public ListViewItem (string [] items, int imageIndex)
		{
			this.subItems = new ListViewSubItemCollection (this);
			this.subItems.AddRange (items);
			this.imageIndex = imageIndex;
		}

		public ListViewItem (string [] items, int imageIndex, Color foreColor, 
				     Color backColor, Font font)
		{
			this.subItems = new ListViewSubItemCollection (this);
			this.subItems.AddRange (items);
			this.imageIndex = imageIndex;
			this.foreColor = foreColor;
			this.backColor = backColor;
			this.font = font;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Color BackColor {
			get { return backColor; }
			set { this.backColor = value; }
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
			get { return isChecked; }
			set { isChecked = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool Focused {
			get { return isFocused; }
			set { isFocused = value; }
		}

		[Localizable (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Font Font {
			get { return font; }
			set { font = value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Color ForeColor {
			get { return foreColor; }
			set { foreColor = value; }
		}

		[DefaultValue (-1)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Editor ("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[Localizable (true)]
		[TypeConverter (typeof (ImageIndexConverter))]
		public int ImageIndex {
			get { return imageIndex; }
			set {
				if (value < -1)
					throw new ArgumentException ("Invalid ImageIndex. It must be greater than or equal to -1.");
				imageIndex = value;
			}
		}

		[Browsable (false)]
		public ImageList ImageList {
			get {
				if (owner == null)
					return null;
				else if (owner.View == View.LargeIcon)
					return owner.largeImageList;
				else
					return owner.smallImageList;
			}
		}

		[Browsable (false)]
		public int Index {
			get {
				if (owner == null)
					return -1;
				else
					return owner.Items.IndexOf (this);
			}
		}

		[Browsable (false)]
		public ListView ListView {
			get { return owner; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool Selected {
			get { return selected; }
			set {
				if (value != selected) {
					selected = value;
					if (owner != null && owner.MultiSelect) {
						if (selected)
							//do we need !owner.SelectedItems.Contains (this))
							owner.SelectedItems.list.Add (this);
						else
							owner.SelectedItems.list.Remove (this);
					}
				}
			}
		}

		[DefaultValue (-1)]
		[Editor ("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[Localizable (true)]
		[TypeConverter (typeof (ImageIndexConverter))]
		public int StateImageIndex {
			get { return stateImageIndex; }
			set {
				if (value < -1 || value > 14)
					throw new ArgumentOutOfRangeException ("Invalid StateImageIndex. It must be in the range of [-1, 14].");

				stateImageIndex = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ListViewSubItemCollection SubItems {
			get { return subItems; }
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
			get { return text; }
			set { text = value; }
		}

		[DefaultValue (true)]
		public bool UseItemStyleForSubItems {
			get { return useItemStyle; }
			set { useItemStyle = value; }
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public void BeginEdit ()
		{
			// FIXME: TODO
			// if (owner != null && owner.LabelEdit)
			// allow editing
			// else
			// throw new InvalidOperationException ();
		}

		public virtual object Clone ()
		{
			// FIXME: TODO
			return new ListViewItem ();
		}

		public virtual void EnsureVisible ()
		{
			// FIXME: TODO
		}

		public Rectangle GetBounds (ItemBoundsPortion portion)
		{
			// FIXME: TODO
			return new Rectangle (0, 0, 0, 0);
		}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			// FIXME: TODO
		}

		public virtual void Remove ()
		{
			if (owner != null)
				owner.Items.Remove (this);
			owner = null;
		}

		public override string ToString ()
		{
			return string.Format ("ListViewItem: {{0}}", text);
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


		#region Subclasses

		[DefaultProperty ("Text")]
		[DesignTimeVisible (false)]
		[Serializable]
		[ToolboxItem (false)]
		//[TypeConverter (typeof (ListViewSubItemConverter))]
		public class ListViewSubItem
		{
			private Color backColor;
			private Font font;
			private Color foreColor;
			internal ListViewItem owner;
			private string text;
			
			#region Public Constructors
			public ListViewSubItem ()
			{
			}

			public ListViewSubItem (ListViewItem owner, string text)
			{
				this.owner = owner;
				this.text = text;
			}

			public ListViewSubItem (ListViewItem owner, string text, Color foreColor, Color backColor, Font font)
			{
				this.owner = owner;
				this.text = text;
				this.foreColor = foreColor;
				this.backColor = backColor;
				this.font = font;
			}
			#endregion // Public Constructors

			#region Public Instance Properties
			public Color BackColor {
				get { return backColor; }
				set { backColor = value; }
			}

			[Localizable (true)]
			public Font Font {
				get { return font; }
				set { font = value; }
			}

			public Color ForeColor {
				get { return foreColor; }
				set { foreColor = value; }
			}

			[Localizable (true)]
			public string Text {
				get { return text; }
				set { text = value; }
			}
			#endregion // Public Instance Properties

			#region Public Methods
			public void ResetStyle ()
			{
				font = ThemeEngine.Current.DefaultFont;
				backColor = ThemeEngine.Current.DefaultControlBackColor;
				foreColor = ThemeEngine.Current.DefaultControlForeColor;
			}

			public override string ToString ()
			{
				return string.Format ("ListViewSubItem {{0}}", text);
			}
			#endregion // Public Methods
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
			public virtual int Count {
				get { return list.Count; }
			}

			public virtual bool IsReadOnly {
				get { return false; }
			}

			public ListViewSubItem this [int index] {
				get { return (ListViewSubItem) list [index]; }
				set { 
					value.owner = this.owner;
					list [index] = value;
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
						throw new ArgumentException("Not of type ListViewSubItem", "value");
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

			public ListViewSubItem Add (string text, Color foreColor, Color backColor, Font font)
			{
				ListViewSubItem item = new ListViewSubItem (this.owner, text, foreColor, backColor, font);
				list.Add (item);
				return item;
			}

			public void AddRange (ListViewSubItem [] items)
			{
				list.Clear ();
				foreach (ListViewSubItem item in items)
					this.Add (item);
			}

			public void AddRange (string [] items)
			{
				list.Clear ();
				foreach (string item in items)
					this.Add (item);
			}

			public void AddRange (string [] items, Color foreColor, Color backColor, Font font)
			{
				list.Clear ();
				foreach (string item in items)
					this.Add (item, foreColor, backColor, font);
			}

			public virtual void Clear ()
			{
				list.Clear ();
			}

			public bool Contains (ListViewSubItem item)
			{
				return list.Contains (item);
			}

			public virtual IEnumerator GetEnumerator ()
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
					throw new ArgumentException("Not of type ListViewSubItem", "item");
				}

				ListViewSubItem subItem = (ListViewSubItem) item;
				subItem.owner = this.owner;
				return list.Add (subItem);
			}

			bool IList.Contains (object subItem)
			{
				if (! (subItem is ListViewSubItem)) {
					throw new ArgumentException("Not of type ListViewSubItem", "subItem");
				}

				return this.Contains ((ListViewSubItem) subItem);
			}

			int IList.IndexOf (object subItem)
			{
				if (! (subItem is ListViewSubItem)) {
					throw new ArgumentException("Not of type ListViewSubItem", "subItem");
				}

				return this.IndexOf ((ListViewSubItem) subItem);
			}

			void IList.Insert (int index, object item)
			{
				if (! (item is ListViewSubItem)) {
					throw new ArgumentException("Not of type ListViewSubItem", "item");
				}

				this.Insert (index, (ListViewSubItem) item);
			}

			void IList.Remove (object item)
			{
				if (! (item is ListViewSubItem)) {
					throw new ArgumentException("Not of type ListViewSubItem", "item");
				}

				this.Remove ((ListViewSubItem) item);
			}

			public int IndexOf (ListViewSubItem subItem)
			{
				return list.IndexOf (subItem);
			}

			public void Insert (int index, ListViewSubItem item)
			{
				item.owner = this.owner;
				list.Insert (index, item);
			}

			public void Remove (ListViewSubItem item)
			{
				list.Remove (item);
			}

			public virtual void RemoveAt (int index)
			{
				list.RemoveAt (index);
			}
			#endregion // Public Methods
		}
		#endregion // Subclasses
	}
}
