//
// System.Windows.Forms.GridTableStylesCollection.cs
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

    public class GridTableStylesCollection : BaseCollection, IList {

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public DataGridTableStyle this[int index]  {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public DataGridTableStyle this[string s]  {
			get {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public virtual int Add(DataGridTableStyle table)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void AddRange(DataGridTableStyle[] tables)
		{
			//FIXME:
		}

		[MonoTODO]
		public virtual void Clear()
		{
			//FIXME:
		}

		[MonoTODO]
		public bool Contains(DataGridTableStyle table)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove(DataGridTableStyle table)
		{
			//FIXME:
		}

		[MonoTODO]
		public void RemoveAt(int index)
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
				throw new NotImplementedException ();
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

		//[MonoTODO]
		object IList.this[int index]{
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
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IList.RemoveAt( int index){
			throw new NotImplementedException ();
		}
		// End of IList interface

	 }
}
