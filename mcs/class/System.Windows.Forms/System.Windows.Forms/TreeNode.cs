//
// System.Windows.Forms.TreeNode
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;
namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

    public class TreeNode : MarshalByRefObject, ICloneable {

		TreeNodeCollection children;
		TreeNode           parent;
		TreeView           treeView;
		string             text;
		IntPtr             handle;
		int                imageIndex;
		int                selectedImageIndex;
		bool               checked_;

		[MonoTODO]
		public TreeNode()
		{
			children = null;
			parent   = null;
			treeView = null;
			text     = String.Empty;
			handle   = IntPtr.Zero;
			imageIndex = 0;
			selectedImageIndex = 0;
			checked_ = false;
		}
		[MonoTODO]
		public TreeNode( string text ) : this ( )
		{
			this.text = text;
		}
		[MonoTODO]
		public TreeNode( string text, TreeNode[] children ) : this ( text )
		{
			Nodes.AddRange ( children );
		}

		[MonoTODO]
		public TreeNode( string text, int imageIndex, int selectedImageIndex ) : this ( text )
		{
			this.imageIndex = imageIndex;
			this.selectedImageIndex = selectedImageIndex;
		}

		[MonoTODO]
		public TreeNode( string text, int imageIndex, int selectedImageIndex, TreeNode[] children ) : this ( text, children )
		{
			this.imageIndex = imageIndex;
			this.selectedImageIndex = selectedImageIndex;
		}

		// --- Public Properties
		
		[MonoTODO]
		public Color BackColor {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				//FIXME:
			}
		}
		[MonoTODO]
		public Rectangle Bounds {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Checked {
			get {
				return checked_;
			}
			set {
				checked_ = value;
			}
		}
		[MonoTODO]
		public TreeNode FirstNode {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Color ForeColor {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string FullPath {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public IntPtr Handle {
			get {
				 if ( handle == IntPtr.Zero )
					createNode ( );
				 return handle; 
			}
		}
		[MonoTODO]
		public int ImageIndex {
			get {
				if ( TreeView != null && TreeView.ImageIndex != 0 && imageIndex == 0 )
					return TreeView.ImageIndex;
				return imageIndex;
			}
			set {
				imageIndex = value;
			}
		}
		[MonoTODO]
		public int Index {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool IsEditing {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool IsExpanded {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool IsSelected {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool IsVisible {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public TreeNode LastNode {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public TreeNode NextNode {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public TreeNode NextVisibleNode {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				//FIXME:
			}
		}
		[MonoTODO]
		public Font NodeFont {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				//FIXME:
			}
		}
		[MonoTODO]
		public TreeNodeCollection Nodes {
			get {
				if ( children == null )
					children = new TreeNodeCollection ( this, treeView );
				return children;
			}
		}

		public TreeNode Parent {
			get { return parent; }
		}
		[MonoTODO]
		public TreeNode PrevNode {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public TreeNode PrevVisibleNode {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int SelectedImageIndex {
			get {
				if ( TreeView != null && TreeView.SelectedImageIndex != 0 && selectedImageIndex == 0 )
					return TreeView.SelectedImageIndex;

				return selectedImageIndex;
			}
			set {
				selectedImageIndex = value;	
			}
		}
		[MonoTODO]
		public object Tag {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				//FIXME:
			}
		}
		[MonoTODO]
		public string Text {
			get { return text; }
			set {
				text = value;
			}
		}

		public TreeView TreeView {
			get { return treeView; }
		}
		
		// --- Public Methods
		
		[MonoTODO]
		public void BeginEdit()
		{
			//FIXME:
		}
		[MonoTODO]
		public virtual object Clone()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Collapse()
		{
			//FIXME:
		}
		[MonoTODO]
		public void EndEdit(bool cancel)
		{
			//FIXME:
		}
		[MonoTODO]
		public void EnsureVisible()
		{
			//FIXME:
		}
		[MonoTODO]
		public void Expand()
		{
			//FIXME:
		}
		[MonoTODO]
		public void ExpandAll()
		{
			//FIXME:
		}
		[MonoTODO]
		public static TreeNode FromHandle(TreeView tree, IntPtr handle)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int GetNodeCount(bool includeSubTrees)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Remove()
		{
			//FIXME:
		}
		[MonoTODO]
		public void Toggle()
		{
			//FIXME:
		}
		[MonoTODO]
		public override string ToString()
		{
			//FIXME:
			return base.ToString();
		}

		internal void setParent ( TreeNode parent )
		{
			this.parent = parent;
		}
			
		internal void setHandle ( IntPtr hItem )
		{
			this.handle = hItem;
		}

		internal void setTreeView ( TreeView treeView )
		{
			this.treeView = treeView;
			foreach ( TreeNode node in Nodes )
				node.setTreeView ( treeView );
		}

		internal void makeTree ( IntPtr parent )
		{
			if ( handle == IntPtr.Zero )
				handle = insertNode ( parent );

			foreach ( TreeNode node in Nodes )
				node.makeTree ( handle );
		}

		internal void createNode ( )
		{
			IntPtr parentHandle = IntPtr.Zero;

			if ( Parent != null )
				parentHandle = Parent.Handle;
			else {
				unchecked {
					int intPtr = ( int ) TreeViewItemInsertPosition.TVI_ROOT;
					parentHandle = (IntPtr) intPtr;
				}
			}

			if ( parentHandle != IntPtr.Zero ) {
				handle = insertNode ( parentHandle );
			}
		}

		internal IntPtr insertNode (  IntPtr parent )
		{
			if ( TreeView != null ) {
				TVINSERTSTRUCT insStruct = new TVINSERTSTRUCT ( );
				insStruct.hParent = parent;

				insStruct.item.mask = (uint) TreeViewItemFlags.TVIF_TEXT;
				insStruct.item.pszText = Text;
				return (IntPtr) Win32.SendMessage ( TreeView.Handle , TreeViewMessages.TVM_INSERTITEMA, 0, ref insStruct );
			}
			return IntPtr.Zero;
		}
	}
}
