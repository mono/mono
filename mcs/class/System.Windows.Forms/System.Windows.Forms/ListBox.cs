//
// System.Windows.Forms.ListBox.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//
// (C) 2002 Ximian, Inc
//
using System.Collections;
using System.Drawing;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

    public class ListBox : ListControl {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public ListBox()
		{
			throw new NotImplementedException ();
		}

		//
		//	 --- Public Fields
		//
		public const int DefaultItemHeight = 500;//just guessing FIXME // = ??;
		public const int NoMatches = 0 ;//just guessing FIXME // = ??;

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public override Color BackColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Image BackgroundImage {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override RightToLeft RightToLeft {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool ScrollAlwaysVisible {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override int SelectedIndex {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public ListBox.SelectedIndexCollection SelectedIndices {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public object SelectedItem {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public ListBox.SelectedObjectCollection SelectedItems {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public virtual SelectionMode SelectionMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Sorted {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override string Text {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int TopIndex {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool UseTabStops {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public void BeginUpdate()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void ClearSelected()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Dispose()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void EndUpdate()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int FindString(string str)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int FindString(string str, int val)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int FindStringExact(string str)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int FindStringExact(string str, int val)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int GetItemHeight(int index)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Rectangle GetItemRectangle(int index)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public bool GetSelected(int index)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int IndexFromPoint(Point pt)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int IndexFromPoint(int val1, int val2)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Invalidate()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Invalidate(bool val)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Invalidate(Rectangle rect)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Invalidate(Region reg)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Invalidate(Region reg, bool val)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public object Invoke(Delegate del)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public object Invoke(Delegate del, object[] objs)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void PerformLayout()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void PerformLayout(Control ctl, string str )
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void ResumeLayout()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void ResumeLayout(bool val)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Scale(float val)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Scale(float val1, float val2)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Select(bool val1, bool val2)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SetBounds(int val1, int val2, int val3, int val4)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SetBounds(int val1, int val2, int val3, int val4, BoundsSpecified bounds)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SetSelected(int index, bool val)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Events
		//
		[MonoTODO]
		public event DrawItemEventHandler DrawItem {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public event MeasureItemEventHandler MeasureItem {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		protected override Size DefaultSize {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected virtual ObjectCollection CreateItemCollection()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Dispose(bool val1)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnChangeUICues(UICuesEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnDataSourceChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnDisplayMemberChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnFontChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnParentChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnResize(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnSelectedItemChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void RefreshItem(int index)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected ContentAlignment RtlTranslateAlignment(ContentAlignment align)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected HorizontalAlignment RtlTranslateAlignment(HorizontalAlignment align)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected LeftRightAlignment RtlTranslateAlignment(LeftRightAlignment align)
		{
			throw new NotImplementedException ();
		}
		//[MonoTODO]
		//protected virtual void Select(bool val1, bool val2)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		protected override void SetBoundsCore( int x, int y,  int width, int height,  BoundsSpecified specified)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected void Sort()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void WndProc(ref Message mgs)
		{
			throw new NotImplementedException ();
		}
		
		//
		// <summary>
		//	This is only a template.  Nothing is implemented yet.
		// This is a subclass
		// </summary>

			public class SelectedObjectCollection :  IList, ICollection, IEnumerable {

			//
			//  --- Constructor
			//
			[MonoTODO]
			public SelectedObjectCollection(ListBox owner)
			{
				throw new NotImplementedException ();
			}

			//
			//  --- Public Properties
			//
			[MonoTODO]
			public int Count {
				get {
					throw new NotImplementedException ();
				}
			}
			[MonoTODO]
			public bool IsReadOnly {
				get {
					throw new NotImplementedException ();
				}
			}
			[MonoTODO]
			public object this[int index] {
				get {
					throw new NotImplementedException ();
				}
				set {
					throw new NotImplementedException ();
				}
			}

			//
			//  --- Public Methods
			//
			[MonoTODO]
			public bool Contains(object selectedObject)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void CopyTo(Array dest, int index)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public virtual bool Equals(object o)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public static bool Equals(object o1, object o2)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public IEnumerator GetEnumerator()
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public int IndexOf(object selectedObject)
			{
				throw new NotImplementedException ();
			}
		 }//End of subclass

			// <summary>
			//	This is only a template.  Nothing is implemented yet.
			//
			// </summary>

			public class ObjectCollection : IList, ICollection {

			//
			//  --- Constructor
			//
			[MonoTODO]
			public ObjectCollection(ListBox box)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public ObjectCollection(ListBox box, object[] objs)
			{
				throw new NotImplementedException ();
			}

			//
			//  --- Public Properties
			//
			[MonoTODO]
			public int Count {
				get {
					throw new NotImplementedException ();
				}
			}
			[MonoTODO]
			public bool IsReadOnly {
				get {
					throw new NotImplementedException ();
				}
			}
			[MonoTODO]
			public virtual object this[int index] {
				get {
					throw new NotImplementedException ();
				}
				set {
					throw new NotImplementedException ();
				}
			}

			//
			//  --- Public Methods
			//
			[MonoTODO]
			public int Add(object item)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void AddRange(object[] items)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void AddRange(ListBox.ObjectCollection collection)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void Clear()
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public bool Contains(object value)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void CopyTo(object[] dest, int arrayIndex)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public virtual bool Equals(object o)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public static bool Equals(object o1, object o2)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public IEnumerator GetEnumerator()
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public int IndexOf(object val)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void Insert(int index, object item)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void Remove(object val)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void RemoveAt(int index)
			{
				throw new NotImplementedException ();
			}
		 }//end of SubClass

			// <summary>
			//	This is only a template.  Nothing is implemented yet.
			//
			// </summary>

			public class SelectedIndexCollection :  IList, ICollection, IEnumerable {

			//
			//  --- Constructor
			//
			[MonoTODO]
			public SelectedIndexCollection(ListBox owner)
			{
				throw new NotImplementedException ();
			}

			//
			//  --- Public Properties
			//
			[MonoTODO]
			public int Count {
				get {
					throw new NotImplementedException ();
				}
			}
			[MonoTODO]
			public bool IsReadOnly {
				get {
					throw new NotImplementedException ();
				}
			}
			[MonoTODO]
			public int this[int index] {
				get {
					throw new NotImplementedException ();
				}
			}

			//
			//  --- Public Methods
			//
			[MonoTODO]
			public bool Contains(int selectedIndex)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void CopyTo(Array dest, int index)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public virtual bool Equals(object o)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public static bool Equals(object o1, object o2)
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public IEnumerator GetEnumerator()
			{
				throw new NotImplementedException ();
			}
		 }//End of subclass
	 }
}
