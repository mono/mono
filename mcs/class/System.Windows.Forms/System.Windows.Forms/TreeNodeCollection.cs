//
// System.Windows.Forms.TreeNodeCollection
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) 2002 Ximian, Inc
//
using System.Collections;

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

    public class TreeNodeCollection : IList, ICollection, IEnumerable {

		private TreeNode  owner;
		private ArrayList list;
		private TreeView  treeView;

		internal TreeNodeCollection ( TreeNode owner, TreeView  treeView )
		{
			list = new ArrayList();
			this.owner = owner;
			this.treeView = treeView;
		}
		
		public int Count {
			get { return list.Count; }
		}

		public bool IsReadOnly {
			get { return list.IsReadOnly; }
		}
		[MonoTODO]
		public virtual TreeNode this[int index] {
			get {
				return (TreeNode) list[index];
			}
			set {
				list[index] = value;
			}
		}
		
		public virtual TreeNode Add( string text ) 
		{
			TreeNode node =  new TreeNode ( text );
			Add ( node );
			return node;
		}

		[MonoTODO]
		public virtual int Add( TreeNode node ) 
		{
			if ( node == null )
				throw new ArgumentNullException("value");

			if ( node.Parent != null )
				throw new ArgumentException("Object already has a parent.", "node");

			node.setParent( owner );
			node.setTreeView ( treeView );
			int index = list.Add( node );
			return 	index;		
		}

		public virtual void AddRange( TreeNode[] nodes ) 
		{
			if ( nodes == null )
				throw new ArgumentNullException("nodes");

			foreach ( TreeNode node in nodes ) {
				// it will do a check for parent and set the parent
				Add ( node );
			}
		}

		[MonoTODO]
		public virtual void Clear() 
		{
			foreach ( object node in list )
				( ( TreeNode )node ).setParent ( null );

			list.Clear();
		}

		public bool Contains( TreeNode node ) 
		{
			return list.Contains( node );
		}
		[MonoTODO]
		public void CopyTo(Array dest, int index) 
		{
			//FIXME:
		}

		public IEnumerator GetEnumerator() 
		{
			return list.GetEnumerator();
		}

		public int IndexOf( TreeNode node ) 
		{
			return list.IndexOf( node );
		}
		[MonoTODO]
		public virtual void Insert( int index, TreeNode node ) 
		{
			if ( node == null )
				throw new ArgumentNullException ( "node" );

			if ( node.Parent != null)
				throw new ArgumentException ( "Object already has a parent.", "node" );

			if (index < 0 || index > Count )
				throw new ArgumentOutOfRangeException( "index" );

			list.Insert( index, node );
			node.setParent ( owner ); 
		}

		[MonoTODO]
		public void Remove( TreeNode node ) 
		{
			if ( node == null )
				throw new ArgumentNullException( "node" );

			list.Remove( node );
			node.setParent ( null );
		}

		[MonoTODO]
		public virtual void RemoveAt( int index ) 
		{
			if (index < 0 || index > Count )
				throw new ArgumentOutOfRangeException( "index" );

			TreeNode node = (TreeNode) list[ index ];
			list.RemoveAt( index );
			node.setParent ( null );
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
