//
// System.Windows.Forms.CheckedListBox.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Denis hayes (dennish@raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.Drawing;
using System.Collections;

namespace System.Windows.Forms {

	/// <summary>
	/// Displays a ListBox in which a check box is displayed to the left of each item.
	/// </summary>

	[MonoTODO]
	public class CheckedListBox : ListBox {

		// private fields
		private bool checkOnClick;
		private bool threeDCheckBoxes;
		
		
		// --- Constructor ---
		public CheckedListBox() : base() 
		{
			checkOnClick = false;
			threeDCheckBoxes = true;
		}
		
		// --- CheckedListBox Properties ---
		[MonoTODO]
		public CheckedListBox.CheckedIndexCollection CheckedIndices {
			get {
				throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public CheckedListBox.CheckedItemCollection CheckedItems {
			get {
				throw new NotImplementedException (); }
		}
		
		public bool CheckOnClick {
			get {
				return checkOnClick;
			}
			set {
				checkOnClick = value;
			}
		}
		
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);

				createParams.Caption = Text;
				createParams.ClassName = "CHECKEDLISTBOX";
				createParams.X = Left;
				createParams.Y = Top;
				createParams.Width = Width;
				createParams.Height = Height;
				createParams.ClassStyle = 0;
				createParams.ExStyle = 0;
				createParams.Param = 0;
				//			createParams.Parent = Parent.Handle;
				createParams.Style = (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE);
				window.CreateHandle (createParams);
				return createParams;
			}		
		}
		
		[MonoTODO]
		public override DrawMode DrawMode {
			get {
				//FIXME
				return base.DrawMode;
			}
			set {
				//FIXME
				base.DrawMode = value;
			}
		}
		
		[MonoTODO]
		public override int ItemHeight {
			get {
				//FIXME
				return base.ItemHeight;
			}
			set {
				//FIXME
				base.ItemHeight = value;
			}
		}
		
		[MonoTODO]
		public CheckedListBox.ObjectCollection Items {
			get {
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public new object DataSource { // .NET V1.1 Beta. needs implmented
			get { return base.DataSource; }
			set { base.DataSource = value; }
		}
		
		[MonoTODO]
		public new string DisplayMember { // .NET V1.1 Beta. needs implmented
			get { return base.DisplayMember; }
			set { base.DisplayMember = value; }
		}

		[MonoTODO]
		public new string ValueMember { // .NET V1.1 Beta. needs implmented
			get { return base.DisplayMember; }
			set { base.DisplayMember = value; }
		}

		public override SelectionMode SelectionMode {
			set {
				if (value!=SelectionMode.One && value!=SelectionMode.None)
					throw new ArgumentException();
				base.SelectionMode=value;
			}
		}
		
		public bool ThreeDCheckBoxes {
			get { return threeDCheckBoxes; }
			set { threeDCheckBoxes=value; }
		}
		
		// --- CheckedListBox methods ---
		// following methods were not stubbed out, because they only support .NET framework:
		// - protected virtual void OnItemCheck(ItemCheckEventArgs ice)
		// - protected override void WmReflectCommand(ref Message m)

		// I do not think this is part of the spec.
		//protected override AccessibleObject CreateAccessibilityInstance() 
		//{
		//	throw new NotImplementedException ();
		//}
		
//		[MonoTODO]
//		protected override ObjectCollection CreateItemCollection() 
//		{
//			throw new NotImplementedException ();
//		}
		
		[MonoTODO]
		public bool GetItemChecked(int index) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public CheckState GetItemCheckState(int index) 
		{
			throw new NotImplementedException ();
		}
		
		// [event methods]
		[MonoTODO]
		protected override void OnBackColorChanged(EventArgs e) 
		{
			//FIXME
			base.OnBackColorChanged(e);
		}
		
		[MonoTODO]
		protected override void OnClick(EventArgs e) 
		{
			//FIXME
			base.OnClick(e);
		}
		
		[MonoTODO]
		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			//FIXME
			base.OnDrawItem(e);
		}
		
		[MonoTODO]
		protected override void OnFontChanged(EventArgs e) 
		{
			//FIXME
			base.OnFontChanged(e);
		}
		
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) 
		{
			//FIXME
			base.OnHandleCreated(e);
		}
		
		// only supports .NET framework, thus is not stubbed out
		/*
		[MonoTODO]
		protected virtual void OnItemCheck(ItemCheckEventArgs ice) 
		{
			throw new NotImplementedException ();
		}
		*/
		
		[MonoTODO]
		protected override void OnKeyPress(KeyPressEventArgs e) 
		{
			//FIXME
			base.OnKeyPress(e);
		}
		
		[MonoTODO]
		protected override void OnMeasureItem(MeasureItemEventArgs e) 
		{
			//FIXME
			base.OnMeasureItem(e);
		}
		
		[MonoTODO]
		protected override void OnSelectedIndexChanged(EventArgs e) 
		{
			//FIXME
			base.OnSelectedIndexChanged(e);
		}
		// end of [event methods]
		
