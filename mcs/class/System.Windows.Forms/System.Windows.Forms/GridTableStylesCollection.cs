//
// System.Windows.Forms.GridTableStylesCollection.cs
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
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Clear()
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
		public bool Contains(DataGridTableStyle table)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove(DataGridTableStyle table)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt(int index)
		{
			throw new NotImplementedException ();
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

	 }
}
