//
// System.Windows.Forms.TreeView
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;
namespace System.Windows.Forms {

	// <summary>

	//
	// </summary>

    public class TreeView : Control {

		private int imageIndex;
		private int selectedImageIndex;

		//
		//  --- Public Constructors
		//
		[MonoTODO]
		public TreeView()
		{
			imageIndex = 0;
			selectedImageIndex = 0;
		}
		
		// --- Public Properties
		
		[MonoTODO]
		public override Color BackColor {
			get
			{
				//FIXME:
				return base.BackColor;
			}
			set
			{
				//FIXME:
				base.BackColor = value;
			}
		}
		[MonoTODO]
		public override Image BackgroundImage {
			get
			{
				//FIXME:
				return base.BackgroundImage;
			}
			set
			{
				//FIXME:
				base.BackgroundImage = value;
			}
		}
		[MonoTODO]
		public BorderStyle BorderStyle {
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
		public bool CheckBoxes {
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
		public override Color ForeColor {
			get
			{
				//FIXME:
				return base.ForeColor;
			}
			set
			{
				//FIXME:
				base.ForeColor = value;
			}
		}
		[MonoTODO]
		public bool FullRowSelect {
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
		public bool HideSelection {
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
		public bool HotTracking {
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
		public int ImageIndex {
			get
			{
				return imageIndex;
			}
			set
			{
				//FIXME:
			}
		}
		[MonoTODO]
		public ImageList ImageList {
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
		public int Indent {
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
		public int ItemHeight {
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
		public bool LabelEdit {
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
		public string PathSeparator {
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
		public bool Scrollable {
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
		public int SelectedImageIndex {
			get
			{
				return selectedImageIndex;
			}
			set
			{
				//FIXME:
			}
		}
		[MonoTODO]
		public TreeNode SelectedNode {
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
		public bool ShowLines {
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
		public bool ShowPlusMinus {
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
		public bool ShowRootLines {
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
		public bool Sorted {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				//FIXME:
			}
		}

		public override string Text {
			get {
				//FIXME:
				return base.Text;
			}
			set
			{
				//FIXME:
				base.Text = value;
			}
		}
		[MonoTODO]
		public TreeNode TopNode {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int VisibleCount {
			get
			{
				throw new NotImplementedException ();
			}
		}
		
		// --- Public Methods
		
		[MonoTODO]
		public void BeginUpdate() 
		{
			//FIXME:
		}
		[MonoTODO]
		public void CollapseAll()
		{
			//FIXME:
		}
		[MonoTODO]
		public void EndUpdate()
		{
			//FIXME:
		}
		[MonoTODO]
		public void ExpandAll()
		{
			//FIXME:
		}
		[MonoTODO]
		public TreeNode GetNodeAt(Point pt)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public TreeNode GetNodeAt(int x, int y)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int GetNodeCount(bool includeSubTrees)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override string ToString()
		{
			//FIXME:
			return base.ToString();
		}
		
		// --- Public Events
		
		public event TreeViewEventHandler AfterCheck;
		public event TreeViewEventHandler AfterCollapse;
		public event TreeViewEventHandler AfterExpand;
		public event NodeLabelEditEventHandler AfterLabelEdit;
		public event TreeViewEventHandler AfterSelect;
		public event TreeViewCancelEventHandler BeforeCheck;
		public event TreeViewCancelEventHandler BeforeCollapse;
		public event TreeViewCancelEventHandler BeforeExpand;
		public event NodeLabelEditEventHandler BeforeLabelEdit;
		public event TreeViewCancelEventHandler BeforeSelect;
		public event ItemDragEventHandler ItemDrag;
		//public new event PaintEventHandler Paint;
        
        // --- Protected Properties
        
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;

				createParams.ClassName = "TREEVIEW";
				createParams.Style = (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE);
				return createParams;
			}		
		}
		[MonoTODO]
		protected override Size DefaultSize {
			get
			{
				return new Size(121,97);//Correct size.
			}
		}
		
		// --- Protected Methods
		
		[MonoTODO]
		protected override void CreateHandle()
		{
			//FIXME: just to get it to run
			base.CreateHandle();
		}


		[MonoTODO]
		protected override bool IsInputKey(Keys keyData)
		{
			//FIXME:
			return base.IsInputKey(keyData);
		}
		[MonoTODO]
		protected virtual void OnAfterCheck(TreeViewEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnAfterCollapse(TreeViewEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnAfterExpand(TreeViewEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnAfterLabelEdit(NodeLabelEditEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnAfterSelect(TreeViewEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnBeforeCheck(TreeViewCancelEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnBeforeCollapse(TreeViewCancelEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnBeforeExpand(TreeViewCancelEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnBeforeLabelEdit(NodeLabelEditEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected virtual void OnBeforeSelect(TreeViewCancelEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e)
		{
			//FIXME:
			base.OnHandleCreated(e);
		}
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e)
		{
			//FIXME:
			base.OnHandleDestroyed(e);
		}
		[MonoTODO]
		protected virtual void OnItemDrag(ItemDragEventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected override void OnKeyDown(KeyEventArgs e)
		{
			//FIXME:
			base.OnKeyDown(e);
		}
		[MonoTODO]
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			//FIXME:
			base.OnKeyPress(e);
		}
		[MonoTODO]
		protected override void OnKeyUp(KeyEventArgs e)
		{
			//FIXME:
            base.OnKeyUp(e);
		}
		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			//FIXME:
			base.WndProc(ref m);
		}
	}
}
