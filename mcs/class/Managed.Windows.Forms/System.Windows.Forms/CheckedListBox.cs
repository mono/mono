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

	public class CheckedListBox : ListBox
	{
		private CheckedIndexCollection checked_indices;
		private CheckedItemCollection checked_items;
		private bool check_onclick;
		private bool three_dcheckboxes;
		private static readonly Rectangle checkbox_rect = new Rectangle (2, 2, 11,11); // Position of the checkbox relative to the item

		public CheckedListBox ()
		{
			items = new CheckedListBox.ObjectCollection (this);
			checked_indices = new CheckedIndexCollection (this);
			checked_items = new CheckedItemCollection (this);
			check_onclick = false;
			three_dcheckboxes = false;
			listbox_info.item_height = FontHeight + 2;
		}

		#region Events
		public new event EventHandler DataSourceChanged;
		public new event EventHandler DisplayMemberChanged;
		public new event DrawItemEventHandler DrawItem;
		public event ItemCheckEventHandler ItemCheck;
		public new event MeasureItemEventHandler MeasureItem;
		public new event EventHandler ValueMemberChanged;
		#endregion Events

		#region Public Properties
		
		public CheckedListBox.CheckedIndexCollection CheckedIndices {
			get {return checked_indices; }
		}
				
		public CheckedListBox.CheckedItemCollection CheckedItems {
			get {return checked_items; }
		}

		public bool CheckOnClick {
			get { return check_onclick; }
			set { check_onclick = value; }
		}

		protected override CreateParams CreateParams {
			get { return base.CreateParams;}
		}
		
		/*public new object DataSource {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public new string DisplayMember {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}*/

		public override DrawMode DrawMode {
			get { return DrawMode.Normal; }
			set { /* Not possible */ }
		}

		public override int ItemHeight {
			get { return listbox_info.item_height; }
			set { /* Not possible */ }
		}

		public new CheckedListBox.ObjectCollection Items {
			get { return (CheckedListBox.ObjectCollection) base.Items; }
		}

		public override SelectionMode SelectionMode {
			get { return base.SelectionMode; }
			set {
				if (value == SelectionMode.MultiSimple || value == SelectionMode.MultiExtended)
					throw new InvalidEnumArgumentException ("Multi selection modes not supported");

				base.SelectionMode = value;
			}
		}

		public bool ThreeDCheckBoxes {
			get { return three_dcheckboxes; }
			set {
				if (three_dcheckboxes == value)
					return;

				three_dcheckboxes = value;
				Refresh ();
			}
		}

		//public new string ValueMember {get; set;}
		#endregion Public Properties

		#region Public Methods

		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			throw new NotImplementedException ();
		}
		
		protected override ListBox.ObjectCollection CreateItemCollection ()
		{
			return new ObjectCollection (this);
		}

		public bool GetItemChecked (int index)
		{
			return (GetItemCheckState (index) == CheckState.Checked);
		}
		
		public CheckState GetItemCheckState (int index)
		{
			if (index < 0 || index >= Items.Count)
				throw new ArgumentOutOfRangeException ("Index of out range");

			return (Items.GetListBoxItem (index)).State;
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged (e);
		}

		protected override void OnClick (EventArgs e)
		{
			base.OnClick (e);
		}
		
		protected override void OnDrawItem (DrawItemEventArgs e)
		{
			Rectangle item_rect = e.Bounds;

			/* Draw checkbox */
			ButtonState state;

			if ((Items.GetListBoxItem (e.Index)).State == CheckState.Checked)
				state = ButtonState.Checked;
			else
				state = ButtonState.Normal;

			if (ThreeDCheckBoxes == false)
				state |= ButtonState.Flat;

			ControlPaint.DrawCheckBox (e.Graphics,
				item_rect.X + checkbox_rect.X, item_rect.Y + checkbox_rect.Y,
				checkbox_rect.Width, checkbox_rect.Height,
				state);

			item_rect.X += checkbox_rect.Width + checkbox_rect.X * 2;
			item_rect.Width -= checkbox_rect.Width + checkbox_rect.X * 2;

			/* Draw item */
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {
				e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
					(ThemeEngine.Current.ColorHilight), item_rect);

				e.Graphics.DrawString (Items[e.Index].ToString (), e.Font,
					ThemeEngine.Current.ResPool.GetSolidBrush (ThemeEngine.Current.ColorHilightText),
					item_rect, string_format);
			}
			else {
				e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
					(e.BackColor), item_rect);

				e.Graphics.DrawString (Items[e.Index].ToString (), e.Font,
					ThemeEngine.Current.ResPool.GetSolidBrush (e.ForeColor),
					item_rect, string_format);
			}
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}

		protected virtual void OnItemCheck (ItemCheckEventArgs ice)
		{
			if (ItemCheck != null)
				ItemCheck (this, ice);
		}

		protected override void OnKeyPress (KeyPressEventArgs e)
		{
			base.OnKeyPress (e);
		}

		protected override void OnMeasureItem (MeasureItemEventArgs e)
		{

		}

		protected override void OnSelectedIndexChanged (EventArgs e)
		{
			base.OnSelectedIndexChanged (e);
		}

		public void SetItemChecked (int index, bool value)
		{
			SetItemCheckState (index, value ? CheckState.Checked : CheckState.Unchecked);
		}

		public void SetItemCheckState (int index, CheckState value)
		{
			if (index < 0 || index >= Items.Count)
				throw new ArgumentOutOfRangeException ("Index of out range");

			if (!Enum.IsDefined (typeof (CheckState), value))
				throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for CheckState", value));

			CheckState old_value = (Items.GetListBoxItem (index)).State;
			
			if (old_value == value)
				return;
			
			(Items.GetListBoxItem (index)).State = value;

			Rectangle invalidate = GetItemDisplayRectangle (index, LBoxInfo.top_item);

			switch (value) {
				case CheckState.Checked:
					checked_indices.AddIndex (index);
    					checked_items.AddObject (Items[index]);
    					break;
				case CheckState.Unchecked:
					checked_indices.RemoveIndex (index);
					checked_items.RemoveObject (Items[index]);
					break;
				case CheckState.Indeterminate:
				default:
					break;
			}

    			OnItemCheck (new ItemCheckEventArgs (index, value, old_value));

    			if (ClientRectangle.Contains (invalidate))
    				Invalidate (invalidate);
		}

		protected override void WmReflectCommand (ref Message m)
		{
			base.WmReflectCommand (ref m);
		}

		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}

		#endregion Public Methods

		#region Private Methods

		internal override void OnMouseDownLB (object sender, MouseEventArgs e)
		{
			Rectangle hit_rect, item_rect;
			CheckState value =  CheckState.Checked;
			bool set_value = false;
			int index = IndexFromPointDisplayRectangle (e.X, e.Y);

			if (index == -1)
				return;
			
			/* CheckBox hit */
			hit_rect = item_rect = GetItemDisplayRectangle (index, LBoxInfo.top_item); // Full item rect
			hit_rect.X += checkbox_rect.X;
			hit_rect.Y += checkbox_rect.Y;
			hit_rect.Width = checkbox_rect.Width;
			hit_rect.Height = checkbox_rect.Height;
			
			if ((Items.GetListBoxItem (index)).State == CheckState.Checked)
					value = CheckState.Unchecked;


			if (hit_rect.Contains (e.X, e.Y) == true)  {				
				set_value = true;

			} else {
				if (item_rect.Contains (e.X, e.Y) == true) {
					if (check_onclick) {
						set_value = true;
					}
					else {
						if ((Items.GetListBoxItem (index)).Selected == true)
							set_value = true;
					}
				}
			}

			if (set_value)
				SetItemCheckState (index, value);
			
			base.OnMouseDownLB (sender, e);
		}

		internal override void UpdateItemInfo (bool adding, int first, int last)
		{
			base.UpdateItemInfo (adding, first, last);
			CheckedItems.ReCreate ();
			CheckedIndices.ReCreate ();
		}

		#endregion Private Methods

		public class ObjectCollection : ListBox.ObjectCollection
		{		
			public ObjectCollection (CheckedListBox owner) : base (owner)
			{
				
			}

			public int Add (object item,  bool isChecked)
			{
				if (isChecked)
					return Add (item, CheckState.Checked);
				
				return Add (item, CheckState.Unchecked);
					
			}
			
			public int Add (object item, CheckState check)
			{
				int cnt = object_items.Count;
				ListBox.ListBoxItem box_item = new ListBox.ListBoxItem (cnt);
				box_item.State = check;
				object_items.Add (item);
				listbox_items.Add (box_item);
				return cnt;
			}
		}

		/*
			CheckedListBox.CheckedIndexCollection
		*/
		public class CheckedIndexCollection : IList, ICollection, IEnumerable
		{
			private CheckedListBox owner;
			private ArrayList indices = new ArrayList ();

			internal CheckedIndexCollection (CheckedListBox owner)
			{
				this.owner = owner;
			}

			#region Public Properties
			public virtual int Count {
				get { return indices.Count; }
			}

			public virtual bool IsReadOnly {
				get { return true;}
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			bool IList.IsFixedSize{
				get { return true; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			public int this[int index] {
				get {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("Index of out range");

					return (int) indices[index];
				}
			}
			#endregion Public Properties

			public bool Contains (int index)
			{
				return indices.Contains (index);
			}


			public virtual void CopyTo (Array dest, int index)
			{
				indices.CopyTo (dest, index);
			}

			public virtual IEnumerator GetEnumerator ()
			{
				return indices.GetEnumerator ();
			}

			int IList.Add (object value)
			{
				throw new NotSupportedException ();
			}

			void IList.Clear ()
			{
				throw new NotSupportedException ();
			}

			bool IList.Contains (object index)
			{
				return Contains ((int)index);
			}

			int IList.IndexOf (object index)
			{
				return IndexOf ((int) index);
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

			public int IndexOf (int index)
			{
				return indices.IndexOf (index);
			}

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

					if (item.State == CheckState.Checked)
						indices.Add (item.Index);
				}
			}

			#endregion Private Methods
		}

		/*
			CheckedItemCollection
		*/
		public class CheckedItemCollection : IList, ICollection, IEnumerable
		{
			private CheckedListBox owner;
			private ArrayList object_items = new ArrayList ();

			public CheckedItemCollection (CheckedListBox owner)
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

					if (item.State == CheckState.Checked)
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

