//		
//			System.Windows.Forms.CheckedListBox
//
//			Author: 
//				Alberto Fernandez	(infjaf00@yahoo.es)
//
//


using System;
using System.Collections;


namespace System.Windows.Forms
{
	public class CheckedListBox : ListBox
	{
		public CheckedListBox()
		{
			throw new NotImplementedException ();
		}
		
		public CheckedListBox.CheckedIndexCollection CheckedIndices {
			get {throw new NotImplementedException(); }
		}
		public CheckedListBox.CheckedItemCollection CheckedItems {
			get {throw new NotImplementedException(); }
		}
		public bool CheckOnClick {
			get {throw new NotImplementedException(); }
			set {throw new NotImplementedException(); }
		}
		protected override CreateParams CreateParams {
			get {throw new NotImplementedException(); }
		}
		public new object DataSource {
			get {throw new NotImplementedException(); }
			set {throw new NotImplementedException(); }
		}
		public new string DisplayMember {
			get {throw new NotImplementedException(); }
			set {throw new NotImplementedException(); }
		}
		public override DrawMode DrawMode {
			get {throw new NotImplementedException(); }
			set {throw new NotImplementedException(); }
		}
		public override int ItemHeight {
			get {throw new NotImplementedException(); }
			set {throw new NotImplementedException(); }
		}
		public new CheckedListBox.ObjectCollection Items {
			get {throw new NotImplementedException(); }
		}
		public override SelectionMode SelectionMode {
			get {throw new NotImplementedException(); }
			set {throw new NotImplementedException(); }
		}
		public bool ThreeDCheckBoxes {
			get {throw new NotImplementedException(); }
			set {throw new NotImplementedException(); }
		}
		public new string ValueMember {
			get {throw new NotImplementedException(); }
			set {throw new NotImplementedException(); }
		}
		protected override AccessibleObject CreateAccessibilityInstance()
		{
			throw new NotImplementedException();
		}
		
		protected override ListBox.ObjectCollection CreateItemCollection()
		{
			throw new NotImplementedException();
		}

		public bool GetItemChecked(int index)
		{
			throw new NotImplementedException();
		}
		public CheckState GetItemCheckState(int index)
		{
			throw new NotImplementedException();
		}
		protected override void OnBackColorChanged(EventArgs e)
		{
			throw new NotImplementedException();
		}
		protected override void OnClick(EventArgs e)
		{
			throw new NotImplementedException();
		}
		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			throw new NotImplementedException();
		}
		protected override void OnFontChanged(EventArgs e)
		{
			throw new NotImplementedException();
		}
		protected override void OnHandleCreated(EventArgs e)
		{
			throw new NotImplementedException();
		}
		protected virtual void OnItemCheck(ItemCheckEventArgs ice)
		{
			throw new NotImplementedException();
		}
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			throw new NotImplementedException();
		}
		protected override void OnMeasureItem(MeasureItemEventArgs e)
		{
			throw new NotImplementedException();
		}
		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			throw new NotImplementedException();
		}
		public void SetItemChecked(int index,bool value)
		{
			throw new NotImplementedException();
		}
		public void SetItemCheckState(int index, CheckState value)
		{
			throw new NotImplementedException();
		}
		protected override void WmReflectCommand(ref Message m)
		{
			throw new NotImplementedException();
		}
		protected override void WndProc(ref Message m)
		{
			throw new NotImplementedException();
		}


		public new event EventHandler Click;
		public new event EventHandler DataSourceChanged;
		public new event EventHandler DisplayMemberChanged;
		public new event DrawItemEventHandler DrawItem;
		public event ItemCheckEventHandler ItemCheck;
		public new event MeasureItemEventHandler MeasureItem;
		public new event EventHandler ValueMemberChanged;

		public class CheckedIndexCollection : IList,  ICollection, IEnumerable
		{
		
			public bool IsFixedSize {
				get { return false; }
			}
		
			public bool IsSynchronized {
				get { throw new NotImplementedException (); }
			}
			public object SyncRoot {
				get { throw new NotImplementedException (); }
			}

			public virtual int Count {
				get { throw new NotImplementedException (); }
			}
			public virtual bool IsReadOnly {
				get { throw new NotImplementedException (); }
			}
			public object this[int index] {
				get { throw new NotImplementedException (); }
				set { throw new NotImplementedException (); }
			}
			public bool Contains(int index)
			{
			throw new NotImplementedException();
			}
			public virtual void CopyTo(Array dest,   int index)
			{
				throw new NotImplementedException();
			}
			public virtual IEnumerator GetEnumerator()
			{
				throw new NotImplementedException();
			}
			int IList.Add(object value)
			{
				throw new NotImplementedException();
			}
			void IList.Clear()
			{
				throw new NotImplementedException();
			}
			bool IList.Contains(object index)
			{
				throw new NotImplementedException();
			}
			int IList.IndexOf(object index)
			{
				throw new NotImplementedException();
			}
			void IList.Insert(int index,object value)
			{
				throw new NotImplementedException();
			}
			void IList.Remove(object value)
			{
				throw new NotImplementedException();
			}
			void IList.RemoveAt(int index)
			{
				throw new NotImplementedException();
			}
			public int IndexOf(int index)
			{
				throw new NotImplementedException();
			}
			

		}
		public class CheckedItemCollection : IList, ICollection, IEnumerable
		{
		
			public bool IsFixedSize {
				get { return false; }
			}
		
			public bool IsSynchronized {
				get { throw new NotImplementedException (); }
			}
			public object SyncRoot {
				get { throw new NotImplementedException (); }
			}

			public virtual int Count {
				get { throw new NotImplementedException (); }
			}
			public virtual bool IsReadOnly {
				get { throw new NotImplementedException (); }
			}
			public virtual object this[int index] {
				get { throw new NotImplementedException (); }
				set { throw new NotImplementedException (); }
			}
			public virtual bool Contains(object item)
			{
				throw new NotImplementedException();
			}
			public virtual void CopyTo(Array dest, int index)
			{
				throw new NotImplementedException();
			}
			public virtual IEnumerator GetEnumerator()
			{
				throw new NotImplementedException();
			}
			int IList.Add(object value)
			{
				throw new NotImplementedException();
			}
			void IList.Clear()
			{
				throw new NotImplementedException();
			}
			void IList.Insert(int index, object value)
			{
				throw new NotImplementedException();
			}
			void IList.Remove(object value)
			{
				throw new NotImplementedException();
			}
			void IList.RemoveAt(int index)
			{
				throw new NotImplementedException();
			}
			public virtual int IndexOf(object item)
			{
				throw new NotImplementedException();
			}
		}
		public new class ObjectCollection :  ListBox.ObjectCollection
		{
		
			public bool IsFixedSize {
				get { return false; }
			}
		
			public bool IsSynchronized {
				get { throw new NotImplementedException (); }
			}
			public object SyncRoot {
				get { throw new NotImplementedException (); }
			}
			
			public ObjectCollection(CheckedListBox owner) : base (owner)
			{
				throw new NotImplementedException();
			}
			public int Add(object item, bool isChecked)
			{
				throw new NotImplementedException();
			}
			public int Add(object item, CheckState check)
			{
				throw new NotImplementedException();
			}
		}
	}
	
}
