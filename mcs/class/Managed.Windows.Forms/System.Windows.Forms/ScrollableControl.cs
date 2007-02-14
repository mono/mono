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
//	Peter Bartok	pbartok@novell.com
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[Designer ("System.Windows.Forms.Design.ScrollableControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
#if NET_2_0
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
#endif
	public class ScrollableControl : Control {
		#region Local Variables
		private bool			hscroll_visible;
		private bool			vscroll_visible;
		private bool			force_hscroll_visible;
		private bool			force_vscroll_visible;
		private bool			auto_scroll;
		private Size			auto_scroll_margin;
		private Size			auto_scroll_min_size;
		private Point			scroll_position;
		private DockPaddingEdges	dock_padding;
		private SizeGrip		sizegrip;
		private ImplicitHScrollBar	hscrollbar;
		private ImplicitVScrollBar	vscrollbar;
		private Size			canvas_size;
		private Rectangle		display_rectangle;
		private Control			old_parent;
		#endregion	// Local Variables

		[TypeConverter(typeof(ScrollableControl.DockPaddingEdgesConverter))]
		#region Subclass DockPaddingEdges
		public class DockPaddingEdges : ICloneable {
			#region DockPaddingEdges Local Variables
			private int	all;
			private int	left;
			private int	right;
			private int	top;
			private int	bottom;
			private Control	owner;
			#endregion	// DockPaddingEdges Local Variables

			#region DockPaddingEdges Constructor
			internal DockPaddingEdges(Control owner) {
				all = 0;
				left = 0;
				right = 0;
				top = 0;
				bottom = 0;
				this.owner = owner;
			}
			#endregion	// DockPaddingEdges Constructor

			#region DockPaddingEdges Public Instance Properties
			[RefreshProperties(RefreshProperties.All)]
			public int All {
				get {
					return all;
				}

				set {
					all = value;
					left = value;
					right = value;
					top = value;
					bottom = value;

					owner.PerformLayout();
				}
			}

			[RefreshProperties(RefreshProperties.All)]
			public int Bottom {
				get {
					return bottom;
				}

				set {
					bottom = value;
					all = 0;

					owner.PerformLayout();
				}
			}

			[RefreshProperties(RefreshProperties.All)]
			public int Left {
				get {
					return left;
				}

				set {
					left=value;
					all = 0;

					owner.PerformLayout();
				}
			}

			[RefreshProperties(RefreshProperties.All)]
			public int Right {
				get {
					return right;
				}

				set {
					right=value;
					all = 0;

					owner.PerformLayout();
				}
			}

			[RefreshProperties(RefreshProperties.All)]
			public int Top {
				get {
					return top;
				}

				set {
					top=value;
					all = 0;

					owner.PerformLayout();
				}
			}
			#endregion	// DockPaddingEdges Public Instance Properties

			// Public Instance Methods
			public override bool Equals(object other) {
				if (! (other is DockPaddingEdges)) {
					return false;
				}

				if (	(this.all == ((DockPaddingEdges)other).all) && (this.left == ((DockPaddingEdges)other).left) &&
					(this.right == ((DockPaddingEdges)other).right) && (this.top == ((DockPaddingEdges)other).top) && 
					(this.bottom == ((DockPaddingEdges)other).bottom)) {
					return true;
				}

				return false;
			}

			public override int GetHashCode() {
				return all*top*bottom*right*left;
			}

			public override string ToString() {
				return "All = "+all.ToString()+" Top = "+top.ToString()+" Left = "+left.ToString()+" Bottom = "+bottom.ToString()+" Right = "+right.ToString();
			}

			internal void Scale(float dx, float dy) {
				left = (int) (left * dx);
				right = (int) (right * dx);
				top = (int) (top * dy);
				bottom = (int) (bottom * dy);
			}

			object ICloneable.Clone() {
				DockPaddingEdges padding_edge;

				padding_edge=new DockPaddingEdges(owner);

				padding_edge.all=all;
				padding_edge.left=left;
				padding_edge.right=right;
				padding_edge.top=top;
				padding_edge.bottom=bottom;

				return padding_edge;
			}
		}
		#endregion	// Subclass DockPaddingEdges

		#region Subclass DockPaddingEdgesConverter
		public class DockPaddingEdgesConverter : System.ComponentModel.TypeConverter {
			// Public Constructors
			public DockPaddingEdgesConverter() {
			}

			// Public Instance Methods
			public override PropertyDescriptorCollection GetProperties(System.ComponentModel.ITypeDescriptorContext context, object value, Attribute[] attributes) {
				return TypeDescriptor.GetProperties(typeof(DockPaddingEdges), attributes);
			}

			public override bool GetPropertiesSupported(System.ComponentModel.ITypeDescriptorContext context) {
				return true;
			}
		}
		#endregion	// Subclass DockPaddingEdgesConverter

		#region Public Constructors
		public ScrollableControl() {
			SetStyle(ControlStyles.ContainerControl, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, false);
			auto_scroll = false;
			hscroll_visible = false;
			vscroll_visible = false;
			force_hscroll_visible = false;
			force_vscroll_visible = false;
			auto_scroll_margin = new Size(0, 0);
			auto_scroll_min_size = new Size(0, 0);
			scroll_position = new Point(0, 0);
			dock_padding = new DockPaddingEdges(this);
			SizeChanged +=new EventHandler(Recalculate);
			VisibleChanged += new EventHandler(Recalculate);
			LocationChanged += new EventHandler (LocationChangedHandler);
			ParentChanged += new EventHandler (ParentChangedHandler);
		}

		void LocationChangedHandler (object sender, EventArgs e)
		{
			UpdateSizeGripVisible ();
		}

		void ParentChangedHandler (object sender, EventArgs e)
		{
			
			if (old_parent == Parent)
				return;
				
			if (old_parent != null) {
				old_parent.SizeChanged -= new EventHandler (Parent_SizeChanged);
#if NET_2_0				
				old_parent.PaddingChanged -= new EventHandler (Parent_PaddingChanged);
#endif
			}
			
			if (Parent != null) {
				Parent.SizeChanged += new EventHandler (Parent_SizeChanged);
#if NET_2_0
				Parent.PaddingChanged += new EventHandler (Parent_PaddingChanged);
#endif
			}
			
			old_parent = Parent;
		}
#if NET_2_0
		void Parent_PaddingChanged (object sender, EventArgs e)
		{
			UpdateSizeGripVisible ();
		}
#endif
		void Parent_SizeChanged (object sender, EventArgs e)
		{
			UpdateSizeGripVisible ();
		}
		#endregion	// Public Constructors

		#region Protected Static Fields
		protected const int ScrollStateAutoScrolling	= 1;
		protected const int ScrollStateFullDrag		= 16;
		protected const int ScrollStateHScrollVisible	= 2;
		protected const int ScrollStateUserHasScrolled	= 8;
		protected const int ScrollStateVScrollVisible	= 4;
		#endregion	// Protected Static Fields

		#region Public Instance Properties
		[DefaultValue(false)]
		[Localizable(true)]
		[MWFCategory("Layout")]
		public virtual bool AutoScroll {
			get {
				return	auto_scroll;
			}

			set {
				if (auto_scroll == value) {
					return;
				}

				auto_scroll = value;
				if (!auto_scroll) {
					SuspendLayout ();

					Controls.RemoveImplicit (hscrollbar);
					hscrollbar.Dispose();
					hscrollbar = null;
					hscroll_visible = false;

					Controls.RemoveImplicit (vscrollbar);
					vscrollbar.Dispose();
					vscrollbar = null;
					vscroll_visible = false;

					Controls.RemoveImplicit (sizegrip);
					sizegrip.Dispose();
					sizegrip = null;

					ResumeLayout ();
				} else {
					SuspendLayout ();

					hscrollbar = new ImplicitHScrollBar();
					hscrollbar.Visible = false;
					hscrollbar.ValueChanged += new EventHandler(HandleScrollBar);
					hscrollbar.Height = SystemInformation.HorizontalScrollBarHeight;
					this.Controls.AddImplicit (hscrollbar);

					vscrollbar = new ImplicitVScrollBar();
					vscrollbar.Visible = false;
					vscrollbar.ValueChanged += new EventHandler(HandleScrollBar);
					vscrollbar.Width = SystemInformation.VerticalScrollBarWidth;
					this.Controls.AddImplicit (vscrollbar);

					sizegrip = new SizeGrip (this);
					sizegrip.Visible = false;
					this.Controls.AddImplicit (sizegrip);

					ResumeLayout ();
				}
			}
		}

		[Localizable(true)]
		[MWFCategory("Layout")]
		public Size AutoScrollMargin {
			get {
				return auto_scroll_margin;
			}

			set {
				if (value.Width < 0) {
					throw new ArgumentException("Width is assigned less than 0", "value.Width");
				}

				if (value.Height < 0) {
					throw new ArgumentException("Height is assigned less than 0", "value.Height");
				}

				auto_scroll_margin = value;
			}
		}

		[Localizable(true)]
		[MWFCategory("Layout")]
		public Size AutoScrollMinSize {
			get {
				return auto_scroll_min_size;
			}

			set {
				if (value != auto_scroll_min_size) {
					auto_scroll_min_size = value;
					AutoScroll = true;
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Point AutoScrollPosition {
			get {
				return new Point(-scroll_position.X, -scroll_position.Y);
			}

			set {
				if ((value.X != scroll_position.X) || (value.Y != scroll_position.Y)) {
					int	shift_x;
					int	shift_y;

					shift_x = 0;
					shift_y = 0;
					if (hscroll_visible) {
						shift_x = value.X - scroll_position.X;
					}

					if (vscroll_visible) {
						shift_y = value.Y - scroll_position.Y;
					}

					ScrollWindow(shift_x, shift_y);

					if (hscroll_visible) {
						hscrollbar.Value = scroll_position.X;
					}

					if (vscroll_visible) {
						vscrollbar.Value = scroll_position.Y;
					}

				}
			}
		}

		public override Rectangle DisplayRectangle {
			get {
				if (auto_scroll) {
					int		width;
					int		height;

					if (canvas_size.Width <= base.DisplayRectangle.Width) {
						width = base.DisplayRectangle.Width;
						if (vscroll_visible) {
							width -= vscrollbar.Width;
						}
					} else {
						width = canvas_size.Width;
					}

					if (canvas_size.Height <= base.DisplayRectangle.Height) {
						height = base.DisplayRectangle.Height;
						if (hscroll_visible) {
							height -= hscrollbar.Height;
						}
					} else {
						height = canvas_size.Height;
					}

					display_rectangle.X = -scroll_position.X;
					display_rectangle.Y = -scroll_position.Y;
					display_rectangle.Width = Math.Max(auto_scroll_min_size.Width, width);
					display_rectangle.Height = Math.Max(auto_scroll_min_size.Height, height);
				}
				else {
					display_rectangle = base.DisplayRectangle;
				}

				display_rectangle.X += dock_padding.Left;
				display_rectangle.Y += dock_padding.Top;
				display_rectangle.Width -= dock_padding.Left + dock_padding.Right;
				display_rectangle.Height -= dock_padding.Top + dock_padding.Bottom;

				return display_rectangle;
			}
		}

		[MWFCategory("Layout")]
#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#else
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Localizable(true)]
#endif
		public DockPaddingEdges DockPadding {
			get {
				return dock_padding;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected bool HScroll {
			get {
				return hscroll_visible;
			}

			set {
				if (hscroll_visible != value) {
					force_hscroll_visible = value;
					Recalculate(this, EventArgs.Empty);
				}
			}
		}

		protected bool VScroll {
			get {
				return vscroll_visible;
			}

			set {
				if (vscroll_visible != value) {
					force_vscroll_visible = value;
					Recalculate(this, EventArgs.Empty);
				}
			}
		}
		#endregion	// Protected Instance Methods

		#region Public Instance Methods
		public void ScrollControlIntoView(Control activeControl) {
			int	x;
			int	y;
			int	corner_x;
			int	corner_y;

			if (!AutoScroll || (!hscroll_visible && !vscroll_visible)) {
				return;
			}

			if (!Contains(activeControl)) {
				return;
			}

			x = activeControl.Left;
			y = activeControl.Top;

			// Translate into coords relative to us
			if (activeControl.Parent != this) {
				activeControl.PointToScreen(ref x, ref y);
				PointToClient(ref x, ref y);
			}

			x += scroll_position.X;
			y += scroll_position.Y;

			// Don't scroll if already visible
			if ((activeControl.Left >= scroll_position.X) && (activeControl.Left < (scroll_position.X + ClientSize.Width)) &&
			    (activeControl.Top >= scroll_position.Y) && (activeControl.Top < (scroll_position.Y + ClientSize.Height))) {
				return;
			}

			// try to center
			corner_x = Math.Max(0, x + activeControl.Width / 2 - ClientSize.Width / 2);
			corner_y = Math.Max(0, y + activeControl.Height / 2 - ClientSize.Height / 2);

			if (hscroll_visible && (corner_x > hscrollbar.Maximum)) {
				corner_x = Math.Max(0, hscrollbar.Maximum - ClientSize.Width);
			}

			if (vscroll_visible && (corner_y > vscrollbar.Maximum)) {
				corner_y = Math.Max(0, vscrollbar.Maximum - ClientSize.Height);
			}
			if ((corner_x == scroll_position.X) && (corner_y == scroll_position.Y)) {
				return;
			}

			//this.SetDisplayRectLocation(-corner_x, -corner_y);
			hscrollbar.Value = corner_x;
			vscrollbar.Value = corner_y;
		}

		public void SetAutoScrollMargin(int x, int y) {
			if (x < 0) {
				x = 0;
			}

			if (y < 0) {
				y = 0;
			}

			auto_scroll_margin = new Size(x, y);
			Recalculate(this, EventArgs.Empty);
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void AdjustFormScrollbars(bool displayScrollbars) {
			Recalculate(this, EventArgs.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected bool GetScrollState(int bit) {
			// Internal MS
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnLayout(LayoutEventArgs levent) {
			CalculateCanvasSize();

			AdjustFormScrollbars(AutoScroll);	// Dunno what the logic is. Passing AutoScroll seems to match MS behaviour
			base.OnLayout(levent);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnMouseWheel(MouseEventArgs e) {
			if (vscroll_visible) {
				if (e.Delta > 0) {
					if (vscrollbar.Minimum < (vscrollbar.Value - vscrollbar.LargeChange)) {
						vscrollbar.Value -= vscrollbar.LargeChange;
					} else {
						vscrollbar.Value = vscrollbar.Minimum;
					}
				} else {
					if (vscrollbar.Maximum > (vscrollbar.Value + vscrollbar.LargeChange)) {
						vscrollbar.Value += vscrollbar.LargeChange;
					} else {
						vscrollbar.Value = vscrollbar.Maximum;
					}
				}
			}
			base.OnMouseWheel(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnVisibleChanged(EventArgs e) {
			if (Visible) {
				UpdateChildrenZOrder ();
				PerformLayout();
			}
			base.OnVisibleChanged(e);
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Never)]
#endif
		protected override void ScaleCore(float dx, float dy) {
			dock_padding.Scale(dx, dy);
			base.ScaleCore(dx, dy);
		}

		protected void SetDisplayRectLocation(int x, int y) {
			// This method is weird. MS documents that the scrollbars are not
			// updated. We need to move stuff, but leave the scrollbars as is

			if (x > 0) {
				x = 0;
			}

			if (y > 0) {
				y = 0;
			}

			ScrollWindow(scroll_position.X - x , scroll_position.Y - y);
		}

		protected void SetScrollState(int bit, bool value) {
			//throw new NotImplementedException();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);
		}
		#endregion	// Protected Instance Methods

		#region Internal & Private Methods
		private void CalculateCanvasSize() {
			Control		child;
			int		num_of_children;
			int		width;
			int		height;
			int		extra_width;
			int		extra_height;

			num_of_children = Controls.Count;
			width = 0;
			height = 0;
			extra_width = dock_padding.Right;
			extra_height = dock_padding.Bottom;

			for (int i = 0; i < num_of_children; i++) {
				child = Controls[i];
				if (child.Dock == DockStyle.Right) {
					extra_width += child.Width;
				} else if (child.Dock == DockStyle.Bottom) {
					extra_height += child.Height;
				}
			}

			if (!auto_scroll_min_size.IsEmpty) {
				width = auto_scroll_min_size.Width;
				height = auto_scroll_min_size.Height;
			}

			for (int i = 0; i < num_of_children; i++) {
				child = Controls[i];

				switch(child.Dock) {
					case DockStyle.Left: {
						if ((child.Right + extra_width) > width) {
							width = child.Right + extra_width;
						}
						continue;
					}

					case DockStyle.Top: {
						if ((child.Bottom + extra_height) > height) {
							height = child.Bottom + extra_height;
						}
						continue;
					}

					case DockStyle.Fill:
					case DockStyle.Right:
					case DockStyle.Bottom: {
						continue;
					}

					default: {
						AnchorStyles	anchor;

						anchor = child.Anchor;

						if (((anchor & AnchorStyles.Left) != 0) && ((anchor & AnchorStyles.Right) == 0)) {
							if ((child.Right + extra_width) > width) {
								width = child.Right + extra_width;
							}
						}

						if (((anchor & AnchorStyles.Top) != 0) || ((anchor & AnchorStyles.Bottom) == 0)) {
							if ((child.Bottom + extra_height) > height) {
								height = child.Bottom + extra_height;
							}
						}
						continue;
					}
				}
			}
			width += scroll_position.X;
			height += scroll_position.Y;

			canvas_size.Width = width;
			canvas_size.Height = height;
		}

		private void Recalculate (object sender, EventArgs e) {
			if (!auto_scroll && !force_hscroll_visible && !force_vscroll_visible) {
				return;
			}

			Size canvas = canvas_size;
			Size client = ClientSize;

			canvas.Width += auto_scroll_margin.Width;
			canvas.Height += auto_scroll_margin.Height;

			int right_edge = client.Width;
			int bottom_edge = client.Height;
			int prev_right_edge;
			int prev_bottom_edge;

			do {
				prev_right_edge = right_edge;
				prev_bottom_edge = bottom_edge;

				if ((force_hscroll_visible || canvas.Width > right_edge) && client.Width > 0) {
					hscroll_visible = true;
					bottom_edge = client.Height - SystemInformation.HorizontalScrollBarHeight;
				} else {
					hscroll_visible = false;
					bottom_edge = client.Height;
				}

				if ((force_vscroll_visible || canvas.Height > bottom_edge) && client.Height > 0) {
					vscroll_visible = true;
					right_edge = client.Width - SystemInformation.VerticalScrollBarWidth;
				} else {
					vscroll_visible = false;
					right_edge = client.Width;
				}

			} while (right_edge != prev_right_edge || bottom_edge != prev_bottom_edge);

			if (right_edge < 0) right_edge = 0;
			if (bottom_edge < 0) bottom_edge = 0;

			Rectangle hscroll_bounds;
			Rectangle vscroll_bounds;

			hscroll_bounds = new Rectangle (0, client.Height - SystemInformation.HorizontalScrollBarHeight,
							ClientRectangle.Width, SystemInformation.HorizontalScrollBarHeight);
			vscroll_bounds = new Rectangle (client.Width - SystemInformation.VerticalScrollBarWidth, 0,
							SystemInformation.VerticalScrollBarWidth, ClientRectangle.Height);

			/* the ScrollWindow calls here are needed
			 * because (this explanation sucks):
			 * 
			 * when we transition from having a scrollbar to
			 * not having one, we won't receive a scrollbar
			 * moved (value changed) event, so we need to
			 * manually scroll the canvas.
			 * 
			 * if you can fix this without requiring the
			 * ScrollWindow calls, pdb and toshok will each
			 * pay you $5.
			*/
			if (hscroll_visible) {
				hscrollbar.LargeChange = right_edge;
				hscrollbar.SmallChange = 5;
				hscrollbar.Maximum = canvas.Width - 1;
			} else {
				if (hscrollbar.Visible) {
					ScrollWindow (- scroll_position.X, 0);
				}
				scroll_position.X = 0;
			}

			if (vscroll_visible) {
				vscrollbar.LargeChange = bottom_edge;
				vscrollbar.SmallChange = 5;
				vscrollbar.Maximum = canvas.Height - 1;
			} else {
				if (vscrollbar.Visible) {
					ScrollWindow (0, - scroll_position.Y);
				}
				scroll_position.Y = 0;
			}

			if (hscroll_visible && vscroll_visible) {
				hscroll_bounds.Width -= SystemInformation.VerticalScrollBarWidth;
				vscroll_bounds.Height -= SystemInformation.HorizontalScrollBarHeight;

				sizegrip.Bounds = new Rectangle (hscroll_bounds.Right,
								 vscroll_bounds.Bottom,
								 SystemInformation.VerticalScrollBarWidth,
								 SystemInformation.HorizontalScrollBarHeight);
			}

			hscrollbar.Bounds = hscroll_bounds;
			vscrollbar.Bounds = vscroll_bounds;
			hscrollbar.Visible = hscroll_visible;
			vscrollbar.Visible = vscroll_visible;
			UpdateSizeGripVisible ();
		}

		internal void UpdateSizeGripVisible ()
		{
			if (sizegrip == null)
				return;
				
			sizegrip.CapturedControl = Parent;
			// This is really wierd, the size grip is only showing up 
			// if the bottom right corner of the scrollable control is within
			// two pixels from the bottom right corner of its parent.
			bool show_sizegrip = hscroll_visible && vscroll_visible;
			bool enable_sizegrip = false;
			if (show_sizegrip && Parent != null) {
				Point diff = new Point (Parent.ClientRectangle.Bottom - Bottom, Parent.ClientRectangle.Right - Right);
				enable_sizegrip = diff.X <= 2 && diff.X >= 0 && diff.Y <= 2 && diff.Y >= 0;
			}
			sizegrip.Visible = show_sizegrip;
			sizegrip.Enabled = enable_sizegrip || sizegrip.Capture;
		}

		private void HandleScrollBar(object sender, EventArgs e) {
			if (sender == vscrollbar) {
				ScrollWindow(0, vscrollbar.Value- scroll_position.Y);
			} else {
				ScrollWindow(hscrollbar.Value - scroll_position.X, 0);
			}
		}

		private void ScrollWindow(int XOffset, int YOffset) {
			int	num_of_children;

			if (XOffset == 0 && YOffset == 0) {
				return;
			}

			SuspendLayout();

			num_of_children = Controls.Count;

			for (int i = 0; i < num_of_children; i++) {
				Controls[i].Left -= XOffset;
				Controls[i].Top -= YOffset;
				// Is this faster? Controls[i].Location -= new Size(XOffset, YOffset);
			}

			scroll_position.X += XOffset;
			scroll_position.Y += YOffset;

			// Should we call XplatUI.ScrollWindow??? If so, we need to position our windows by other means above
			// Since we're already causing a redraw above
			Invalidate(false);
			ResumeLayout(false);
		}
		#endregion	// Internal & Private Methods

#if NET_2_0
		static object OnScrollEvent = new object ();
		
		protected virtual void OnScroll (ScrollEventArgs se)
		{
			EventHandler eh = (EventHandler) (Events [OnScrollEvent]);
			if (eh != null)
				eh (this, se);
		}

		protected override void OnPaintBackground (PaintEventArgs e)
		{
			base.OnPaintBackground (e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnRightToLeftChanged (EventArgs e)
		{
			base.OnRightToLeftChanged (e);
		}

		public event ScrollEventHandler Scroll {
			add { Events.AddHandler (OnScrollEvent, value); }
			remove { Events.RemoveHandler (OnScrollEvent, value); }
		}
#endif
	}
}
