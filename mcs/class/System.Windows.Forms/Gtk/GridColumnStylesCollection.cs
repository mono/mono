//
// System.Windows.Forms.GridColumnStylesCollection.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Collections;
using System.ComponentModel;
namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class GridColumnStylesCollection : BaseCollection, IList {
		//
		//  --- Public Methods
		//
		[MonoTODO]
		public virtual int Add(DataGridColumnStyle column) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AddRange(DataGridColumnStyle [] columns)
		{
			//FIXME:
		}

		[MonoTODO]
		public void Clear()
		{
			//FIXME:
		}

		[MonoTODO]
		public bool Contains()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int IndexOf(DataGridColumnStyle element)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove(DataGridColumnStyle column)
		{
			//FIXME:
		}

		[MonoTODO]
		public void RemoveAt(int index)
		{
			//FIXME:
		}

		[MonoTODO]
		public void ResetPropertyDescriptors()
		{
			//FIXME:
		}

		//
		//  --- Public Events
		//
		[MonoTODO]
		public event CollectionChangeEventHandler CollectionChanged;

		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected override ArrayList List {
			get {
				return base.List;
				//FIXME:
			}
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected void OnCollectionChanged(CollectionChangeEventArgs cevent)
		{
			//FIXME:
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

		[MonoTODO]
		public object this[int index]{
			get{
				throw new NotImplementedException ();
			}
			set{
				//FIXME:
			}
		}
		
		[MonoTODO]
		void IList.Clear(){
			//FIXME:
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
			//FIXME:
		}

		[MonoTODO]
		void IList.Remove( object value){
			//FIXME:
		}

		[MonoTODO]
		void IList.RemoveAt( int index){
			//FIXME:
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
			//FIXME:
		}
		// End Of ICollection
	}
}