		[MonoTODO]
		public void SetItemChecked(int index,bool value) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SetItemCheckState(int index,CheckState value) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void WndProc(ref Message m) 
		{
			//FIXME
			base.WndProc(ref m);
		}
		
		
		
		
		/// --- CheckedListBox events ---
		/// following events are not stubbed out, because they only support .NET framework:
		/// - public new event EventHandler Click;
		/// - public new event DrawItemEventHandler DrawItem;
		/// - public new event MeasureItemEventHandler MeasureItem;
		public event ItemCheckEventHandler ItemCheck;
		
		
		
		
		/// sub-class: CheckedListBox.CheckedIndexCollection
		/// <summary>
		/// Encapsulates the collection of indexes of checked items (including items in an indeterminate state) in a CheckedListBox.
		/// </summary>
		[MonoTODO]
		public class CheckedIndexCollection : IList, ICollection, IEnumerable {
			
			
			/// --- CheckedIndexCollection Properties ---
			[MonoTODO]
			public int Count {
				get { throw new NotImplementedException (); }
			}
			
			[MonoTODO]
			public bool IsReadOnly {
				get { throw new NotImplementedException (); }
			}
			
			[MonoTODO]
			public int this[int index] {
				get { throw new NotImplementedException (); }
			}
			
			/// --- ICollection properties ---
			bool IList.IsFixedSize {
				[MonoTODO] get { throw new NotImplementedException (); }
			}
			
			object IList.this[int index] {

				[MonoTODO] get { throw new NotImplementedException (); }
				[MonoTODO] set { throw new NotImplementedException (); }
			}
	
			object ICollection.SyncRoot {

				[MonoTODO] get { throw new NotImplementedException (); }
			}
	
			bool ICollection.IsSynchronized {

				[MonoTODO] get { throw new NotImplementedException (); }
			}
			
		
			/// --- CheckedIndexCollection Methods ---
			/// Note: IList methods are stubbed out, otherwise does not IList interface cannot be implemented
			[MonoTODO]
			public bool Contains(int index) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void CopyTo(Array dest,int index) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public IEnumerator GetEnumerator() 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public int IndexOf(int index) 
			{
				throw new NotImplementedException ();
			}
			
			/// --- CheckedIndexCollection.IList methods ---
			[MonoTODO]
			int IList.Add(object value) 
			{
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			void IList.Clear() 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			bool IList.Contains(object index) 
			{
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			int IList.IndexOf(object index) 
			{
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			void IList.Insert(int index,object value) 
			{
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			void IList.Remove(object value) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			void IList.RemoveAt(int index) 
			{
				throw new NotImplementedException ();
			}
		}  // --- end of CheckedListBox.CheckedIndexCollection ---
		
		
		
		
		/// sub-class: CheckedListBox.CheckedItemCollection
		/// <summary>
		/// Encapsulates the collection of checked items (including items in an indeterminate state) in a CheckedListBox control.
		/// </summary>
		[MonoTODO]
		public class CheckedItemCollection : IList, ICollection, IEnumerable {
			
			
			/// --- CheckedItemCollection Properties ---
			[MonoTODO]
			public int Count {
				get { throw new NotImplementedException (); }
			}
			
			[MonoTODO]
			public bool IsReadOnly {
				get { throw new NotImplementedException (); }
			}
			
			[MonoTODO]
			public int this[int index] {
				get { throw new NotImplementedException (); }
				set { throw new NotImplementedException (); }
			}
			
			/// --- ICollection properties ---
			bool IList.IsFixedSize {
				[MonoTODO] get { throw new NotImplementedException (); }
			}
			
			object IList.this[int index] {

				[MonoTODO] get { throw new NotImplementedException (); }
				[MonoTODO] set { throw new NotImplementedException (); }
			}
	
			object ICollection.SyncRoot {

				[MonoTODO] get { throw new NotImplementedException (); }
			}
	
			bool ICollection.IsSynchronized {

				[MonoTODO] get { throw new NotImplementedException (); }
			}
			
			
			
			/// --- CheckedItemCollection Methods ---
			/// Note: IList methods are stubbed out, otherwise IList interface cannot be implemented
			[MonoTODO]
			public bool Contains(int item) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void CopyTo(Array dest,int index) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public IEnumerator GetEnumerator() 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public int IndexOf(int index) 
			{
				throw new NotImplementedException ();
			}
			
			/// --- CheckedItemCollection.IList methods ---
			[MonoTODO]
			int IList.Add(object value) 
			{
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			void IList.Clear() 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			bool IList.Contains(object index) 
			{
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			int IList.IndexOf(object index) 
			{
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			void IList.Insert(int index,object value) 
			{
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			void IList.Remove(object value) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			void IList.RemoveAt(int index) 
			{
				throw new NotImplementedException ();
			}
		}  // --- end of CheckedListBox.CheckedItemCollection ---
		
		/// sub-class: CheckedListBox.ObjectCollection
		/// <summary>
		/// Represents the collection of items in a CheckedListBox.
		/// </summary>
		
		[MonoTODO]
		public class ObjectCollection : ListBox.ObjectCollection {
			
			/// --- ObjectCollection.constructor ---
			[MonoTODO]
			public ObjectCollection(CheckedListBox owner) :base(owner)
			{
				
			}
			
			/// --- methods ---
			[MonoTODO]
			public int Add(object item,bool isChecked) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public int Add(object item,CheckState check) 
			{
				throw new NotImplementedException ();
			}
		}
		
	}
}
