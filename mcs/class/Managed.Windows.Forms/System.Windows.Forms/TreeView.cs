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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)


using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;

namespace System.Windows.Forms {

	public class TreeView : Control {

		private string path_separator = "\\";
		private int item_height = -1;
		private bool sorted;
		private TreeNode top_node;
		internal TreeNode root_node;
		private TreeNodeCollection nodes;
		private int total_node_count;

		private ImageList image_list;
		private int image_index = -1;
		private int selected_image_index = -1;

		private bool full_row_select;
		private bool hot_tracking;
		private int indent = 19;

		private bool checkboxes;
		private bool label_edit;
		private bool scrollable;
		private bool show_lines = true;
		private bool show_root_lines = true;
		private bool show_plus_minus = true;

		private int update_stack;

		private TreeViewEventHandler on_after_check;
		private TreeViewEventHandler on_after_collapse;
		private TreeViewEventHandler on_after_expand;
		private NodeLabelEditEventHandler on_after_label_edit;
		private TreeViewEventHandler on_after_select;
		private TreeViewCancelEventHandler on_before_check;
		private TreeViewCancelEventHandler on_before_collapse;
		private TreeViewCancelEventHandler on_before_expand;
		private NodeLabelEditEventHandler on_before_label_edit;
		private TreeViewCancelEventHandler on_before_select;

		private Pen dash;

		public TreeView ()
		{
			root_node = new TreeNode (this);
			root_node.Text = "ROOT NODE";
			nodes = new TreeNodeCollection (root_node);
			root_node.SetNodes (nodes);

			MouseDown += new MouseEventHandler (MouseDownHandler);
			SizeChanged += new EventHandler (SizeChangedHandler);

			SetStyle (ControlStyles.AllPaintingInWmPaint, true);
			SetStyle (ControlStyles.UserPaint, true);

			dash = new Pen (SystemColors.ControlLight, 1);
		}

		public string PathSeparator {
			get { return path_separator; }
			set { path_separator = value; }
		}

		public bool Sorted {
			get { return sorted; }
			set {
				if (sorted != value)
					sorted = value;
				if (sorted)
					Nodes.Sort ();
			}
		}

		public TreeNode TopNode {
			get { return top_node; }
		}

