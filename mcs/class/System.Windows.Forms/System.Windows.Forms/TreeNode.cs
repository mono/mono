//
// System.Windows.Forms.TreeNode
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;
namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

    public class TreeNode : MarshalByRefObject, ICloneable {

		
		//  --- Public Constructors
		
		[MonoTODO]
		public TreeNode()
		{
			
		}
		[MonoTODO]
		public TreeNode(string text)
		{
			
		}
		[MonoTODO]
		public TreeNode(string text, TreeNode[] children)
		{
			
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
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int ImageIndex {
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
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public TreeNode Parent {
			get
			{
				throw new NotImplementedException ();
			}
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
		public TreeView TreeView {
			get
			{
				throw new NotImplementedException ();
			}
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
	}
}
