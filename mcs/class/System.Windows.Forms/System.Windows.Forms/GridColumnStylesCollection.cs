//
// System.Windows.Forms.GridColumnStylesCollection.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Collections;
using System.ComponentModel;
namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

    public class GridColumnStylesCollection : BaseCollection, IList {

		//
		//  --- Public Properties
		//
//		[MonoTODO]
//		public DataGridColumnStyle Item this[int index]  {
//			get {
//				throw new NotImplementedException ();
//			}
//		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public virtual int Add(DataGridColumnStyle column)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AddRange(DataGridColumnStyle [] columns)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Equals(object o)
		{
			throw new NotImplementedException ();
		}

		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}

		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}

		[MonoTODO]
		public int IndexOf(DataGridColumnStyle element)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove(DataGridColumnStyle column)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt(int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ResetPropertyDescriptors()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Public Events
		//
		[MonoTODO]
		public event CollectionChangeEventHandler CollectionChanged {
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
			throw new NotImplementedException ();
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
