//
// System.Windows.Forms.TreeNodeCollection
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Collections;

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

    public class TreeNodeCollection : IList, ICollection, IEnumerable {

		
		//  --- Public Properties
		
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
		public virtual TreeNode this[int index] {
			get	{
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		// --- Public Methods
		
		[MonoTODO]
		public virtual TreeNode Add(string text) 
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual int Add(TreeNode node) 
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void AddRange(TreeNode[] nodes) 
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void Clear() 
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public bool Contains(TreeNode node) 
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void CopyTo(Array dest, int index) 
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public IEnumerator GetEnumerator() 
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int IndexOf(TreeNode node) 
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void Insert(int index, TreeNode node) 
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Remove(TreeNode node) 
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void RemoveAt(int index) 
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