		public TreeNodeCollection Nodes {
			get { return nodes; }
		}

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
				Refresh ();
			}
		}

		public int VisibleCount {
			get {
				return ClientRectangle.Height / ItemHeight;
			}
		}

		[MonoTODO ("Anything special need to be done here?")]
		public ImageList ImageList {
			get { return image_list; }
			set { image_list = value; }
		}

		public int ImageIndex {
			get { return image_index; }
			set {
				if (value < 0) {
					throw new ArgumentException ("'" + value + "' is not a valid value for 'value'. " +
						"'value' must be greater than or equal to 0.");
				}
				image_index = value;
			}
		}

		public int SelectedImageIndex {
			get { return selected_image_index; }
			set {
				if (value < 0) {
					throw new ArgumentException ("'" + value + "' is not a valid value for 'value'. " +
						"'value' must be greater than or equal to 0.");
				}
			}
		}

		public override Color BackColor {
			get { return SystemColors.Window; }
			set { base.BackColor = value; }
		}

		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

		/*
		   Commented out until this is implemented in Control
		public override BorderStyle BorderStyle {
			get { return base.BorderStyle; }
			set { base.BorderStyle = value; }
		}
		*/

		public override Color ForeColor {
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}

		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		public bool FullRowSelect {
			get { return full_row_select; }
			set {
				if (value == full_row_select)
					return;
				full_row_select = value;
				Refresh ();
			}
		}

		public bool HotTracking {
			get { return hot_tracking; }
			set { hot_tracking = value; }
		}

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
				Refresh ();
			}
		}

		public bool LabelEdit {
			get { return label_edit; }
			set { label_edit = value; }
		}

		public bool Scrollable {
			get { return scrollable; }
			set {
				if (scrollable == value)
					return;
				scrollable = value;
			}
		}

		public bool ShowLines {
			get { return show_lines; }
			set {
				if (show_lines == value)
					return;
				show_lines = value;
				Refresh ();
			}
		}

		public bool ShowRootLines {
			get { return show_root_lines; }
			set {
				if (show_root_lines == value)
					return;
				show_root_lines = value;
				Refresh ();
			}
		}

		public bool CheckBoxes {
			get { return checkboxes; }
			set {
				if (value == checkboxes)
					return;
				checkboxes = value;
				Refresh ();
			}
		}

		public bool ShowPlusMinus {
			get { return show_plus_minus; }
			set {
				if (show_plus_minus == value)
					return;
				show_plus_minus = value;
				Refresh ();
			}
		}

		[MonoTODO ("Anything extra needed here")]
		protected override CreateParams CreateParams {
			get {
				CreateParams cp = base.CreateParams;
				return cp;
			}
		}

		
		protected override Size DefaultSize {
			get { return new Size (121, 97); }
		}

		internal int TotalNodeCount {
			get { return total_node_count; }
			set { total_node_count = value; }
		}

		protected override void CreateHandle ()
		{
			base.CreateHandle ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (image_list != null)
					image_list.Dispose ();
			}
			base.Dispose (disposing);
		}

		[MonoTODO ("What does the state effect?")]
		protected OwnerDrawPropertyBag GetItemRenderStyles (TreeNode node, int state)
		{
			return node.prop_bag;
		}

		[MonoTODO ("Need to know if we are editing, not if editing is enabled")]
		protected override bool IsInputKey (Keys key_data)
		{
			if (label_edit && (key_data & Keys.Alt) == 0)
			{
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

		protected virtual void OnAfterCheck (TreeViewEventArgs e)
		{
			if (on_after_check != null)
				on_after_check (this, e);
		}

		protected internal virtual void OnAfterCollapse (TreeViewEventArgs e)
		{
			if (on_after_collapse != null)
				on_after_collapse (this, e);
		}

		protected internal virtual void OnAfterExpand (TreeViewEventArgs e)
		{
			if (on_after_expand != null)
				on_after_expand (this, e);
		}

		protected virtual void OnAfterLabelEdit (NodeLabelEditEventArgs e)
		{
			if (on_after_label_edit != null)
				on_after_label_edit (this, e);
		}

		protected virtual void OnAfterSelect (TreeViewEventArgs e)
		{
			if (on_after_select != null)
				on_after_select (this, e);
		}

		protected virtual void OnBeforeCheck (TreeViewCancelEventArgs e)
		{
			if (on_before_check != null)
				on_before_check (this, e);
		}

		protected internal virtual void OnBeforeCollapse (TreeViewCancelEventArgs e)
		{
			if (on_before_collapse != null)
				on_before_collapse (this, e);
		}

		protected internal virtual void OnBeforeExpand (TreeViewCancelEventArgs e)
		{
			if (on_before_expand != null)
				on_before_expand (this, e);
		}

		protected virtual void OnBeforeLabelEdit (NodeLabelEditEventArgs e)
		{
			if (on_before_label_edit != null)
				on_before_label_edit (this, e);
		}

		protected virtual void OnBeforeSelect (TreeViewCancelEventArgs e)
		{
			if (on_before_select != null)
				on_before_select (this, e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		public void BeginUpdate ()
		{
			if (!IsHandleCreated)
				return;
			update_stack++;
		}

		public void EndUpdate ()
		{
			if (!IsHandleCreated)
				return;

			if (update_stack > 1) {
				update_stack--;
			} else {
				update_stack = 0;
				Refresh ();
			}
		}

		public void ExpandAll ()
		{
			root_node.ExpandAll ();
		}

		public void CollapseAll ()
		{
			root_node.CollapseAll ();
		}

		public TreeNode GetNodeAt (Point pt)
		{
			return GetNodeAt (pt.X, pt.Y);
		}

		public TreeNode GetNodeAt (int x, int y)
		{
			if (top_node == null)
				top_node = nodes [0];

			OpenTreeNodeEnumerator o = new OpenTreeNodeEnumerator (TopNode);
			int move = y / ItemHeight;

			for (int i = 0; i < move; i++) {
				if (!o.MoveNext ())
					return null;
			}

			// Make sure it is in the horizontal bounding box
			if (o.CurrentNode.Bounds.Left > x && o.CurrentNode.Bounds.Right < x)
				return o.CurrentNode;

			return null;
		}

		public int GetNodeCount (bool include_subtrees)
		{
			return root_node.GetNodeCount (include_subtrees);
		}

		public override string ToString ()
		{
			int count = Nodes.Count;
			if (count < 0)
				return String.Concat (base.ToString (), "Node Count: 0");
			return String.Concat (base.ToString (), "Node Count: ", count, " Nodes[0]: ", Nodes [0]);
						
		}

		protected override void WndProc(ref Message m)
		{
			switch ((Msg) m.Msg) {
			case Msg.WM_PAINT: {				
				PaintEventArgs	paint_event;

				paint_event = XplatUI.PaintEventStart (Handle);
				DoPaint (paint_event);
				XplatUI.PaintEventEnd (Handle);
				return;
			}
			case Msg.WM_LBUTTONDBLCLK:
				Console.WriteLine ("double click");
				break;
			}
			base.WndProc (ref m);
		}

		internal void UpdateBelow (TreeNode node)
		{
			// Invalidate all these nodes and the nodes below it
		}

		private void DoPaint (PaintEventArgs pe)
		{
			if (Width <= 0 || Height <=  0 || Visible == false)
				return;

			Draw();
			pe.Graphics.DrawImage (ImageBuffer, 0, 0);
		}

		private bool add_hscroll;
		private bool add_vscroll;
		private int max_node_width;
		
		private void Draw ()
		{
			DateTime start = DateTime.Now;
			if (top_node == null && Nodes.Count > 0)
				top_node = nodes [0];
			// Decide if we need a scrollbar
			int visible_node_count = GetVisibleNodeCount ();
			Console.WriteLine ("time to get visible node count:  " + (DateTime.Now - start));
			int node_count = 0;

			Rectangle fill = ClientRectangle;
			Rectangle vclip = Rectangle.Empty;

			add_vscroll = false;
			add_hscroll = false;

			if ((visible_node_count * ItemHeight) > ClientRectangle.Height) {
				add_vscroll = true;
				if (vbar == null)
					vbar = new VScrollBar ();
				vclip = new Rectangle (ClientRectangle.Width - vbar.Width, 0, vbar.Width, Height);
				fill.Width -= vbar.Width;
				DeviceContext.ExcludeClip (vclip);
			}

			DeviceContext.FillRectangle (new SolidBrush (Color.White), fill);
			
			int depth = 0;
			int item_height = ItemHeight;
			Font font = Font;
			int height = ClientRectangle.Height;

			foreach (TreeNode node in nodes) {
				DrawNode (node, ref depth, ref node_count, item_height,
						font, ref visible_node_count, height);
				depth = 0;
			}

			if (max_node_width > ClientRectangle.Width) {
				add_hscroll = true;
				AddHorizontalScrollBar ();
			}

			if (add_vscroll)
				AddVerticalScrollBar (node_count, vclip);

			if (add_hscroll && add_vscroll) {
				Rectangle grip = new Rectangle (hbar.Right, vbar.Bottom, vbar.Width, hbar.Height);
				DeviceContext.FillRectangle (new SolidBrush (BackColor), grip);
				ControlPaint.DrawSizeGrip (DeviceContext, BackColor, grip);
			}
			
			/*
			ControlPaint.DrawBorder3D (DeviceContext, ClientRectangle, Border3DStyle.Sunken,
				Border3DSide.Left | Border3DSide.Right | Border3DSide.Top | Border3DSide.Bottom);
			*/

			/*
			int depth = 0;
			foreach (TreeNode node in nodes) {
				DumpNode (node, ref depth);
				depth = 0;
			}

			*/
			Console.WriteLine ("treeview drawing time:  " + (DateTime.Now - start));
			Console.WriteLine ("node count:		    " + node_count);
			Console.WriteLine ("total node count:	    " + total_node_count);
		}

		private void DumpNode (TreeNode node, ref int depth)
		{
			for (int i = 0; i < depth; i++)
				Console.Write ("****");
			Console.WriteLine (node.Text);

			if (node.PrevNode != null)
				Console.WriteLine (" -- " + node.PrevNode.Text);
			depth++;
			foreach (TreeNode child in node.Nodes) {
				DumpNode (child, ref depth);
			}
			depth--;
			
		}

		private void DrawNodePlusMinus (TreeNode node, int x, int y, int middle)
		{
			DeviceContext.DrawRectangle (SystemPens.ControlDark, x, middle - 4, 8, 8);

			if (node.IsExpanded) {
				DeviceContext.DrawLine (SystemPens.ControlDarkDark, x + 2, middle, x + 6, middle); 
			} else {
				DeviceContext.DrawLine (SystemPens.ControlDarkDark, x + 2, middle, x + 6, middle);
				DeviceContext.DrawLine (SystemPens.ControlDarkDark, x + 4, y + 6, x + 4, y + 10);
			}
		}

		private void DrawNodeLines (TreeNode node, Pen dash, int x, int y, int middle, int item_height, int node_count)
		{
			int xadjust = 9;
			if (node_count > 0 && show_plus_minus)
				xadjust = 13;
			DeviceContext.DrawLine (dash, x - indent + xadjust, middle, x, middle);

			int ly = 0;
			if (node.PrevNode != null) {
				int prevadjust = (node.Nodes.Count > 0 && show_plus_minus ? 4 : 0);
				int myadjust = (node.Nodes.Count > 0 && show_plus_minus ? 4 : 0);
				ly = node.PrevNode.Bounds.Bottom - (item_height / 2) + prevadjust;
				DeviceContext.DrawLine (dash, x - indent + 9, middle - myadjust, x - indent + 9, ly);
			} else if (node.Parent != null) {
				int myadjust = (node.Nodes.Count > 0 && show_plus_minus ? 4 : 0);
				ly = node.Parent.Bounds.Bottom;
				DeviceContext.DrawLine (dash, x - indent + 9, middle - myadjust, x - indent + 9, ly);
			}
		}

		private void DrawNodeImage (TreeNode node, int x, int y)
		{
			if (node.ImageIndex > -1 && ImageList != null && node.ImageIndex < ImageList.Images.Count) {
				ImageList.Draw (DeviceContext, x, y + 2, ImageList.ImageSize.Width, 
						ImageList.ImageSize.Height, image_index);
			} else if (ImageIndex > -1 && ImageList != null && ImageIndex < ImageList.Images.Count) {
				ImageList.Draw (DeviceContext, x, y + 2, ImageList.ImageSize.Width, 
						ImageList.ImageSize.Height, ImageIndex);
			}
		}

		private void UpdateNodeBounds (TreeNode node, int x, int y, int item_height)
		{
			int width = (int) (node.Text.Length * Font.Size);
			int xoff = indent;

			if (!show_root_lines && node.Parent == null)
				xoff = 0;

			if (image_list != null)
				xoff += image_list.ImageSize.Width;
			node.UpdateBounds (x + xoff, y, width, item_height);
		}

		private void DrawNode (TreeNode node, ref int depth, ref int node_count, int item_height,
				Font font, ref int visible_node_count, int max_height)
		{
			node_count++;
			int x = (!show_root_lines && node.Parent != null ? depth  - 1 : depth) * indent;
			int y = (item_height + 1) * (node_count - skipped_nodes - 1);
			bool visible = (y >= 0 && y < max_height);

			if (visible)
				visible_node_count++;

			int _n_count = node.nodes.Count;
			int middle = y + (item_height / 2);

			UpdateNodeBounds (node, x, y, item_height);

			if (show_root_lines || node.Parent != null) {
				x += 5;
				if (_n_count > 0) {
					if (show_plus_minus && visible) {
						DrawNodePlusMinus (node, x, y, middle);
					}
				}
				x += indent - 5; 
			}

			if (show_lines)
				DrawNodeLines (node, dash, x, y, middle, item_height, _n_count);

			int ox = x;
			if (ImageList != null) {
				if (visible)
					DrawNodeImage (node, x, y);
				// MS leaves the space for the image if the ImageList is
				// non null regardless of whether or not an image is drawn
				ox += ImageList.ImageSize.Width + 3; // leave a little space so the text isn't against the image
			}

			
			if (visible) {
				DeviceContext.DrawString (node.Text, font, new SolidBrush (Color.Black), ox, y + 2);
				y += item_height + 1;
			}

			if (node.Bounds.Right > max_node_width)
				max_node_width = node.Bounds.Right;

			depth++;
			if (node.IsExpanded) {
				for (int i = 0; i < _n_count; i++) {
					int tdepth = depth;
					DrawNode (node.nodes [i], ref tdepth, ref node_count, item_height,
							font, ref visible_node_count, max_height);
				}
			}

		}

		VScrollBar vbar;
		bool vbar_added;
		int skipped_nodes;
		
		private void AddVerticalScrollBar (int total_nodes, Rectangle bounds)
		{
			vbar.Maximum = total_nodes;
			int height = ClientRectangle.Height;

			vbar.LargeChange = height / ItemHeight;

			if (add_hscroll)
				bounds.Height -= hbar.Height;

			vbar.Bounds = bounds;

			if (!vbar_added) {
				Controls.Add (vbar);
				vbar.ValueChanged += new EventHandler (VScrollBarValueChanged);
				vbar_added = true;
			}
		}

		HScrollBar hbar;
		bool hbar_added;
		int hbar_offset;

		private void AddHorizontalScrollBar ()
		{
			if (hbar == null) {
				hbar = new HScrollBar ();
			}

			hbar.Bounds = new Rectangle (ClientRectangle.Left, ClientRectangle.Bottom - hbar.Height,
					(add_vscroll ? Width - vbar.Width : Width), hbar.Height);

			if (!hbar_added) {
				Controls.Add (hbar);
				hbar_added = true;
			}
		}

		private void SizeChangedHandler (object sender, EventArgs e)
		{
			
			if (max_node_width > ClientRectangle.Width) {
				add_hscroll = true;
				AddHorizontalScrollBar ();
			}

			if (vbar != null) {
				vbar.Left = Right - vbar.Width;
				vbar.Height = Height;
			}

			if (hbar != null) {
				hbar.Top = Bottom - hbar.Height;
				hbar.Width = Width;
			}
		}

		private void VScrollBarValueChanged (object sender, EventArgs e)
		{
			skipped_nodes = vbar.Value;
			Refresh ();
		}

		private int GetVisibleNodeCount ()
		{

			if (Nodes.Count < 1)
				return 0;

			OpenTreeNodeEnumerator e = new OpenTreeNodeEnumerator (root_node.Nodes [0]);

			int count = 0;
			while (e.MoveNext ()) {
				count++;
			}

			return count;
		}

		// TODO: Handle all sorts o stuff here
		private void MouseDownHandler (object sender, MouseEventArgs e)
		{
			if (!show_plus_minus)
				return;

			OpenTreeNodeEnumerator walk = new OpenTreeNodeEnumerator (root_node);

			// TODO: So much optimization potential here
			int half_height = ItemHeight / 2;
			while (walk.MoveNext ()) {
				TreeNode node = (TreeNode) walk.Current;
				Rectangle pm = new Rectangle (node.Bounds.Left - indent - 9, node.Bounds.Top + half_height, 8, 8);
				if (pm.Contains (e.X, e.Y)) {
					Console.WriteLine ("toggling node:  " + node);
					node.Toggle ();
					Console.WriteLine ("node:   " + node.IsExpanded);
					break;
				}
			}
		}

		public event TreeViewEventHandler AfterCheck {
			add { on_after_check += value; }
			remove { on_after_check -= value; }
		}

		public event TreeViewEventHandler AfterCollapse {
			add { on_after_collapse += value; }
			remove { on_after_collapse -= value; }
		}

		public event TreeViewEventHandler AfterExpand {
			add { on_after_expand += value; }
			remove { on_after_expand -= value; }
		}

		public event NodeLabelEditEventHandler AfterLabelEdit {
			add { on_after_label_edit += value; }
			remove { on_after_label_edit -= value; }
		}

		public event TreeViewEventHandler AfterSelect {
			add { on_after_select += value; }
			remove { on_after_select -= value; }
		}

		public event TreeViewCancelEventHandler BeforeCheck {
			add { on_before_check += value; }
			remove { on_before_check -= value; }
		}

		public event TreeViewCancelEventHandler BeforeCollapse {
			add { on_before_collapse += value; }
			remove { on_before_collapse -= value; }
		}

		public event TreeViewCancelEventHandler BeforeExpand {
			add { on_before_expand += value; }
			remove { on_before_expand -= value; }
		}

		public event NodeLabelEditEventHandler BeforeLabelEdit {
			add { on_before_label_edit += value; }
			remove { on_before_label_edit -= value; }
		}

		public event TreeViewCancelEventHandler BeforeSelect {
			add { on_before_select += value; }
			remove { on_before_select -= value; }
		}

		public new event PaintEventHandler Paint {
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}

		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}
	}
}

