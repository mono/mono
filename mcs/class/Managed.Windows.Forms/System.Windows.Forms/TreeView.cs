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

		private TreeNode selected_node = null;
		private TreeNode focused_node = null;
		private bool select_mmove = false;

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
		private bool hide_selection = true;

		private bool add_hscroll;
		private bool add_vscroll;
		private int max_node_width;
		private VScrollBar vbar;
		private bool vbar_added;
		private int skipped_nodes;
		private HScrollBar hbar;
		private bool hbar_added;
		private int hbar_offset;

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
		private int open_node_count = -1;
		
		public TreeView ()
		{
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

			SetStyle (ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
			SetStyle (ControlStyles.UserPaint | ControlStyles.Selectable, true);

			dash = new Pen (SystemColors.ControlLight, 1);
		}

		public string PathSeparator {
			get { return path_separator; }
			set { path_separator = value; }
		}

		public bool HideSelection {
			get { return hide_selection; }
			set {
				if (hide_selection == value)
					return;
				hide_selection = value;
				this.Refresh ();
			}
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
				if (value < -1) {
					throw new ArgumentException ("'" + value + "' is not a valid value for 'value'. " +
						"'value' must be greater than or equal to 0.");
				}
				image_index = value;
			}
		}

		public int SelectedImageIndex {
			get { return selected_image_index; }
			set {
				if (value < -1) {
					throw new ArgumentException ("'" + value + "' is not a valid value for 'value'. " +
						"'value' must be greater than or equal to 0.");
				}
			}
		}

		public TreeNode SelectedNode {
			get { return selected_node; }
			set {
				if (selected_node == value)
					return;

				TreeViewCancelEventArgs e = new TreeViewCancelEventArgs (value, false, TreeViewAction.Unknown);
				OnBeforeSelect (e);

				if (e.Cancel)
					return;

				selected_node = value;
				focused_node = value;
				Refresh ();
				
				OnAfterSelect (new TreeViewEventArgs (value, TreeViewAction.Unknown));
			}
		}

		public override Color BackColor {
			get { return base.BackColor;}
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

				// Match a "bug" in the MS implementation where disabling checkboxes
				// collapses the entire tree, but enabling them does not affect the
				// state of the tree.
				if (!checkboxes)
					root_node.CollapseAllUncheck ();

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
			TreeNode node = GetNodeAt (y);
			if (node == null || !IsTextArea (node, x))
				return null;
			return node;
					
		}

		private TreeNode GetNodeAt (int y)
		{

			if (top_node == null)
				top_node = nodes [0];

			OpenTreeNodeEnumerator o = new OpenTreeNodeEnumerator (TopNode);
			int move = y / ItemHeight + skipped_nodes;

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
				int val = m.LParam.ToInt32();
				DoubleClickHandler (null, new MouseEventArgs (MouseButtons.Left, 2, val & 0xffff, (val>>16) & 0xffff, 0));
				break;
			}
			base.WndProc (ref m);
		}

		// TODO: Update from supplied node down
		internal void UpdateBelow (TreeNode node)
		{
			// We need to update the current node so the plus/minus block gets update too
			Rectangle invalid = new Rectangle (0, node.Bounds.Top, Width, Height - node.Bounds.Top);
			Invalidate (invalid);
		}

		internal void UpdateNode (TreeNode node)
		{
			Rectangle invalid = new Rectangle (0, node.Bounds.Top, Width, node.Bounds.Height);
			Invalidate (invalid);
		}

		int draw_count = 0;

		private void DoPaint (PaintEventArgs pe)
		{
			if (Width <= 0 || Height <=  0 || Visible == false)
				return;

			Draw (pe.ClipRectangle);
			pe.Graphics.DrawImage (ImageBuffer, pe.ClipRectangle, pe.ClipRectangle, GraphicsUnit.Pixel);
		}
		
		private void Draw (Rectangle clip)
		{
			DateTime start = DateTime.Now;
			if (top_node == null && Nodes.Count > 0)
				top_node = nodes [0];
			// Decide if we need a scrollbar
			int old_open_node_count = open_node_count;
			open_node_count = GetOpenNodeCount ();
			int node_count = 0;

			Rectangle fill = ClientRectangle;
			add_vscroll = false;
			add_hscroll = false;
			
			add_vscroll = (open_node_count * ItemHeight) > ClientRectangle.Height;
			
			DeviceContext.FillRectangle (new SolidBrush (BackColor), fill);

			int depth = 0;
			int item_height = ItemHeight;
			Font font = Font;
			int height = ClientRectangle.Height;

			int visible_node_count = 0;
			foreach (TreeNode node in nodes) {
				DrawNode (node, clip, ref depth, ref node_count, item_height,
						font, ref visible_node_count, height);
				depth = 0;
			}

			if (max_node_width > ClientRectangle.Width)
				add_hscroll = true;

			if (add_vscroll)
				add_hscroll = max_node_width > ClientRectangle.Width - ThemeEngine.Current.VScrollBarDefaultSize.Width;
			if (add_hscroll)
				add_vscroll = (open_node_count * ItemHeight) > ClientRectangle.Height - ThemeEngine.Current.HScrollBarDefaultSize.Width;

			if (add_hscroll) {
				AddHorizontalScrollBar ();
			} else if (hbar != null) {
				hbar_offset = 0;
				hbar.Visible = false;
			}

			if (add_vscroll) {
				AddVerticalScrollBar (open_node_count, old_open_node_count != open_node_count);
			} else if (vbar != null) {
				vbar.Visible = false;
				skipped_nodes = 0;
			}

			if (add_hscroll && add_vscroll) {
				Rectangle grip = new Rectangle (hbar.Right, vbar.Bottom, vbar.Width, hbar.Height);
				DeviceContext.FillRectangle (new SolidBrush (ThemeEngine.Current.ColorButtonFace), grip);
				ControlPaint.DrawSizeGrip (DeviceContext, ThemeEngine.Current.ColorButtonFace, grip);
			}

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
			node.UpdatePlusMinusBounds (x, middle - 4, 8, 8);

			DeviceContext.DrawRectangle (SystemPens.ControlDark, node.PlusMinusBounds);

			if (node.IsExpanded) {
				DeviceContext.DrawLine (SystemPens.ControlDarkDark, x + 2, middle, x + 6, middle); 
			} else {
				DeviceContext.DrawLine (SystemPens.ControlDarkDark, x + 2, middle, x + 6, middle);
				DeviceContext.DrawLine (SystemPens.ControlDarkDark, x + 4, middle - 2, x + 4, middle + 2);
			}
		}

		private void DrawNodeCheckBox (TreeNode node, int x, int y)
		{
			int offset = (ItemHeight - 13);

			node.UpdateCheckBoxBounds (x + 3, y + offset, 10, 10);

			DeviceContext.DrawRectangle (new Pen (Color.Black, 2), x + 0.5F + 3, y + 0.5F + offset, 11, 11);

			if (node.Checked) {
				Pen check_pen = new Pen (Color.Black, 1);

				DeviceContext.DrawLine (check_pen, x + 6, y + offset + 5, x + 8, y + offset + 8);
				DeviceContext.DrawLine (check_pen, x + 6, y + offset + 6, x + 8, y + offset + 9);

				DeviceContext.DrawLine (check_pen, x + 7, y + offset + 8, x + 13, y + offset + 3);
				DeviceContext.DrawLine (check_pen, x + 7, y + offset + 9, x + 13, y + offset + 4);
			}
		}

		private void DrawNodeLines (TreeNode node, Pen dash, int x, int y, int middle, int item_height, int node_count)
		{
			int ladjust = 9; // left adjust
			int radjust = 0; // right adjust

			if (node_count > 0 && show_plus_minus)
				ladjust = 13;
			if (checkboxes)
				radjust = 3;
			
			DeviceContext.DrawLine (dash, x - indent + ladjust, middle, x + radjust, middle);

			int ly = 0;
			if (node.PrevNode != null) {
				int prevadjust = (node.Nodes.Count > 0 && show_plus_minus ? (node.PrevNode.Nodes.Count == 0 ? 0 : 4) :
						(node.PrevNode.Nodes.Count == 0 ? 0 : 4));
				int myadjust = (node.Nodes.Count > 0 && show_plus_minus ? 4 : 0);
				ly = node.PrevNode.Bounds.Bottom - (item_height / 2) + prevadjust;
				DeviceContext.DrawLine (dash, x - indent + 9, middle - myadjust, x - indent + 9, ly);
			} else if (node.Parent != null) {
				int myadjust = (node.Nodes.Count > 0 && show_plus_minus ? 4 : 0);
				ly = node.Parent.Bounds.Bottom - 1;
				DeviceContext.DrawLine (dash, x - indent + 9, middle - myadjust, x - indent + 9, ly);
			}
		}

		private void DrawNodeImage (TreeNode node, int x, int y)
		{
			if (node.ImageIndex > -1 && ImageList != null && node.ImageIndex < ImageList.Images.Count) {
				ImageList.Draw (DeviceContext, x, y + 2, ImageList.ImageSize.Width, 
						ImageList.ImageSize.Height, node.ImageIndex);
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

		private void DrawNode (TreeNode node, Rectangle clip, ref int depth, ref int node_count, int item_height,
				Font font, ref int visible_node_count, int max_height)
		{
			node_count++;
			int x = (!show_root_lines && node.Parent != null ? depth  - 1 : depth) * indent - hbar_offset;
			int y = item_height * (node_count - skipped_nodes - 1);
			bool visible = (y >= 0 && y < max_height);

			if (visible)
				visible_node_count++;

			// The thing is totally out of the clipping rectangle
			if (clip.Top > y || clip.Bottom < y)
				visible = false;

			int _n_count = node.nodes.Count;
			int middle = y + (item_height / 2);

			if (show_root_lines || node.Parent != null) {
				x += 5;
				if (_n_count > 0) {
					if (show_plus_minus && visible) {
						DrawNodePlusMinus (node, x, y, middle);
					}
				}
				x += indent - 5; 
			}

			int ox = x;

			if (checkboxes) {
				DrawNodeCheckBox (node, ox, y);
				ox += 19;
			}

			if (show_lines)
				DrawNodeLines (node, dash, x, y, middle, item_height, _n_count);

			if (ImageList != null) {
				if (visible)
					DrawNodeImage (node, ox, y);
				// MS leaves the space for the image if the ImageList is
				// non null regardless of whether or not an image is drawn
				ox += ImageList.ImageSize.Width + 3; // leave a little space so the text isn't against the image
			}

			UpdateNodeBounds (node, x, y, item_height);

			if (visible) {
				Rectangle r = node.Bounds;
				StringFormat format = new StringFormat ();
				format.LineAlignment = StringAlignment.Center;
				r.Y += 2; // we have to adjust this to get nice middle alignment
				r.X = ox;

				Color text_color = (Focused && SelectedNode == node ? ThemeEngine.Current.ColorHilightText : node.ForeColor);
				if (Focused) {
					if (SelectedNode == node)
						DeviceContext.FillRectangle (new SolidBrush (ThemeEngine.Current.ColorHilight), node.Bounds);
					if (focused_node == node) {
						Pen dot_pen = new Pen (ThemeEngine.Current.ColorButtonHilight, 1);
						dot_pen.DashStyle = DashStyle.Dot;
						DeviceContext.DrawRectangle (new Pen (ThemeEngine.Current.ColorButtonDkShadow),
								node.Bounds.X, node.Bounds.Y, node.Bounds.Width - 1, node.Bounds.Height - 1);
						DeviceContext.DrawRectangle (dot_pen, node.Bounds.X, node.Bounds.Y, node.Bounds.Width - 1, node.Bounds.Height - 1);
					}
				} else {
					if (!HideSelection && SelectedNode == node)
						DeviceContext.FillRectangle (new SolidBrush (ThemeEngine.Current.ColorButtonFace), node.Bounds);
				}
				DeviceContext.DrawString (node.Text, font, new SolidBrush (text_color), r, format);
				y += item_height + 1;
			}

			if (node.Bounds.Right > max_node_width)
				max_node_width = node.Bounds.Right;

			depth++;
			if (node.IsExpanded) {
				for (int i = 0; i < _n_count; i++) {
					int tdepth = depth;
					DrawNode (node.nodes [i], clip, ref tdepth, ref node_count, item_height,
							font, ref visible_node_count, max_height);
				}
			}

		}

		private void AddVerticalScrollBar (int total_nodes, bool count_changed)
		{
			if (vbar == null) {
				vbar = new VScrollBar ();
				count_changed = true;
			}

			vbar.Bounds = new Rectangle (ClientRectangle.Width - vbar.Width,
				0, vbar.Width, (add_hscroll ? Height - ThemeEngine.Current.HScrollBarDefaultSize.Height : Height));

			if (count_changed) {
				vbar.Maximum = total_nodes;
				int height = ClientRectangle.Height;
				vbar.LargeChange = height / ItemHeight;
			}

			if (!vbar_added) {
				Controls.Add (vbar);
				vbar.ValueChanged += new EventHandler (VScrollBarValueChanged);
				vbar_added = true;
			}

			vbar.Visible = true;
		}

		private void AddHorizontalScrollBar ()
		{
			if (hbar == null)
				hbar = new HScrollBar ();

			hbar.Bounds = new Rectangle (ClientRectangle.Left, ClientRectangle.Bottom - hbar.Height,
					(add_vscroll ? Width - ThemeEngine.Current.VScrollBarDefaultSize.Width : Width), hbar.Height);

			if (!hbar_added) {
				Controls.Add (hbar);
				hbar.ValueChanged += new EventHandler (HScrollBarValueChanged);
				hbar_added = true;
			}

			hbar.Visible = true;
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

		private void HScrollBarValueChanged(object sender, EventArgs e)
		{
			hbar_offset = hbar.Value;
			Refresh ();
		}

		private int GetOpenNodeCount ()
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

			TreeNode node = GetNodeAt (e.Y);
			if (node == null)
				return;
			if (IsTextArea (node, e.X)) {
				selected_node = node;
				Console.WriteLine ("selected node");
				if (selected_node != focused_node) {
					select_mmove = true;
					Refresh ();
				}
			} else if (node.PlusMinusBounds.Contains (e.X, e.Y)) {
				node.Toggle ();
				return;
			} else if (node.CheckBoxBounds.Contains (e.X, e.Y)) {
				node.Checked = !node.Checked;
				return;
			}
		}

		private void MouseUpHandler (object sender, MouseEventArgs e) {
			if (!select_mmove)
				return;
				
			select_mmove = false;

			TreeViewCancelEventArgs ce = new TreeViewCancelEventArgs (selected_node, false, TreeViewAction.ByMouse);
			OnBeforeSelect (ce);
			if (!ce.Cancel) {
				focused_node = selected_node;
				OnAfterSelect (new TreeViewEventArgs (selected_node, TreeViewAction.ByMouse));
			} else {
				selected_node = focused_node;
			}

			Refresh();
		}

		private void MouseMoveHandler (object sender, MouseEventArgs e) {
			if(!select_mmove)
				return;
			TreeNode node = GetNodeAt(e.X,e.Y);
			if(node == selected_node)
				return;
			
			selected_node = focused_node;
			select_mmove = false;
			Refresh();
		}

		private void DoubleClickHandler (object sender, MouseEventArgs e) {
			TreeNode node = GetNodeAt(e.X,e.Y);
			if(node != null) {
				node.Toggle();
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

