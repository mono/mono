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
			internal int page_size;			/* Number of listbox items per page */
			internal Rectangle textdrawing_rect;	/* Displayable Client Rectangle minus the scrollbars and with IntegralHeight calculated*/
			internal bool show_verticalsb;

			public ListBoxInfo ()
			{
				item_height = 0;
				top_item = 0;
				page_size = 0;
				show_verticalsb = false;
			}
		}

		internal class ListBoxItem
		{
			internal object obj;
			internal Rectangle rect;
			internal bool Selected;

			public ListBoxItem ()
			{
				obj = null;
				Selected = true;
			}

			public ListBoxItem (object obj)
			{
				this.obj = obj;
				Selected = true;
			}
		}

		private BorderStyle border_style;
		private int column_width;
		private DrawMode draw_mode;
		private int horizontal_extent;
		private bool horizontal_scrollbar;
		private bool integral_height;
		private ObjectCollection items;
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

		private ListBoxInfo listbox_info;
		private VScrollBar vscrollbar_ctrl;

		public ListBox ()
		{
			border_style = BorderStyle.Fixed3D;
			column_width = 0;
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

			items = new ObjectCollection (this);
			selected_indices = new SelectedIndexCollection (this);
			selected_items = new SelectedObjectCollection (this);
			listbox_info = new ListBoxInfo ();
			listbox_info.item_height = FontHeight;

			vscrollbar_ctrl = new VScrollBar ();
			vscrollbar_ctrl.Minimum = 0;
			vscrollbar_ctrl.SmallChange = 1;
			vscrollbar_ctrl.LargeChange = 1;
			vscrollbar_ctrl.Maximum = 0;
			vscrollbar_ctrl.ValueChanged += new EventHandler (VerticalScrollEvent);
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

				if (column_width == value)
					return;

    				column_width = value;
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
				Refresh ();
			}
		}

		public bool ScrollAlwaysVisible {
			get { return scroll_always_visible; }
			set {
				if (scroll_always_visible == value)
					return;

    				scroll_always_visible = value;
				UpdateShowVerticalScrollBar ();
			}
		}

		public override int SelectedIndex {
			get { return selected_index;}
			set {
				if (selected_index == value)
					return;

    				selected_index = value;
				Refresh ();
			}
		}

		public SelectedIndexCollection SelectedIndices {
			get { return selected_indices; }
		}

		public object SelectedItem {
			get { return selected_item;}
			set {
				if (selected_item == value)
					return;

    				selected_item = value;
				Refresh ();
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
				Refresh ();
    			}
		}

		public override string Text {
			get {
				return "";
				//throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public int TopIndex {
			get { return top_index;}
			set { throw new NotImplementedException ();}
		}

		public bool UseTabStops {
			get { return use_tabstops; }

    			set {
				if (use_tabstops == value)
					return;

    				use_tabstops = value;
				Refresh ();
    			}
		}

		#endregion Public Properties

		#region Private Properties

		internal ListBoxInfo LBoxInfo {
			get { return listbox_info; }
		}

		#endregion Private Properties

		#region Public Methods
		protected virtual void AddItemsCore (object[] value)
		{
			items.AddRange (value);
		}


		public void BeginUpdate ()
		{

		}

		public void ClearSelected ()
		{
			throw new NotImplementedException ();
		}

		protected virtual ObjectCollection CreateItemCollection ()
		{
			return new ObjectCollection (this);
		}

		public void EndUpdate ()
		{
			throw new NotImplementedException ();
		}

		public int FindString (String s)
		{
			throw new NotImplementedException ();
		}

		public int FindString (string s,  int startIndex)
		{
			throw new NotImplementedException ();
		}

		public int FindStringExact (string s)
		{
			throw new NotImplementedException ();
		}

		public int FindStringExact(string s,  int startIndex)
		{
			throw new NotImplementedException ();
		}

		public int GetItemHeight (int index)
		{
			//if (index < 0 || index >= Count)
			//	throw new ArgumentOutOfRangeException ("Index of out range");

			return 0;
		}

		public Rectangle GetItemRectangle (int index)
		{
			throw new NotImplementedException ();
		}

		public bool GetSelected (int index)
		{
			throw new NotImplementedException ();
		}

		public int IndexFromPoint (Point p)
		{
			throw new NotImplementedException ();
		}

		public int IndexFromPoint (int x,  int y)
		{
			throw new NotImplementedException ();
		}

		protected override void OnChangeUICues(UICuesEventArgs e)
		{

		}

		protected override void OnDataSourceChanged (EventArgs e)
		{

		}

		protected override void OnDisplayMemberChanged (EventArgs e)
		{

		}

		protected virtual void OnDrawItem (DrawItemEventArgs e)
		{

		}

		protected override void OnFontChanged (EventArgs e)
		{
			listbox_info.item_height = FontHeight;
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			RellocateVerticalScroll ();
			Controls.Add (vscrollbar_ctrl);
			UpdateItemInfo ();
		}

		protected override void OnHandleDestroyed( EventArgs e)
		{

		}

		protected virtual void OnMeasureItem (MeasureItemEventArgs e)
		{

		}

		protected override void OnParentChanged ( EventArgs e)
		{

		}

		protected override void OnResize (EventArgs e)
		{
			CalcClientArea ();
			RellocateVerticalScroll ();
		}

		protected override void OnSelectedIndexChanged (EventArgs e)
		{

		}

		protected override void OnSelectedValueChanged (EventArgs e)
		{

		}

		public override void Refresh ()
		{
			base.Refresh ();
		}

		protected override void RefreshItem (int index)
		{
			throw new NotImplementedException ();
		}

		protected override void SetBoundsCore (int x,  int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x,  y, width, height, specified);
		}

		protected override void SetItemCore (int index,  object value)
		{
			throw new NotImplementedException ();
		}

		protected override void SetItemsCore (IList value)
		{
			throw new NotImplementedException ();
		}

		public void SetSelected (int index,bool value)
		{
			throw new NotImplementedException ();
		}

		protected virtual void Sort ()
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		protected virtual void WmReflectCommand (ref Message m)
		{
			throw new NotImplementedException ();
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
			listbox_info.page_size = (ClientRectangle.Height
				- (ThemeEngine.Current.DrawListBoxDecorationTop (BorderStyle) + ThemeEngine.Current.DrawListBoxDecorationBottom (BorderStyle))) / listbox_info.item_height;

			listbox_info.textdrawing_rect = ClientRectangle;
			listbox_info.textdrawing_rect.Width -= vscrollbar_ctrl.Width;
			listbox_info.textdrawing_rect.X += ThemeEngine.Current.DrawListBoxDecorationLeft (BorderStyle);
			listbox_info.textdrawing_rect.Width -=  ThemeEngine.Current.DrawListBoxDecorationRight (BorderStyle);

			/* Adjust size to visible the maxim number of displayable items */
			if (IntegralHeight == true) {
				listbox_info.textdrawing_rect.Height = (ThemeEngine.Current.DrawListBoxDecorationTop (BorderStyle) + ThemeEngine.Current.DrawListBoxDecorationBottom (BorderStyle)) + (listbox_info.page_size * listbox_info.item_height);
			}
		}

		internal void Draw ()
		{
			ThemeEngine.Current.DrawListBox (DeviceContext, ClientRectangle, this);
		}


		internal void RellocateVerticalScroll ()
		{

			vscrollbar_ctrl.Location = new Point (ClientRectangle.Width - vscrollbar_ctrl.Width
				- ThemeEngine.Current.DrawListBoxDecorationRight (BorderStyle), ThemeEngine.Current.DrawListBoxDecorationTop (BorderStyle));

			vscrollbar_ctrl.Size = new Size (vscrollbar_ctrl.Width,
				ClientRectangle.Height - (ThemeEngine.Current.DrawListBoxDecorationTop (BorderStyle) + ThemeEngine.Current.DrawListBoxDecorationBottom (BorderStyle)));

		}

		internal void UpdateShowVerticalScrollBar ()
		{
			bool show = false;

			show = (Items.Count > listbox_info.page_size || /* Items do not fit in a single page*/
				ScrollAlwaysVisible == true);

			if (listbox_info.show_verticalsb == show)
				return;

			if (Items.Count > listbox_info.page_size)
				vscrollbar_ctrl.Enabled = true;
			else
				vscrollbar_ctrl.Enabled = false;

			CalcClientArea ();
			RellocateVerticalScroll ();
		}

		// Updates the scrollbar's position with the new items and inside area
		internal void UpdateItemInfo ()
		{
			if (!IsHandleCreated)
				return;

			UpdateShowVerticalScrollBar ();

			if (Items.Count > listbox_info.page_size) {
				vscrollbar_ctrl.Maximum = Items.Count - listbox_info.page_size;
			}

			Refresh ();
		}

		// Value Changed
		private void VerticalScrollEvent (object sender, EventArgs e)
		{
			LBoxInfo.top_item = /*listbox_info.page_size + */ vscrollbar_ctrl.Value;
			Refresh ();
		}

		private void OnPaintLB (PaintEventArgs pevent)
		{
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;

			/* Copies memory drawing buffer to screen*/
			Draw ();
			pevent.Graphics.DrawImage (ImageBuffer, 0, 0);
		}

		#endregion Private Methods


		/*
			ListBox.ObjectCollection
		*/
		public class ObjectCollection : IList, ICollection, IEnumerable
		{
			private ListBox owner;
			private ArrayList object_items = new ArrayList ();
			private ArrayList listbox_items = new ArrayList ();

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
				throw new NotImplementedException ();
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
				owner.UpdateItemInfo ();
				return idx;
			}

			public void AddRange (object[] items)
			{
				foreach (object mi in items)
					AddItem (mi);

				owner.UpdateItemInfo ();
			}

			public void AddRange (ObjectCollection col)
			{
				foreach (object mi in col)
					AddItem (mi);

				owner.UpdateItemInfo ();
			}

			public virtual void Clear ()
			{
				object_items.Clear ();
				listbox_items.Clear ();
				owner.UpdateItemInfo ();
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
				return IndexOf ((MenuItem)value);
			}

			public virtual void Insert (int index,  object item)
			{
				throw new NotImplementedException ();
			}

			public virtual void Remove (object value)
			{
				RemoveAt (IndexOf (value));
				owner.UpdateItemInfo ();
			}

			public virtual void RemoveAt (int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				object_items.RemoveAt (index);
				listbox_items.RemoveAt (index);
				owner.UpdateItemInfo ();
			}
			#endregion Public Methods

			#region Private Methods
			private int AddItem (object item)
			{
				object_items.Add (item);
				listbox_items.Add (null);
				return object_items.Count - 1;
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

			#endregion Private Methods
		}

		/*
			ListBox.SelectedIndexCollection

			The idea is to get all the data for this collection from
			the ListBox SelectedObjectCollection object

		*/
		public class SelectedIndexCollection : IList, ICollection, IEnumerable
		{
			private ListBox owner;
			private ArrayList items = new ArrayList ();

			public SelectedIndexCollection (ListBox owner)
			{
				this.owner = owner;
			}

			#region Public Properties
			public virtual int Count {
				get { return owner.SelectedItems.Count; }
			}

			public virtual bool IsReadOnly {
				get { return true; }
			}

			public int this [int index] {
				get { throw new NotImplementedException (); }
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
				throw new NotImplementedException ();
			}

			public virtual void CopyTo (Array dest, int index)
			{
				throw new NotImplementedException ();
			}

			public virtual IEnumerator GetEnumerator ()
			{
				throw new NotImplementedException ();
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
				return IndexOf ((int)selectedIndex);
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
				get {throw new NotImplementedException (); }
				set {throw new NotImplementedException (); }
			}

			public int IndexOf (int selectedIndex)
			{
				throw new NotSupportedException ();
			}
			#endregion Public Methods

		}

		/*
			SelectedObjectCollection
		*/
		public class SelectedObjectCollection : IList, ICollection, IEnumerable
		{
			private ListBox owner;
			private ArrayList items = new ArrayList ();

			public SelectedObjectCollection (ListBox owner)
			{
				this.owner = owner;
			}

			#region Public Properties
			public virtual int Count {
				get { return items.Count; }
			}

			public virtual bool IsReadOnly {
				get { return true; }
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

			object IList.this[int index] {
				get { return items [index]; }
				set { items [index] = value; }
			}

			#endregion Public Properties

			#region Public Methods
			public virtual bool Contains (object selectedObject)
			{
				return items.Contains (selectedObject);
			}

			public virtual void CopyTo (Array dest, int index)
			{
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}

			public virtual IEnumerator GetEnumerator ()
			{
				throw new NotImplementedException ();
			}

			#endregion Public Methods

		}

	}
}

