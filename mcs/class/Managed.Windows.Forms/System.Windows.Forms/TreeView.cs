// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//	Kazuki Oikawa (kazuki@panicode.com)

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[DefaultProperty("Nodes")]
	[DefaultEvent("AfterSelect")]
	[Designer("System.Windows.Forms.Design.TreeViewDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class TreeView : Control {
		#region Fields
		private string path_separator = "\\";
		private int item_height = -1;
		private bool sorted;
		internal TreeNode root_node;
		internal bool nodes_added;
		private TreeNodeCollection nodes;

		private TreeViewAction selection_action = TreeViewAction.Unknown;
		internal TreeNode selected_node = null;
		private TreeNode focused_node = null;
		private TreeNode highlighted_node;
		private Rectangle mouse_rect = Rectangle.Empty;
		private bool select_mmove = false;

		private ImageList image_list;
		private int image_index = -1;
		private int selected_image_index = -1;

		private bool full_row_select;
		private bool hot_tracking;
		private int indent = 19;

		private NodeLabelEditEventArgs edit_args;
		private LabelEditTextBox edit_text_box;
		internal TreeNode edit_node;
		
		private bool checkboxes;
		private bool label_edit;
		private bool scrollable = true;
		private bool show_lines = true;
		private bool show_root_lines = true;
		private bool show_plus_minus = true;
		private bool hide_selection = true;

		private int max_visible_order = -1;
		private VScrollBar vbar;
		private HScrollBar hbar;
		private bool vbar_bounds_set;
		private bool hbar_bounds_set;
		internal int skipped_nodes;
		internal int hbar_offset;
		
		private int update_stack;
		private bool update_needed;
		
		private Pen dash;
		private Color line_color;
		private StringFormat string_format;

		private int drag_begin_x = 0;
		private int drag_begin_y = 0;
		private long handle_count = 1;

		#endregion	// Fields

		#region Public Constructors	
		public TreeView ()
		{
			border_style = BorderStyle.Fixed3D;
			base.background_color = ThemeEngine.Current.ColorWindow;
			base.foreground_color = ThemeEngine.Current.ColorWindowText;

			root_node = new TreeNode (this);
			root_node.Text = "ROOT NODE";
			nodes = new TreeNodeCollection (root_node);
			root_node.SetNodes (nodes);

			MouseDown += new MouseEventHandler (MouseDownHandler);
			MouseUp += new MouseEventHandler(MouseUpHandler);
			MouseMove += new MouseEventHandler(MouseMoveHandler);
			SizeChanged += new EventHandler (SizeChangedHandler);
			FontChanged += new EventHandler (FontChangedHandler);
			LostFocus += new EventHandler (LostFocusHandler);
			GotFocus += new EventHandler (GotFocusHandler);
			MouseWheel += new MouseEventHandler(MouseWheelHandler);
			VisibleChanged += new EventHandler (VisibleChangedHandler);

			SetStyle (ControlStyles.UserPaint | ControlStyles.StandardClick
#if NET_2_0
				| ControlStyles.UseTextForAccessibility
#endif
				, false);

			string_format = new StringFormat ();
			string_format.LineAlignment = StringAlignment.Center;
			string_format.Alignment = StringAlignment.Center;

			vbar = new ImplicitVScrollBar ();
			hbar = new ImplicitHScrollBar ();

			vbar.Visible = false;
			hbar.Visible = false;
			vbar.ValueChanged += new EventHandler (VScrollBarValueChanged);
			hbar.ValueChanged += new EventHandler (HScrollBarValueChanged);

			SuspendLayout ();
			Controls.AddImplicit (vbar);
			Controls.AddImplicit (hbar);
			ResumeLayout ();
		}

		#endregion	// Public Constructors

		#region Public Instance Properties
		public override Color BackColor {
			get { return base.BackColor;}
			set {
				base.BackColor = value;

				CreateDashPen ();
				Invalidate ();
			}
		}


		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

		[DefaultValue(BorderStyle.Fixed3D)]
		[DispId(-504)]
		public BorderStyle BorderStyle {
			get { return InternalBorderStyle; }
			set { InternalBorderStyle  = value; }
		}

		[DefaultValue(false)]
		public bool CheckBoxes {
			get { return checkboxes; }
			set {
				if (value == checkboxes)
					return;
				checkboxes = value;

				// Match a "bug" in the MS implementation where disabling checkboxes
				// collapses the entire tree, but enabling them does not affect the
				// state of the tree.
				if (!checkboxes)
					root_node.CollapseAllUncheck ();

				Invalidate ();
			}
		}

		public override Color ForeColor {
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}
		[DefaultValue(false)]
		public bool FullRowSelect {
			get { return full_row_select; }
			set {
				if (value == full_row_select)
					return;
				full_row_select = value;
				Invalidate ();
			}
		}
		[DefaultValue(true)]
		public bool HideSelection {
			get { return hide_selection; }
			set {
				if (hide_selection == value)
					return;
				hide_selection = value;
				Invalidate ();
			}
		}

		[DefaultValue(false)]
		public bool HotTracking {
			get { return hot_tracking; }
			set { hot_tracking = value; }
		}

		[DefaultValue(0)]
		[Editor("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[Localizable(true)]
		[TypeConverter(typeof(TreeViewImageIndexConverter))]
		public int ImageIndex {
			get { return image_index; }
			set {
				if (value < -1) {
					throw new ArgumentException ("'" + value + "' is not a valid value for 'value'. " +
						"'value' must be greater than or equal to 0.");
				}
				if (image_index == value)
					return;
				image_index = value;
				Invalidate ();
			}
		}

		[DefaultValue(null)]
		public ImageList ImageList {
			get { return image_list; }
			set {
				image_list = value;
				Invalidate ();
			}
		}

		[Localizable(true)]
		public int Indent {
			get { return indent; }
			set {
				if (indent == value)
					return;
				if (value > 32000) {
					throw new ArgumentException ("'" + value + "' is not a valid value for 'Indent'. " +
						"'Indent' must be less than or equal to 32000");
				}	
				if (value < 0) {
					throw new ArgumentException ("'" + value + "' is not a valid value for 'Indent'. " +
						"'Indent' must be greater than or equal to 0.");
				}
				indent = value;
				Invalidate ();
			}
		}

		[Localizable(true)]
		public int ItemHeight {
			get {
				if (item_height == -1)
					return FontHeight + 3;
				return item_height;
			}
			set {
				if (value == item_height)
					return;
				item_height = value;
				Invalidate ();
			}
		}

		internal int ActualItemHeight {
			get {
				int res = ItemHeight;
				if (ImageList != null && ImageList.ImageSize.Height > res)
					res = ImageList.ImageSize.Height;
				return res;
			}
		}

		[DefaultValue(false)]
		public bool LabelEdit {
			get { return label_edit; }
			set { label_edit = value; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[MergableProperty(false)]
		[Localizable(true)]
		public TreeNodeCollection Nodes {
			get { return nodes; }
		}

		[DefaultValue("\\")]
		public string PathSeparator {
			get { return path_separator; }
			set { path_separator = value; }
		}

		[DefaultValue(true)]
		public bool Scrollable {
			get { return scrollable; }
			set {
				if (scrollable == value)
					return;
				scrollable = value;
				UpdateScrollBars ();
			}
		}

		[Editor("System.Windows.Forms.Design.ImageIndexEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[TypeConverter(typeof(TreeViewImageIndexConverter))]
		[Localizable(true)]
		[DefaultValue(0)]
		public int SelectedImageIndex {
			get { return selected_image_index; }
			set {
				if (value < -1) {
					throw new ArgumentException ("'" + value + "' is not a valid value for 'value'. " +
						"'value' must be greater than or equal to 0.");
				}
				UpdateNode (SelectedNode);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public TreeNode SelectedNode {
			get { return selected_node; }
			set {
				if (selected_node == value) {
					selection_action = TreeViewAction.Unknown;
					return;
				}

				TreeViewCancelEventArgs e = new TreeViewCancelEventArgs (value, false, selection_action);
				OnBeforeSelect (e);

				if (e.Cancel)
					return;

				Rectangle invalid = Rectangle.Empty;

				if (selected_node != null) {
					invalid = Bloat (selected_node.Bounds);
				}
				if (focused_node != null) {
					invalid = Rectangle.Union (invalid,
							Bloat (focused_node.Bounds));
				}

				if (value != null)
					invalid = Rectangle.Union (invalid, Bloat (value.Bounds));

				highlighted_node = value;
				selected_node = value;
				focused_node = value;

				if (full_row_select) {
					invalid.X = 0;
					invalid.Width = ViewportRectangle.Width;
				}

				if (invalid != Rectangle.Empty)
					Invalidate (invalid);

				// We ensure its visible after we update because
				// scrolling is used for insure visible
				if (selected_node != null)
					selected_node.EnsureVisible ();

				OnAfterSelect (new TreeViewEventArgs (value, TreeViewAction.Unknown));
				selection_action = TreeViewAction.Unknown;
			}
		}

		private Rectangle Bloat (Rectangle rect)
		{
			rect.Y--;
			rect.X--;
			rect.Height += 2;
			rect.Width += 2;
			return rect;
		}

		[DefaultValue(true)]
		public bool ShowLines {
			get { return show_lines; }
			set {
				if (show_lines == value)
					return;
				show_lines = value;
				Invalidate ();
			}
		}

		[DefaultValue(true)]
		public bool ShowPlusMinus {
			get { return show_plus_minus; }
			set {
				if (show_plus_minus == value)
					return;
				show_plus_minus = value;
				Invalidate ();
			}
		}

		[DefaultValue(true)]
		public bool ShowRootLines {
			get { return show_root_lines; }
			set {
				if (show_root_lines == value)
					return;
				show_root_lines = value;
				Invalidate ();
			}
		}

		[DefaultValue(false)]
		public bool Sorted {
			get { return sorted; }
			set {
				if (sorted != value)
					sorted = value;
				if (sorted) {
					Sort ();
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Bindable(false)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public TreeNode TopNode {
			get {
				if (root_node.FirstNode == null)
					return null;
				OpenTreeNodeEnumerator one = new OpenTreeNodeEnumerator (root_node.FirstNode);
				one.MoveNext ();
				for (int i = 0; i < skipped_nodes; i++)
					one.MoveNext ();
				return one.CurrentNode;
			}
#if NET_2_0
			set {
				SetTop (value);
			}
#endif
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int VisibleCount {
			get {
				return ViewportRectangle.Height / ActualItemHeight;
			}
		}

#if NET_2_0
		/// According to MSDN this property has no effect on the treeview
		private bool double_buffered;
		protected override bool DoubleBuffered {
			get { return double_buffered; }
			set { double_buffered = value; }
		}
#endif

#if NET_2_0
		public
#else
		private
#endif
		Color LineColor {
			get {
				if (line_color == Color.Empty) {
					Color res = ControlPaint.Dark (BackColor);
					if (res == BackColor)
						res = ControlPaint.Light (BackColor);
					return res;
				}
				return line_color;
			}
			set {
				line_color = value;
				if (show_lines) {
					CreateDashPen ();
					Invalidate ();
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				CreateParams cp = base.CreateParams;
				return cp;
			}
		}

		protected override Size DefaultSize {
			get { return new Size (121, 97); }
		}

		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public void BeginUpdate ()
		{
			update_stack++;
		}

		public void EndUpdate ()
		{
			if (update_stack > 1) {
				update_stack--;
			} else {
				update_stack = 0;
				if (update_needed) {
					RecalculateVisibleOrder (root_node);
					UpdateScrollBars ();
					//	if (SelectedNode != null)
					//		SelectedNode.EnsureVisible ();
					Invalidate (ViewportRectangle);
					update_needed = false;
				}
			}
		}

#if NET_2_0
		public
#else
		private
#endif
		void Sort ()
		{
			Nodes.Sort ();
			RecalculateVisibleOrder (root_node);
			UpdateScrollBars ();
			Invalidate ();
		}

		public void ExpandAll ()
		{
			BeginUpdate ();
			root_node.ExpandAll ();

			EndUpdate ();

			// Walk all the way to the end, then walk back visible count
			//to find the new top node
			OpenTreeNodeEnumerator walk = new OpenTreeNodeEnumerator (root_node);
			while (walk.MoveNext ())
			{ }

			walk.CurrentNode.EnsureVisible ();
		}

		
		public void CollapseAll ()
		{
			TreeNode walk = TopNode;
			
			while (walk.parent != root_node)
				walk = walk.parent;

			BeginUpdate ();
			root_node.CollapseAll ();
			EndUpdate ();

			SetTop (walk);
		}

		public TreeNode GetNodeAt (Point pt) {
			return GetNodeAt (pt.Y);
		}

		public TreeNode GetNodeAt (int x, int y)
		{
			return GetNodeAt (y);
		}

		private TreeNode GetNodeAtUseX (int x, int y) {
			TreeNode node = GetNodeAt (y);
			if (node == null || !(IsTextArea (node, x) || full_row_select))
				return null;
			return node;
					
		}

		public int GetNodeCount (bool include_subtrees) {
			return root_node.GetNodeCount (include_subtrees);
		}

		public override string ToString () {
			int count = Nodes.Count;
			if (count <= 0)
				return String.Concat (base.ToString (), "Node Count: 0");
			return String.Concat (base.ToString (), "Node Count: ", count, " Nodes[0]: ", Nodes [0]);
						
		}

		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void CreateHandle () {
			base.CreateHandle ();

			UpdateScrollBars ();
		}

		protected override void Dispose (bool disposing) {
			if (disposing)
				image_list = null;

			base.Dispose (disposing);
		}

		protected OwnerDrawPropertyBag GetItemRenderStyles (TreeNode node, int state) {
			return node.prop_bag;
		}

		protected override bool IsInputKey (Keys key_data)
		{
			if ((key_data & Keys.Alt) == 0) {
				switch (key_data & Keys.KeyCode) {
				case Keys.Enter:
				case Keys.Escape:
				case Keys.Prior:
				case Keys.Next:
				case Keys.End:
				case Keys.Home:
				case Keys.Left:
				case Keys.Up:
				case Keys.Right:
				case Keys.Down:
					return true;
				}
			}
			return base.IsInputKey (key_data);
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			OpenTreeNodeEnumerator ne;

			switch (e.KeyData & Keys.KeyCode) {
			case Keys.Add:
				if (selected_node != null && selected_node.IsExpanded)
					selected_node.Expand ();
				break;
			case Keys.Subtract:
				if (selected_node != null && selected_node.IsExpanded)
					selected_node.Collapse ();
				break;
			case Keys.Left:
				if (selected_node != null) {
					if (selected_node.IsExpanded)
						selected_node.Collapse ();
					else {
						TreeNode parent = selected_node.Parent;
						if (parent != null) {
							selection_action = TreeViewAction.ByKeyboard;
							SelectedNode = parent;
						}
					}
				}
				break;
			case Keys.Right:
				if (selected_node != null) {
					if (!selected_node.IsExpanded)
						selected_node.Expand ();
					else {
						TreeNode child = selected_node.FirstNode;
						if (child != null)
							SelectedNode = child;
					}
				}
				break;
			case Keys.Up:
				if (selected_node != null) {
					ne = new OpenTreeNodeEnumerator (selected_node);
					if (ne.MovePrevious () && ne.MovePrevious ()) {
						selection_action = TreeViewAction.ByKeyboard;
						SelectedNode = ne.CurrentNode;
					}
				}
				break;
			case Keys.Down:
				if (selected_node != null) {
					ne = new OpenTreeNodeEnumerator (selected_node);
					if (ne.MoveNext () && ne.MoveNext ()) {
						selection_action = TreeViewAction.ByKeyboard;
						SelectedNode = ne.CurrentNode;
					}
				}
				break;
			case Keys.Home:
				if (root_node.Nodes.Count > 0) {
					ne = new OpenTreeNodeEnumerator (root_node.Nodes [0]);
					if (ne.MoveNext ()) {
						selection_action = TreeViewAction.ByKeyboard;
						SelectedNode = ne.CurrentNode;
					}
				}
				break;
			case Keys.End:
				if (root_node.Nodes.Count > 0) {
					ne = new OpenTreeNodeEnumerator (root_node.Nodes [0]);
					while (ne.MoveNext ())
					{ }
					selection_action = TreeViewAction.ByKeyboard;
					SelectedNode = ne.CurrentNode;
				}
				break;
			case Keys.PageDown:
				if (selected_node != null) {
					ne = new OpenTreeNodeEnumerator (selected_node);
					int move = VisibleCount;
					for (int i = 0; i < move && ne.MoveNext (); i++) {
						
					}
					selection_action = TreeViewAction.ByKeyboard;
					SelectedNode = ne.CurrentNode;
				}
				break;
			case Keys.PageUp:
				if (selected_node != null) {
					ne = new OpenTreeNodeEnumerator (selected_node);
					int move = VisibleCount;
					for (int i = 0; i < move && ne.MovePrevious (); i++)
					{ }
					selection_action = TreeViewAction.ByKeyboard;
					SelectedNode = ne.CurrentNode;
				}
				break;
			case Keys.Multiply:
				if (selected_node != null)
					selected_node.ExpandAll ();
				break;
			}
			base.OnKeyDown (e);

			if (!e.Handled && checkboxes &&
           		     selected_node != null &&
			    (e.KeyData & Keys.KeyCode) == Keys.Space) {
				selected_node.check_reason = TreeViewAction.ByKeyboard;
				selected_node.Checked = !selected_node.Checked;		
				e.Handled = true;
			}
		}

		protected override void OnKeyPress (KeyPressEventArgs e)
		{
			base.OnKeyPress (e);
			if (e.KeyChar == ' ')
				e.Handled = true;
		}

		protected override void OnKeyUp (KeyEventArgs e)
		{
			base.OnKeyUp (e);
			if ((e.KeyData & Keys.KeyCode) == Keys.Space)
				e.Handled = true;
		}

		protected virtual void OnItemDrag (ItemDragEventArgs e)
		{
			ItemDragEventHandler eh = (ItemDragEventHandler)(Events [ItemDragEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnAfterCheck (TreeViewEventArgs e) {
			TreeViewEventHandler eh = (TreeViewEventHandler)(Events [AfterCheckEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnAfterCollapse (TreeViewEventArgs e) {
			TreeViewEventHandler eh = (TreeViewEventHandler)(Events [AfterCollapseEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnAfterExpand (TreeViewEventArgs e) {
			TreeViewEventHandler eh = (TreeViewEventHandler)(Events [AfterExpandEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnAfterLabelEdit (NodeLabelEditEventArgs e) {
			NodeLabelEditEventHandler eh = (NodeLabelEditEventHandler)(Events [AfterLabelEditEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnAfterSelect (TreeViewEventArgs e) {
			TreeViewEventHandler eh = (TreeViewEventHandler)(Events [AfterSelectEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnBeforeCheck (TreeViewCancelEventArgs e) {
			TreeViewCancelEventHandler eh = (TreeViewCancelEventHandler)(Events [BeforeCheckEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnBeforeCollapse (TreeViewCancelEventArgs e) {
			TreeViewCancelEventHandler eh = (TreeViewCancelEventHandler)(Events [BeforeCollapseEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnBeforeExpand (TreeViewCancelEventArgs e) {
			TreeViewCancelEventHandler eh = (TreeViewCancelEventHandler)(Events [BeforeExpandEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnBeforeLabelEdit (NodeLabelEditEventArgs e) {
			NodeLabelEditEventHandler eh = (NodeLabelEditEventHandler)(Events [BeforeLabelEditEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnBeforeSelect (TreeViewCancelEventArgs e) {
			TreeViewCancelEventHandler eh = (TreeViewCancelEventHandler)(Events [BeforeSelectEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnHandleCreated (EventArgs e) {
			base.OnHandleCreated (e);
		}

		protected override void OnHandleDestroyed (EventArgs e) {
			base.OnHandleDestroyed (e);
		}

		protected override void WndProc(ref Message m) {
			switch ((Msg) m.Msg) {

			case Msg.WM_LBUTTONDBLCLK:
				int val = m.LParam.ToInt32();
				DoubleClickHandler (null, new
						MouseEventArgs (MouseButtons.Left,
								2, val & 0xffff,
								(val>>16) & 0xffff, 0));
					break;
			}
			base.WndProc (ref m);
		}

		#endregion	// Protected Instance Methods

		#region	Internal & Private Methods and Properties
		internal IntPtr CreateNodeHandle ()
		{
			return (IntPtr) handle_count++;
		}

		internal TreeNode NodeFromHandle (IntPtr handle)
		{
			// This method is called rarely, so instead of maintaining a table
			// we just walk the tree nodes to find the matching handle
			return NodeFromHandleRecursive (root_node,  handle);
		}

		private TreeNode NodeFromHandleRecursive (TreeNode node, IntPtr handle)
		{
			if (node.handle == handle)
				return node;
			foreach (TreeNode child in node.Nodes) {
				TreeNode match = NodeFromHandleRecursive (child, handle);
				if (match != null)
					return match;
			}
			return null;
		}

	        internal Rectangle ViewportRectangle {
			get {
				Rectangle res = ClientRectangle;

				if (vbar != null && vbar.Visible)
					res.Width -= vbar.Width;
				if (hbar != null && hbar.Visible)
					res.Height -= hbar.Height;
				return res;
			}
		}

		private TreeNode GetNodeAt (int y)
		{
			if (nodes.Count <= 0)
				return null;

			OpenTreeNodeEnumerator o = new OpenTreeNodeEnumerator (TopNode);
			int move = y / ActualItemHeight;
			for (int i = -1; i < move; i++) {
				if (!o.MoveNext ())
					return null;
			}

			return o.CurrentNode;
		}

		private bool IsTextArea (TreeNode node, int x)
		{
			return node != null && node.Bounds.Left <= x && node.Bounds.Right >= x;
		}

		private bool IsSelectableArea (TreeNode node, int x)
		{
			if (node == null)
				return false;
			int l = node.Bounds.Left;
			if (ImageList != null)
				l -= ImageList.ImageSize.Width;
			return l <= x && node.Bounds.Right >= x;
				
		}

		private bool IsPlusMinusArea (TreeNode node, int x)
		{
			if (node.Nodes.Count == 0 || (node.parent == root_node && !show_root_lines))
				return false;

			int l = node.Bounds.Left + 5;

			if (show_root_lines || node.Parent != null)
				l -= indent;
			if (ImageList != null)
				l -= ImageList.ImageSize.Width + 3;
			if (checkboxes)
				l -= 19;
			return (x > l && x < l + 8);
		}

		private bool IsCheckboxArea (TreeNode node, int x)
		{
			int l = CheckBoxLeft (node);
			return (x > l && x < l + 10);
		}

		private int CheckBoxLeft (TreeNode node)
		{
			int l = node.Bounds.Left + 5;

			if (show_root_lines || node.Parent != null)
				l -= indent;
			if (ImageList != null)
				l -= ImageList.ImageSize.Width + 3;

			return l;
		}

		internal void RecalculateVisibleOrder (TreeNode start)
		{
			if (update_stack > 0)
				return;

			int order;
			if (start == null) {
				start = root_node;
				order = 0;
			} else
				order = start.visible_order;

			
			
			OpenTreeNodeEnumerator walk = new OpenTreeNodeEnumerator (start);
			while (walk.MoveNext ()) {
				walk.CurrentNode.visible_order = order;
				order++;
			}

			max_visible_order = order;
		}

		internal void SetTop (TreeNode node)
		{
			if (!vbar.is_visible)
				return;

			TreeNode first = root_node.FirstNode;

			if (first == null)
				return;  // I don't think its possible for this to happen

			OpenTreeNodeEnumerator walk = new OpenTreeNodeEnumerator (first);
			int offset = -1;
			int vc = VisibleCount;
			while (walk.CurrentNode != node && walk.MoveNext () && offset < vbar.Maximum - vc)
				offset++;

			SetVScrollTop (walk.CurrentNode);
		}

		internal void SetBottom (TreeNode node)
		{
			if (!vbar.is_visible)
				return;

			OpenTreeNodeEnumerator walk = new OpenTreeNodeEnumerator (node);

			int bottom = ViewportRectangle.Bottom;
			int offset = 0;
			while (walk.MovePrevious ()) {
				if (walk.CurrentNode.Bounds.Bottom <= bottom)
					break;
				offset++;
			}

			int nv = vbar.Value + offset;
			if (vbar.Value + offset < vbar.Maximum) {
				vbar.Value = nv;
			} else {
#if DEBUG
				Console.Error.WriteLine ("setting bottom to value greater then maximum ({0}, {1})",
						nv, vbar.Maximum);
#endif
			}
				
		}

		internal void UpdateBelow (TreeNode node)
		{
			if (update_stack > 0) {
				update_needed = true;
				return;
			}

			if (node == root_node) {
				Invalidate (ViewportRectangle);
				return;
			}
				
			// We need to update the current node so the plus/minus block gets update too
			int top = Math.Max (node.Bounds.Top - 1, 0);
			Rectangle invalid = new Rectangle (0, top,
					Width, Height - top);
			Invalidate (invalid);
		}

		internal void UpdateNode (TreeNode node)
		{
			if (node == null)
				return;

			if (update_stack > 0) {
				update_needed = true;
				return;
			}

			if (node == root_node) {
				Invalidate ();
				return;
			}

			Rectangle invalid = new Rectangle (0, node.Bounds.Top - 1, Width,
					node.Bounds.Height + 1);
			Invalidate (invalid);
		}

		internal void UpdateNodePlusMinus (TreeNode node)
		{
			if (update_stack > 0) {
				update_needed = true;
				return;
			}

			int l = node.Bounds.Left + 5;

			if (show_root_lines || node.Parent != null)
				l -= indent;
			if (ImageList != null)
				l -= ImageList.ImageSize.Width + 3;
			if (checkboxes)
				l -= 19;

			Invalidate (new Rectangle (l, node.Bounds.Top, 8, node.Bounds.Height));
		}

		internal override void OnPaintInternal (PaintEventArgs pe)
		{
			Draw (pe.ClipRectangle, pe.Graphics);
		}

		internal void CreateDashPen ()
		{
			dash = new Pen (LineColor, 1);
			dash.DashStyle = DashStyle.Dot;
		}

		private void Draw (Rectangle clip, Graphics dc)
		{
			dc.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), clip);

			if (dash == null)
				CreateDashPen ();

			Rectangle viewport = ViewportRectangle;
			Rectangle original_clip = clip;
			if (clip.Bottom > viewport.Bottom)
				clip.Height = viewport.Bottom - clip.Top;

			OpenTreeNodeEnumerator walk = new OpenTreeNodeEnumerator (TopNode);
			while (walk.MoveNext ()) {
				TreeNode current = walk.CurrentNode;

				// Haven't gotten to visible nodes yet
				if (current.GetY () + ActualItemHeight < clip.Top)
					continue;

				// Past the visible nodes
				if (current.GetY () > clip.Bottom)
					break;

				DrawNode (current, dc, clip);
			}

			if (hbar.Visible && vbar.Visible) {
				Rectangle corner = new Rectangle (hbar.Right, vbar.Bottom, vbar.Width, hbar.Height);
				if (original_clip.IntersectsWith (corner))
					dc.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (ThemeEngine.Current.ColorControl),
							corner);
			}
		}

		private void DrawNodePlusMinus (TreeNode node, Graphics dc, int x, int middle)
		{
			dc.DrawRectangle (SystemPens.ControlDark, x, middle - 4, 8, 8);

			if (node.IsExpanded) {
				dc.DrawLine (SystemPens.ControlDarkDark, x + 2, middle, x + 6, middle); 
			} else {
				dc.DrawLine (SystemPens.ControlDarkDark, x + 2, middle, x + 6, middle);
				dc.DrawLine (SystemPens.ControlDarkDark, x + 4, middle - 2, x + 4, middle + 2);
			}
		}

		private void DrawNodeCheckBox (TreeNode node, Graphics dc, int x, int middle)
		{
			Pen pen = ThemeEngine.Current.ResPool.GetSizedPen(Color.Black, 2);
				dc.DrawRectangle (pen, x + 3, middle - 4, 11, 11);

			if (node.Checked) {
				Pen check_pen = ThemeEngine.Current.ResPool.GetPen(Color.Black);
				
				int check_size = 5;
				int lineWidth = 3;
				
				Rectangle rect = new Rectangle (x + 5, middle - 2, check_size, check_size);
				
				for (int i = 0; i < lineWidth; i++) {
					dc.DrawLine (check_pen, rect.Left + 1, rect.Top + lineWidth + i, rect.Left + 3, rect.Top + 5 + i);
					dc.DrawLine (check_pen, rect.Left + 3, rect.Top + 5 + i, rect.Left + 7, rect.Top + 1 + i);
				}
			}
		}

		private void DrawNodeLines (TreeNode node, Graphics dc, Rectangle clip, Pen dash, int x, int y,	int middle)
		{
			int ladjust = 9;
			int radjust = 0;

			if (node.nodes.Count > 0 && show_plus_minus)
				ladjust = 13;
			if (checkboxes)
				radjust = 3;

			dc.DrawLine (dash, x - indent + ladjust, middle, x + radjust, middle);

			if (node.PrevNode != null || node.Parent != null) {
				ladjust = 9;
				dc.DrawLine (dash, x - indent + ladjust, node.Bounds.Top,
						x - indent + ladjust, middle - (show_plus_minus && node.Nodes.Count > 0 ? 4 : 0));
			}

			if (node.NextNode != null) {
				ladjust = 9;
				dc.DrawLine (dash, x - indent + ladjust, middle + (show_plus_minus && node.Nodes.Count > 0 ? 4 : 0),
						x - indent + ladjust, node.Bounds.Bottom);
				
			}

			ladjust = 0;
			if (show_plus_minus)
				ladjust = 9;
			TreeNode parent = node.Parent;
			while (parent != null) {
				if (parent.NextNode != null) {
					int px = parent.GetLinesX () - indent + ladjust;
					dc.DrawLine (dash, px, node.Bounds.Top, px, node.Bounds.Bottom);
				}
				parent = parent.Parent;
			}
		}

		private void DrawNodeImage (TreeNode node, Graphics dc, Rectangle clip, int x, int y)
		{
			// Rectangle r = new Rectangle (x, y + 2, ImageList.ImageSize.Width, ImageList.ImageSize.Height);

			if (!RectsIntersect (clip, x, y + 2, ImageList.ImageSize.Width, ImageList.ImageSize.Height))
				return;

			if (ImageList == null)
				return;

			int use_index = -1;
			if (node.ImageIndex > -1 && node.ImageIndex < ImageList.Images.Count) {
				use_index = node.ImageIndex;
			} else if (ImageIndex > -1 && ImageIndex < ImageList.Images.Count) {
				use_index = ImageIndex;
			}

			if (use_index == -1 && ImageList.Images.Count > 0) {
				use_index = 0;
			}

			if (use_index != -1) {
				ImageList.Draw (dc, x, y + 2, ImageList.ImageSize.Width, 
						ImageList.ImageSize.Height, use_index);
			}
		}

		private void LabelEditFinished (object sender, EventArgs e)
		{
			EndEdit (edit_node);
		}

		internal void BeginEdit (TreeNode node)
		{
			if (edit_node != null)
				EndEdit (edit_node);

			if (edit_text_box == null) {
				edit_text_box = new LabelEditTextBox ();
				edit_text_box.BorderStyle = BorderStyle.FixedSingle;
				edit_text_box.EditingFinished += new EventHandler (LabelEditFinished);
				Controls.Add (edit_text_box);
			}

			edit_text_box.Bounds = node.Bounds;
			edit_text_box.Width += 4;

			edit_text_box.Text = node.Text;
			edit_text_box.Visible = true;
			edit_text_box.Focus ();
			edit_text_box.SelectAll ();

			edit_args = new NodeLabelEditEventArgs (edit_node);
			OnBeforeLabelEdit (edit_args);

			if (edit_args.CancelEdit)
				EndEdit (node);
			
			edit_node = node;
		}

		internal void EndEdit (TreeNode node)
		{
			if (edit_text_box != null && edit_text_box.Visible) {
				edit_text_box.Visible = false;
			}

			if (edit_node != null && edit_node == node) {
				OnAfterLabelEdit (edit_args);

				if (!edit_args.CancelEdit) {
					if (edit_args.Label != null)
						edit_node.Text = edit_args.Label;
					else
						edit_node.Text = edit_text_box.Text;
				}

			}

			
			edit_node = null;
			UpdateNode (node);
		}

		internal int GetNodeWidth (TreeNode node)
		{
			Font font = node.NodeFont;
			if (node.NodeFont == null)
				font = Font;
			return (int) DeviceContext.MeasureString (node.Text, font, 0, string_format).Width + 3;
		}

		private void DrawSelectionAndFocus(TreeNode node, Graphics dc, Rectangle r)
		{
			if (Focused && focused_node == node) {
				ControlPaint.DrawFocusRectangle (dc, r, ForeColor, BackColor);
			}
			r.Inflate(-1, -1);
			if (Focused && node == highlighted_node) {
				dc.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (ThemeEngine.Current.ColorHighlight), r);
			} else if (!hide_selection && node == highlighted_node) {
				dc.FillRectangle (SystemBrushes.Control, r);
			} else {
				dc.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (node.BackColor), r);
			}
		}
		 
		private void DrawStaticNode (TreeNode node, Graphics dc)
		{
			if (!full_row_select)
				DrawSelectionAndFocus(node, dc, node.Bounds);

			Font font = node.NodeFont;
			if (node.NodeFont == null)
				font = Font;
			Color text_color = (Focused && node == highlighted_node ?
					ThemeEngine.Current.ColorHighlightText : node.ForeColor);
			dc.DrawString (node.Text, font,
					ThemeEngine.Current.ResPool.GetSolidBrush (text_color),
					node.Bounds, string_format);
		}

		private void DrawNode (TreeNode node, Graphics dc, Rectangle clip)
		{
			int child_count = node.nodes.Count;
			int y = node.GetY ();
			int middle = y + (ActualItemHeight / 2);

			if (full_row_select) {
				Rectangle r = new Rectangle (1, y + 2, ViewportRectangle.Width - 2, ActualItemHeight);
				DrawSelectionAndFocus (node, dc, r);
			}

			if ((show_root_lines || node.Parent != null) && show_plus_minus && child_count > 0)
				DrawNodePlusMinus (node, dc, node.GetLinesX () - Indent + 5, middle);

			if (checkboxes)
				DrawNodeCheckBox (node, dc, CheckBoxLeft (node) - 3, middle);

			if (show_lines)
				DrawNodeLines (node, dc, clip, dash, node.GetLinesX (), y, middle);

			if (ImageList != null)
                                DrawNodeImage (node, dc, clip, node.GetImageX (), y);

			if (!node.IsEditing)
				DrawStaticNode (node, dc);
		}

		internal void UpdateScrollBars ()
		{
			if (IsDisposed || update_stack > 0 || !IsHandleCreated || !Visible)
				return;

			bool vert = false;
			bool horz = false;
			int height = -1;
			int width = -1;

			if (scrollable) {
				OpenTreeNodeEnumerator walk = new OpenTreeNodeEnumerator (root_node);
				
				while (walk.MoveNext ()) {
					int r = walk.CurrentNode.Bounds.Right;
					int b = walk.CurrentNode.Bounds.Bottom;

					if (r > width)
						width = r;
					if (b > height)
						height = b;
				}

				// Remove scroll adjustments
				if (nodes.Count > 0)
					height -= nodes [0].Bounds.Top;
				width += hbar_offset;

				if (height > ClientRectangle.Height) {
					vert = true;

					if (width > ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth)
						horz = true;
				} else if (width > ClientRectangle.Width) {
					horz = true;
				}

				if (!vert && horz && height > ClientRectangle.Height - SystemInformation.HorizontalScrollBarHeight)
					vert = true;
			}

			if (vert) {
				int visible_height = horz ? ClientRectangle.Height - hbar.Height : ClientRectangle.Height;
				vbar.SetValues (Math.Max (0, max_visible_order - 2), visible_height / ActualItemHeight);
				/*
				vbar.Maximum = max_visible_order;
				vbar.LargeChange = ClientRectangle.Height / ItemHeight;
				*/

				if (!vbar_bounds_set) {
					vbar.Bounds = new Rectangle (ClientRectangle.Width - vbar.Width, 0, vbar.Width,
							ClientRectangle.Height -
							(horz ? SystemInformation.VerticalScrollBarWidth : 0));
					vbar_bounds_set = true;

					// We need to recalc the hbar if the vbar is now visible
					hbar_bounds_set = false;
				}

				vbar.Visible = true;
			} else {
				skipped_nodes = 0;
				RecalculateVisibleOrder (root_node);
				vbar.Visible = false;
				vbar_bounds_set = false;
			}

			if (horz) {
				hbar.SetValues (width + 1, ClientRectangle.Width - (vert ? SystemInformation.VerticalScrollBarWidth : 0));
				/*
				hbar.LargeChange = ClientRectangle.Width;
				hbar.Maximum = width + 1;
				*/

				if (!hbar_bounds_set) {
					hbar.Bounds = new Rectangle (0, ClientRectangle.Height - hbar.Height,
							ClientRectangle.Width - (vert ? SystemInformation.VerticalScrollBarWidth : 0),
							hbar.Height);
					hbar_bounds_set = true;
				}
				hbar.Visible = true;
			} else {
				hbar_offset = 0;
				hbar.Visible = false;
				hbar_bounds_set = false;
			}
		}

		private void SizeChangedHandler (object sender, EventArgs e)
		{
			if (IsHandleCreated) {
				if (max_visible_order == -1)
					RecalculateVisibleOrder (root_node);
				UpdateScrollBars ();
			}

			if (vbar.Visible) {
				vbar.Bounds = new Rectangle (ClientRectangle.Width - vbar.Width, 0, vbar.Width,
						ClientRectangle.Height - (hbar.Visible ? SystemInformation.HorizontalScrollBarHeight : 0));
			}

			if (hbar.Visible) {
				hbar.Bounds = new Rectangle (0, ClientRectangle.Height - hbar.Height,
						ClientRectangle.Width - (vbar.Visible ? SystemInformation.VerticalScrollBarWidth : 0), hbar.Height);
			}
		}

		private void VScrollBarValueChanged (object sender, EventArgs e)
		{
			EndEdit (edit_node);

			SetVScrollPos (vbar.Value, null);
		}

		private void SetVScrollPos (int pos, TreeNode new_top)
		{
			if (pos < 0)
				pos = 0;

			if (skipped_nodes == pos)
				return;

			OpenTreeNodeEnumerator walk = new OpenTreeNodeEnumerator (TopNode);

			int old_skip = skipped_nodes;
			skipped_nodes = pos;
			int diff = old_skip - skipped_nodes;

			// Determine the new top node if we have to
			if (new_top == null) {

				
				if (diff < 0) {
					for (int i = diff; i <= 0; i++)
						walk.MoveNext ();
					new_top = walk.CurrentNode;
				} else {
					for (int i = 0; i <= diff; i++)
						walk.MovePrevious ();
					new_top = walk.CurrentNode;
				}
			}

			int y_move = diff * ActualItemHeight;
			XplatUI.ScrollWindow (Handle, ViewportRectangle, 0, y_move, false);
		}

		private void SetVScrollTop (TreeNode new_top)
		{
			OpenTreeNodeEnumerator walk = new OpenTreeNodeEnumerator (root_node);

			int skip_nodes = -1;
			while (walk.MoveNext () && walk.CurrentNode != new_top)
				skip_nodes++;

			vbar.Value = skip_nodes;
		}

		private void HScrollBarValueChanged(object sender, EventArgs e)
		{
			EndEdit (edit_node);

			int old_offset = hbar_offset;
			hbar_offset = hbar.Value;

			if (hbar_offset < 0) {
				hbar_offset = 0;
			}

			XplatUI.ScrollWindow (Handle, ViewportRectangle, old_offset - hbar_offset, 0, false);
		}

		internal void ExpandBelow (TreeNode node, int count_to_next)
		{
			if (update_stack > 0) {
				update_needed = true;
				return;
			}

			Rectangle below = new Rectangle (0, node.Bounds.Bottom + 2, ViewportRectangle.Width,
					ViewportRectangle.Height - node.Bounds.Bottom);

			int amount = count_to_next * ActualItemHeight;

			if (amount > 0)
				XplatUI.ScrollWindow (Handle, below, 0, amount, false);

			if (show_plus_minus) {
				//int linesx = node.GetLinesX ();
				Invalidate (new Rectangle (0, node.GetY (), Width, ActualItemHeight));
			}
		}

		internal void CollapseBelow (TreeNode node, int count_to_next)
		{
			if (update_stack > 0) {
				update_needed = true;
				return;
			}

			Rectangle below = new Rectangle (0, node.Bounds.Bottom + 2, ViewportRectangle.Width,
					ViewportRectangle.Height - node.Bounds.Bottom + 2);

			int amount = count_to_next * ActualItemHeight;

			if (amount > 0)
				XplatUI.ScrollWindow (Handle, below, 0, -amount, false);

			if (show_plus_minus) {
				//int linesx = node.GetLinesX ();
				Invalidate (new Rectangle (0, node.GetY (), Width, ActualItemHeight));
			}
		}

		private void MouseWheelHandler(object sender, MouseEventArgs e) {

			if (vbar == null || !vbar.is_visible) {
				return;
			}

			if (e.Delta < 0) {
				vbar.Value = Math.Min(vbar.Value + SystemInformation.MouseWheelScrollLines, vbar.Maximum - VisibleCount + 1);
			} else {
				vbar.Value = Math.Max(0, vbar.Value - SystemInformation.MouseWheelScrollLines);
			}
		}

		private void VisibleChangedHandler (object sender, EventArgs e)
		{
			if (Visible) {
				UpdateScrollBars ();
			}
		}

		private void FontChangedHandler (object sender, EventArgs e)
		{
			if (IsHandleCreated) {
				TreeNode top = TopNode;
				InvalidateNodeWidthRecursive (root_node);

				SetTop (top);
			}
		}

		private void InvalidateNodeWidthRecursive (TreeNode node)
		{
			node.InvalidateWidth ();
			foreach (TreeNode child in node.Nodes) {
				InvalidateNodeWidthRecursive (child);
			}
		}

		private void GotFocusHandler (object sender, EventArgs e)
		{
			if (selected_node == null)
				SelectedNode = TopNode;
			else
				UpdateNode (selected_node);
		}

		private void LostFocusHandler (object sender, EventArgs e)
		{
			UpdateNode (SelectedNode);
		}

		private void MouseDownHandler (object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
				Focus ();

			TreeNode node = GetNodeAt (e.Y);
			if (node == null)
				return;

			if (show_plus_minus && IsPlusMinusArea (node, e.X)) {
				node.Toggle ();
				return;
			} else if (checkboxes && IsCheckboxArea (node, e.X)) {
				node.check_reason = TreeViewAction.ByMouse;
				node.Checked = !node.Checked;
				UpdateNode(node);
				return;
			} else if (IsSelectableArea (node, e.X) || full_row_select) {
				TreeNode old_highlighted = highlighted_node;
				highlighted_node = node;
				if (label_edit && e.Clicks == 1 && highlighted_node == old_highlighted) {
					BeginEdit (node);
				} else if (highlighted_node != focused_node) {
					Size ds = SystemInformation.DragSize;
					mouse_rect.X = e.X - ds.Width;
					mouse_rect.Y = e.Y - ds.Height;
					mouse_rect.Width = ds.Width * 2;
					mouse_rect.Height = ds.Height * 2;

					select_mmove = true;
				}

				Invalidate (highlighted_node.Bounds);
				if (old_highlighted != null)
					Invalidate (Bloat (old_highlighted.Bounds));
			} 
		}

		private void MouseUpHandler (object sender, MouseEventArgs e) {

			drag_begin_x = -1;
			drag_begin_y = -1;

			if (!select_mmove)
				return;

			if (e.Button == MouseButtons.Right) {
				Invalidate (highlighted_node.Bounds);
				highlighted_node = selected_node;
				Invalidate (selected_node.Bounds);
			}
			select_mmove = false;

			TreeViewCancelEventArgs ce = new TreeViewCancelEventArgs (highlighted_node, false, TreeViewAction.ByMouse);
			OnBeforeSelect (ce);

			Rectangle invalid;
			if (!ce.Cancel) {
				TreeNode prev_focused_node = focused_node;
				TreeNode prev_highlighted_node = highlighted_node;
				
				selected_node = highlighted_node;
				focused_node = highlighted_node;
				OnAfterSelect (new TreeViewEventArgs (selected_node, TreeViewAction.ByMouse));

				if (prev_focused_node != null) {
					invalid = Rectangle.Union (Bloat (prev_focused_node.Bounds),
							Bloat (prev_highlighted_node.Bounds));
				} else {
					invalid = Bloat (prev_highlighted_node.Bounds);
				}

				Invalidate (invalid);
			} else {
				highlighted_node = focused_node;
				selected_node = focused_node;
			}
		}

		private void MouseMoveHandler (object sender, MouseEventArgs e) {

			if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) {
				if (drag_begin_x == -1 && drag_begin_y == -1) {
					drag_begin_x = e.X;
					drag_begin_y = e.Y;
				} else {
					double rise = Math.Pow (drag_begin_x - e.X, 2);
					double run = Math.Pow (drag_begin_y - e.Y, 2);
					double move = Math.Sqrt (rise + run);
					if (move > 3) {
						TreeNode drag = GetNodeAtUseX (e.X, e.Y);
						
						if (drag != null) {
							OnItemDrag (new ItemDragEventArgs (e.Button, drag));
						}
						drag_begin_x = -1;
						drag_begin_y = -1;
					}
				}
				
			}

			// If there is enough movement before the mouse comes up,
			// selection is reverted back to the originally selected node
			if (!select_mmove || mouse_rect.Contains (e.X, e.Y))
				return;

			Invalidate (highlighted_node.Bounds);
			Invalidate (selected_node.Bounds);
			Invalidate (focused_node.Bounds);

			highlighted_node = selected_node;
			focused_node = selected_node;

			select_mmove = false;
		}

		private void DoubleClickHandler (object sender, MouseEventArgs e) {
			TreeNode node = GetNodeAtUseX (e.X,e.Y);
			if(node != null) {
				node.Toggle();
			}
		}

		
		private bool RectsIntersect (Rectangle r, int left, int top, int width, int height)
		{
			return !((r.Left > left + width) || (r.Right < left) ||
					(r.Top > top + height) || (r.Bottom < top));
		}

		#endregion	// Internal & Private Methods and Properties

		#region Events
		static object ItemDragEvent = new object ();
		static object AfterCheckEvent = new object ();
		static object AfterCollapseEvent = new object ();
		static object AfterExpandEvent = new object ();
		static object AfterLabelEditEvent = new object ();
		static object AfterSelectEvent = new object ();
		static object BeforeCheckEvent = new object ();
		static object BeforeCollapseEvent = new object ();
		static object BeforeExpandEvent = new object ();
		static object BeforeLabelEditEvent = new object ();
		static object BeforeSelectEvent = new object ();

		public event ItemDragEventHandler ItemDrag {
			add { Events.AddHandler (ItemDragEvent, value); }
			remove { Events.RemoveHandler (ItemDragEvent, value); }
		}

		public event TreeViewEventHandler AfterCheck {
			add { Events.AddHandler (AfterCheckEvent, value); }
			remove { Events.RemoveHandler (AfterCheckEvent, value); }
		}

		public event TreeViewEventHandler AfterCollapse {
			add { Events.AddHandler (AfterCollapseEvent, value); }
			remove { Events.RemoveHandler (AfterCollapseEvent, value); }
		}

		public event TreeViewEventHandler AfterExpand {
			add { Events.AddHandler (AfterExpandEvent, value); }
			remove { Events.RemoveHandler (AfterExpandEvent, value); }
		}

		public event NodeLabelEditEventHandler AfterLabelEdit {
			add { Events.AddHandler (AfterLabelEditEvent, value); }
			remove { Events.RemoveHandler (AfterLabelEditEvent, value); }
		}

		public event TreeViewEventHandler AfterSelect {
			add { Events.AddHandler (AfterSelectEvent, value); }
			remove { Events.RemoveHandler (AfterSelectEvent, value); }
		}

		public event TreeViewCancelEventHandler BeforeCheck {
			add { Events.AddHandler (BeforeCheckEvent, value); }
			remove { Events.RemoveHandler (BeforeCheckEvent, value); }
		}

		public event TreeViewCancelEventHandler BeforeCollapse {
			add { Events.AddHandler (BeforeCollapseEvent, value); }
			remove { Events.RemoveHandler (BeforeCollapseEvent, value); }
		}

		public event TreeViewCancelEventHandler BeforeExpand {
			add { Events.AddHandler (BeforeExpandEvent, value); }
			remove { Events.RemoveHandler (BeforeExpandEvent, value); }
		}

		public event NodeLabelEditEventHandler BeforeLabelEdit {
			add { Events.AddHandler (BeforeLabelEditEvent, value); }
			remove { Events.RemoveHandler (BeforeLabelEditEvent, value); }
		}

		public event TreeViewCancelEventHandler BeforeSelect {
			add { Events.AddHandler (BeforeSelectEvent, value); }
			remove { Events.RemoveHandler (BeforeSelectEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]	
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	
		[Browsable (false)]
		public new event PaintEventHandler Paint {
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	
		[Browsable (false)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}
		#endregion	// Events
	}
}

