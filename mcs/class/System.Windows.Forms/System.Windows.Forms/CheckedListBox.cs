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
		private CheckedListBox.CheckedIndexCollection CheckedIndices_;
		
		internal ArrayList checkedIndexCollection_ = new ArrayList();
		
		// --- Constructor ---
		public CheckedListBox() : base() 
		{
			checkOnClick = false;
			threeDCheckBoxes = true;
			Items_ = new CheckedListBox.ObjectCollection(this);
			CheckedIndices_ = new CheckedListBox.CheckedIndexCollection(this);
			DrawMode_ = DrawMode.Normal;
		}
		
		// --- CheckedListBox Properties ---
		[MonoTODO]
		public CheckedListBox.CheckedIndexCollection CheckedIndices {
			get {
				return CheckedIndices_; 
			}
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
				if( Parent != null) {
					CreateParams createParams = base.CreateParams;
					// set ownerDraw flag to be able to paint check-boxes
					createParams.Style |= (int)ListBoxStyles.LBS_OWNERDRAWFIXED;
					return createParams;
				}
				return null;
			}		
		}
		
		[MonoTODO]
		public override DrawMode DrawMode {
			get {
				return DrawMode.Normal;
			}
			set {
				// always DrawMode.Normal
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
				return (CheckedListBox.ObjectCollection)Items_; 
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
			set { 
				if( threeDCheckBoxes != value) {
					threeDCheckBoxes = value; 
					Invalidate();
				}
			}
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
			return CheckedIndices.Contains(index);
		}
		
		[MonoTODO]
		public CheckState GetItemCheckState(int index) 
		{
			return CheckedIndices.Contains(index) ? CheckState.Checked : CheckState.Unchecked;
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
			Rectangle checkRect = new Rectangle( e.Bounds.Left, e.Bounds.Top, e.Bounds.Height, e.Bounds.Height);
			checkRect.Inflate(-1,-1);
			Rectangle textRect = new Rectangle( checkRect.Right, e.Bounds.Top, e.Bounds.Width - checkRect.Width - 1, e.Bounds.Height);
			
			if( (e.State & DrawItemState.Selected) != 0) {
				e.Graphics.FillRectangle(SystemBrushes.Highlight, textRect);
				e.Graphics.DrawString(Items_[e.Index].ToString(), Font, SystemBrushes.HighlightText, textRect.X, textRect.Y);
			}
			else {
				e.Graphics.FillRectangle(SystemBrushes.Window, textRect);
				e.Graphics.DrawString(Items_[e.Index].ToString(), Font, SystemBrushes.ControlText, textRect.X, textRect.Y);
			}
		
			ButtonState state = ButtonState.Normal;
			if( !threeDCheckBoxes) {
				state |= ButtonState.Flat;
			}
			
			if( CheckedIndices.Contains(e.Index)) {
				state |= ButtonState.Checked;
			}
			
			ControlPaint.DrawCheckBox(e.Graphics, checkRect, state);
			
			if( 0 != (DrawItemState.Focus & e.State)) {
				ControlPaint.DrawFocusRectangle(e.Graphics, textRect);
			}
			
			//base.OnDrawItem(e);
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
			SetItemCheckState(index, value ? CheckState.Checked : CheckState.Unchecked);
		}
		
		[MonoTODO]
		public void SetItemCheckState(int index, CheckState value) 
		{
			if( index < 0 || index > Items.Count) {
				// FIXME: Set exception properties
				throw new ArgumentException();
			}

			bool invalidateControl = false;
			
			if( value == CheckState.Checked) {
				if(!checkedIndexCollection_.Contains(index)) {
					checkedIndexCollection_.Add(index);
					invalidateControl = true;
				}
			}
			else if( value == CheckState.Unchecked) {
				if(checkedIndexCollection_.Contains(index)) {
					checkedIndexCollection_.Remove(index);
					invalidateControl = true;
				}
			}
			else {
			}
			if( invalidateControl) {
				// FIXME: Minimize repainting here, invalidate only part on the control ?
				Invalidate();
			}
		}

		internal void listboxSelChange()
		{
			int curSel = Win32.SendMessage(Handle, (int)ListBoxMessages.LB_GETCURSEL, 0, 0);
			//Console.WriteLine("ListBoxNotifications.LBN_SELCHANGE. {0} item is active", curSel);
			// CHECKME: the things work nice w/out call to control, but may be this will be needed.
			//CallControlWndProc(ref m);
			if(checkOnClick || prevSelectedIndex == curSel) {
				SelectedIndex = curSel;
				SetItemChecked(SelectedIndex, !CheckedIndices.Contains(SelectedIndex));
			}
			prevSelectedIndex = curSel;
		}
		
		[MonoTODO]
		protected override void WndProc(ref Message m) 
		{
			//FIXME
			switch (m.Msg) {
				case Msg.WM_COMMAND: 
					switch(m.HiWordWParam) {
						case (uint)ListBoxNotifications.LBN_SELCHANGE:
							listboxSelChange();
							m.Result = IntPtr.Zero;
							break;
						case (uint)ListBoxNotifications.LBN_DBLCLK:
							listboxSelChange();
							m.Result = IntPtr.Zero;
							break;
						default:
							base.WndProc(ref m);
							break;
					}
					break;
				default:
					base.WndProc(ref m);
					break;
			}
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
			CheckedListBox 		owner_;
			
			internal CheckedIndexCollection(CheckedListBox owner)
			{
				owner_ = owner;
			}
			
			/// --- CheckedIndexCollection Properties ---
			[MonoTODO]
			public int Count {
				get { return owner_.checkedIndexCollection_.Count; }
			}
			
			[MonoTODO]
			public bool IsReadOnly {
				get { return owner_.checkedIndexCollection_.IsReadOnly; }
			}
			
			[MonoTODO]
			public int this[int index] {
				get { return (int)owner_.checkedIndexCollection_[index]; }
			}
			
			/// --- ICollection properties ---
			bool IList.IsFixedSize {
				[MonoTODO] get { throw new NotImplementedException(); }
			}
			
			object IList.this[int index] {

				[MonoTODO] get { throw new NotImplementedException(); }
				[MonoTODO] set { ; }
			}
	
			object ICollection.SyncRoot {

				[MonoTODO] get { throw new NotImplementedException(); }
			}
	
			bool ICollection.IsSynchronized {

				[MonoTODO] get { throw new NotImplementedException(); }
			}
			
		
			/// --- CheckedIndexCollection Methods ---
			/// Note: IList methods are stubbed out, otherwise does not IList interface cannot be implemented
			[MonoTODO]
			public bool Contains(int index) 
			{
				return owner_.checkedIndexCollection_.Contains(index);
			}
			
			[MonoTODO]
			public void CopyTo(Array dest,int index) 
			{
				owner_.checkedIndexCollection_.CopyTo(dest, index);
			}
			
			[MonoTODO]
			public IEnumerator GetEnumerator() 
			{
				return owner_.checkedIndexCollection_.GetEnumerator();
			}
			
			[MonoTODO]
			public int IndexOf(int index) 
			{
				return owner_.checkedIndexCollection_.IndexOf(index);
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
			
			internal override void OnItemRemovedAt( int index) {
				if(((CheckedListBox)owner_).checkedIndexCollection_.Contains(index)) {
					((CheckedListBox)owner_).checkedIndexCollection_.Remove(index);
				}
			}
		}
		
	}
}
