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
//

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	[LookupBindingPropertiesAttribute ()]
	public class CheckedListBox : ListBox
	{
		private CheckedIndexCollection checked_indices;
		private CheckedItemCollection checked_items;
		private Hashtable check_states = new Hashtable ();
		private bool check_onclick = false;
		private bool three_dcheckboxes = false;
		
		public CheckedListBox ()
		{
			checked_indices = new CheckedIndexCollection (this);
			checked_items = new CheckedItemCollection (this);
			SetStyle (ControlStyles.ResizeRedraw, true);
		}

		#region events
		static object ItemCheckEvent = new object ();

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler Click {
			add { base.Click += value; }
			remove { base.Click -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DataSourceChanged {
			add { base.DataSourceChanged += value; }
			remove { base.DataSourceChanged -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DisplayMemberChanged {
			add { base.DisplayMemberChanged += value; }
			remove { base.DisplayMemberChanged -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event DrawItemEventHandler DrawItem {
			add { base.DrawItem += value; }
			remove { base.DrawItem -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event MeasureItemEventHandler MeasureItem {
			add { base.MeasureItem += value; }
			remove { base.MeasureItem -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ValueMemberChanged {
			add { base.ValueMemberChanged += value; }
			remove { base.ValueMemberChanged -= value; }
		}

		public event ItemCheckEventHandler ItemCheck {
			add { Events.AddHandler (ItemCheckEvent, value); }
			remove { Events.RemoveHandler (ItemCheckEvent, value); }
		}
		
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event MouseEventHandler MouseClick {
			add { base.MouseClick += value; }
			remove { base.MouseClick -= value; }
		}
		#endregion Events

		#region Public Properties
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public CheckedListBox.CheckedIndexCollection CheckedIndices {
			get {return checked_indices; }
		}
				
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public CheckedListBox.CheckedItemCollection CheckedItems {
			get {return checked_items; }
		}

		[DefaultValue (false)]
		public bool CheckOnClick {
			get { return check_onclick; }
			set { check_onclick = value; }
		}

		protected override CreateParams CreateParams {
			get { return base.CreateParams;}
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public new object DataSource {
			get { return base.DataSource; }
			// FIXME: docs say you can't use a DataSource with this subclass
			set { base.DataSource = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public new string DisplayMember {
			get { return base.DisplayMember; }
			set { base.DisplayMember = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override DrawMode DrawMode {
			get { return DrawMode.Normal; }
			set { /* Not an exception, but has no effect. */ }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override int ItemHeight {
			get { return base.ItemHeight; }
			set { /* Not an exception, but has no effect. */ }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Localizable (true)]
		[Editor ("System.Windows.Forms.Design.ListControlStringCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public new CheckedListBox.ObjectCollection Items {
			get { return (CheckedListBox.ObjectCollection) base.Items; }
		}

		public override SelectionMode SelectionMode {
			get { return base.SelectionMode; }
			set {
				if (!Enum.IsDefined (typeof (SelectionMode), value))
					throw new InvalidEnumArgumentException ("value", (int) value, typeof (SelectionMode));

				if (value == SelectionMode.MultiSimple || value == SelectionMode.MultiExtended)
					throw new ArgumentException ("Multi selection not supported on CheckedListBox");

				base.SelectionMode = value;
			}
		}

		[DefaultValue (false)]
		public bool ThreeDCheckBoxes {
			get { return three_dcheckboxes; }
			set {
				if (three_dcheckboxes == value)
					return;

				three_dcheckboxes = value;
				Refresh ();
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new string ValueMember {
			get { return base.ValueMember; }
			set { base.ValueMember = value; }			
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Padding Padding {
			get { return base.Padding; }
			set { base.Padding = value; }
		}
		#endregion Public Properties

		#region Public Methods

		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return base.CreateAccessibilityInstance ();
		}
		
		protected override ListBox.ObjectCollection CreateItemCollection ()
		{
			return new ObjectCollection (this);
		}

		public bool GetItemChecked (int index)
		{
			return check_states.Contains (Items [index]);
		}
		
		public CheckState GetItemCheckState (int index)
		{
			if (index < 0 || index >= Items.Count)
				throw new ArgumentOutOfRangeException ("Index of out range");

			object o = Items [index];
			if (check_states.Contains (o))
				return (CheckState) check_states [o];
			else
				return CheckState.Unchecked;
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
			if (check_states.Contains (Items [e.Index])) {
				DrawItemState state = e.State | DrawItemState.Checked;
				if (((CheckState) check_states [Items [e.Index]]) == CheckState.Indeterminate)
					state |= DrawItemState.Inactive;
				e = new DrawItemEventArgs (e.Graphics, e.Font, e.Bounds, e.Index, state, e.ForeColor, e.BackColor);
			}
			ThemeEngine.Current.DrawCheckedListBoxItem (this, e);
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
			ItemCheckEventHandler eh = (ItemCheckEventHandler)(Events [ItemCheckEvent]);
			if (eh != null)
				eh (this, ice);
		}

		protected override void OnKeyPress (KeyPressEventArgs e)
		{
			base.OnKeyPress (e);
			
			if (e.KeyChar == ' ' && FocusedItem != -1)
				SetItemChecked (FocusedItem, !GetItemChecked (FocusedItem));
		}

		protected override void OnMeasureItem (MeasureItemEventArgs e)
		{
			base.OnMeasureItem (e);
		}

		protected override void OnSelectedIndexChanged (EventArgs e)
		{
			base.OnSelectedIndexChanged (e);
		}

		protected override void RefreshItems ()
		{
			base.RefreshItems ();
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

			CheckState old_value = GetItemCheckState (index);
			
			if (old_value == value)
				return;

			ItemCheckEventArgs icea = new ItemCheckEventArgs (index, value, old_value);
    			OnItemCheck (icea);

			switch (icea.NewValue) {
			case CheckState.Checked:
			case CheckState.Indeterminate:
				check_states[Items[index]] = icea.NewValue;
    				break;
			case CheckState.Unchecked:
				check_states.Remove (Items[index]);
				break;
			default:
				break;
			}

			UpdateCollections ();

    			InvalidateCheckbox (index);
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

		int last_clicked_index = -1;

		internal override void OnItemClick (int index)
		{			
			if ((CheckOnClick || last_clicked_index == index) && index > -1) {
				if (GetItemChecked (index))
					SetItemCheckState (index, CheckState.Unchecked);
				else
					SetItemCheckState (index, CheckState.Checked);
			}
			
			last_clicked_index = index;
			base.OnItemClick (index);
		}

		internal override void CollectionChanged ()
		{
			base.CollectionChanged ();
			UpdateCollections ();
		}

		private void InvalidateCheckbox (int index)
		{
			Rectangle area = GetItemDisplayRectangle (index, TopIndex);
			area.X += 2;
			area.Y += (area.Height - 11) / 2;
			area.Width = 11;
			area.Height = 11;
			Invalidate (area);
		}

		private void UpdateCollections ()
		{
			CheckedItems.Refresh ();
			CheckedIndices.Refresh ();
		}

		#endregion Private Methods

		public new class ObjectCollection : ListBox.ObjectCollection
		{		
			private CheckedListBox owner;

			public ObjectCollection (CheckedListBox owner) : base (owner)
			{
				this.owner = owner;				
			}

			public int Add (object item, bool isChecked)
			{
				return Add (item, isChecked ? CheckState.Checked : CheckState.Unchecked);
			}
			
			public int Add (object item, CheckState check)
			{
				int idx = Add (item);

				ItemCheckEventArgs icea = new ItemCheckEventArgs (idx, check, CheckState.Unchecked);
				
				if (check == CheckState.Checked)
					owner.OnItemCheck (icea);
					
				if (icea.NewValue != CheckState.Unchecked)
					owner.check_states[item] = icea.NewValue;
					
				owner.UpdateCollections ();
				return idx;
			}

			public override void Clear ()
			{
				owner.check_states.Clear ();
				base.Clear ();
			}

			internal override void UpdateSelection (int removed_index)
			{
				owner.check_states.Remove (this[removed_index]);
				base.UpdateSelection (removed_index);
			}
		}	

		public class CheckedIndexCollection : IList, ICollection, IEnumerable
		{
			private CheckedListBox owner;
			private ArrayList indices = new ArrayList ();

			internal CheckedIndexCollection (CheckedListBox owner)
			{
				this.owner = owner;
			}

			#region Public Properties
			public int Count {
				get { return indices.Count; }
			}

			public bool IsReadOnly {
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

			[Browsable (false)]
			[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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


			public void CopyTo (Array dest, int index)
			{
				indices.CopyTo (dest, index);
			}

			public IEnumerator GetEnumerator ()
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
			internal void Refresh ()
			{
				indices.Clear ();
				for (int i = 0; i < owner.Items.Count; i++)
					if (owner.check_states.Contains (owner.Items [i]))
						indices.Add (i);
			}
			#endregion Private Methods

		}

		public class CheckedItemCollection : IList, ICollection, IEnumerable
		{
			private CheckedListBox owner;
			private ArrayList list = new ArrayList ();

			internal CheckedItemCollection (CheckedListBox owner)
			{
				this.owner = owner;
			}

			#region Public Properties
			public int Count {
				get { return list.Count; }
			}

			public bool IsReadOnly {
				get { return true; }
			}

			[Browsable (false)]
			[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
			public object this [int index] {
				get {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("Index of out range");

					return list[index];
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
			public bool Contains (object item)
			{
				return list.Contains (item);
			}

			public void CopyTo (Array dest, int index)
			{
				list.CopyTo (dest, index);
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
				return list.IndexOf (item);
			}

			public IEnumerator GetEnumerator ()
			{
				return list.GetEnumerator ();
			}

			#endregion Public Methods

			#region Private Methods
			internal void Refresh ()
			{
				list.Clear ();
				for (int i = 0; i < owner.Items.Count; i++)
					if (owner.check_states.Contains (owner.Items [i]))
						list.Add (owner.Items[i]);
			}
			#endregion Private Methods
		}
		[DefaultValue (false)]
		public bool UseCompatibleTextRendering {
			get { return use_compatible_text_rendering; }
			set { use_compatible_text_rendering = value; }
		}
	}
}

