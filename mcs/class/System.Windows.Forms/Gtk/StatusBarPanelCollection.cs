//
// System.Windows.Forms.StatusBarPanelCollection
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//
using System.Collections;

namespace System.Windows.Forms {

	// <summary>
	//	Represents the collection of panels in a StatusBar control.
	// </summary>

	public class StatusBarPanelCollection : IList, ICollection, IEnumerable {

		
		//  --- Public Methods
		
		[MonoTODO]
		StatusBarPanelCollection(StatusBar owner)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual int Add(StatusBarPanel panel)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual StatusBarPanel Add(string s)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void AddRange(StatusBarPanel[] panels)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void Clear()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public bool Contains(StatusBarPanel panel)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int IndexOf(StatusBarPanel panel)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void Insert(int index, StatusBarPanel panel)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void Remove(StatusBarPanel panel)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void RemoveAt(int index)
		{
			throw new NotImplementedException ();
		}

		
		//  --- Protected Methods
		
		[MonoTODO]
		~StatusBarPanelCollection()
		{
			throw new NotImplementedException ();
		}

		
		//  --- Public Properties
		
		[MonoTODO]
		public int Count {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public bool IsReadOnly {

			get
			{
				return false; // for this collection, this is always false
			}
		}
		[MonoTODO]
		public virtual StatusBarPanel this[int index] {

			get{ throw new NotImplementedException (); }
			set{ throw new NotImplementedException (); }
		}
		/// <summary>
		/// IList Interface implmentation.
		/// </summary>
		bool IList.IsReadOnly{
			get{
				// We allow addition, removeal, and editing of items after creation of the list.
				return false;
			}
		}
		bool IList.IsFixedSize{
			get{
				// We allow addition and removeal of items after creation of the list.
				return false;
			}
		}

		//[MonoTODO]
		object IList.this[int index]{
			get{
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		void IList.Clear(){
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		int IList.Add( object value){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool IList.Contains( object value){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int IList.IndexOf( object value){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IList.Insert(int index, object value){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IList.Remove( object value){
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IList.RemoveAt( int index){
			throw new NotImplementedException ();
		}
		// End of IList interface
		/// <summary>
		/// ICollection Interface implmentation.
		/// </summary>
		int ICollection.Count{
			get{
				throw new NotImplementedException ();
			}
		}
		bool ICollection.IsSynchronized{
			get{
				throw new NotImplementedException ();
			}
		}
		object ICollection.SyncRoot{
			get{
				throw new NotImplementedException ();
			}
		}
		void ICollection.CopyTo(Array array, int index){
			throw new NotImplementedException ();
		}
		// End Of ICollection
	}
}
