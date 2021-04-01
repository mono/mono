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
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public class ScrollableControl : Control {
		#region Local Variables
		private bool			force_hscroll_visible;
		private bool			force_vscroll_visible;
		private bool			auto_scroll;
		private Size			auto_scroll_margin;
		private Size			auto_scroll_min_size;
		private Point			scroll_position;
		private DockPaddingEdges	dock_padding;
		private SizeGrip		sizegrip;
		internal ImplicitHScrollBar	hscrollbar;
		internal ImplicitVScrollBar	vscrollbar;
		internal Size			canvas_size;
		private Rectangle		display_rectangle;
		private Control			old_parent;
		private HScrollProperties	horizontalScroll;
		private VScrollProperties	verticalScroll;
		private bool			autosized_child;
		#endregion	// Local Variables

		[TypeConverter(typeof(ScrollableControl.DockPaddingEdgesConverter))]
		#region Subclass DockPaddingEdges
		public class DockPaddingEdges : ICloneable
		{
			private Control	owner;
			
			internal DockPaddingEdges (Control owner)
			{
				this.owner = owner;
			}

			#region DockPaddingEdges Public Instance Properties
			[RefreshProperties (RefreshProperties.All)]
			public int All {
				get { return owner.Padding.All; }
				set { owner.Padding = new Padding (value); }
			}

			[RefreshProperties (RefreshProperties.All)]
			public int Bottom {
				get { return owner.Padding.Bottom; }
				set { owner.Padding = new Padding (Left, Top, Right, value); }
			}

			[RefreshProperties (RefreshProperties.All)]
			public int Left {
				get { return owner.Padding.Left; }
				set { owner.Padding = new Padding (value, Top, Right, Bottom); }
			}

			[RefreshProperties (RefreshProperties.All)]
			public int Right {
				get { return owner.Padding.Right; }
				set { owner.Padding = new Padding (Left, Top, value, Bottom); }
			}

			[RefreshProperties (RefreshProperties.All)]
			public int Top {
				get { return owner.Padding.Top; }
				set { owner.Padding = new Padding (Left, value, Right, Bottom); }
			}
			#endregion	// DockPaddingEdges Public Instance Properties

			// Public Instance Methods
			public override bool Equals (object other)
			{
				if (!(other is DockPaddingEdges)) {
					return false;
				}

				if ((this.All == ((DockPaddingEdges)other).All) && (this.Left == ((DockPaddingEdges)other).Left) &&
					(this.Right == ((DockPaddingEdges)other).Right) && (this.Top == ((DockPaddingEdges)other).Top) &&
					(this.Bottom == ((DockPaddingEdges)other).Bottom)) {
					return true;
				}

				return false;
			}

			public override int GetHashCode ()
			{
				return All * Top * Bottom * Right * Left;
			}

			public override string ToString ()
			{
				return "All = " + All.ToString () + " Top = " + Top.ToString () + " Left = " + Left.ToString () + " Bottom = " + Bottom.ToString () + " Right = " + Right.ToString ();
			}

			internal void Scale (float dx, float dy)
			{
				Left = (int)(Left * dx);
				Right = (int)(Right * dx);
				Top = (int)(Top * dy);
				Bottom = (int)(Bottom * dy);
			}

			object ICloneable.Clone ()
			{
				return new DockPaddingEdges (owner);
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
			force_hscroll_visible = false;
			force_vscroll_visible = false;
			auto_scroll_margin = new Size(0, 0);
			auto_scroll_min_size = new Size(0, 0);
			scroll_position = new Point(0, 0);
			SizeChanged +=new EventHandler(Recalculate);
			VisibleChanged += new EventHandler (VisibleChangedHandler);
			LocationChanged += new EventHandler (LocationChangedHandler);
			ParentChanged += new EventHandler (ParentChangedHandler);
			HandleCreated += new EventHandler (AddScrollbars);

			CreateScrollbars ();
			
			horizontalScroll = new HScrollProperties (this);
			verticalScroll = new VScrollProperties (this);
		}

		void VisibleChangedHandler (object sender, EventArgs e)
		{
			Recalculate (false);
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
				old_parent.PaddingChanged -= new EventHandler (Parent_PaddingChanged);
			}
			
			if (Parent != null) {
				Parent.SizeChanged += new EventHandler (Parent_SizeChanged);
				Parent.PaddingChanged += new EventHandler (Parent_PaddingChanged);
			}
			
			old_parent = Parent;
		}

		void Parent_PaddingChanged (object sender, EventArgs e)
		{
			UpdateSizeGripVisible ();
		}

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
				if (auto_scroll != value) {
					auto_scroll = value;
					PerformLayout (this, "AutoScroll");
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

		internal bool ShouldSerializeAutoScrollMargin ()
		{
			return this.AutoScrollMargin != new Size (0, 0);
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
					PerformLayout (this, "AutoScrollMinSize");
				}
			}
		}

		internal bool ShouldSerializeAutoScrollMinSize ()
		{
			return this.AutoScrollMinSize != new Size (0, 0);
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Point AutoScrollPosition {
			get {
				return DisplayRectangle.Location;
			}

			set {
				if (value != AutoScrollPosition) {
					int	shift_x;
					int	shift_y;

					shift_x = 0;
					shift_y = 0;
					if (hscrollbar.VisibleInternal) {
						int max = hscrollbar.Maximum - hscrollbar.LargeChange + 1;
						value.X = value.X < hscrollbar.Minimum ? hscrollbar.Minimum : value.X;
						value.X = value.X > max ? max : value.X;
						shift_x = value.X - scroll_position.X;
					}

					if (vscrollbar.VisibleInternal) {
						int max = vscrollbar.Maximum - vscrollbar.LargeChange + 1;
						value.Y = value.Y < vscrollbar.Minimum ? vscrollbar.Minimum : value.Y;
						value.Y = value.Y > max ? max : value.Y;
						shift_y = value.Y - scroll_position.Y;
					}

					ScrollWindow(shift_x, shift_y);

					if (hscrollbar.VisibleInternal) {
						if (scroll_position.X >= hscrollbar.Minimum && scroll_position.X <= hscrollbar.Maximum)
							hscrollbar.Value = scroll_position.X;
					}

					if (vscrollbar.VisibleInternal) {
						if (scroll_position.Y >= vscrollbar.Minimum && scroll_position.Y <= vscrollbar.Maximum)
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
						if (vscrollbar.VisibleInternal) {
							width -= vscrollbar.Width;
						}
					} else {
						width = canvas_size.Width;
					}

					if (canvas_size.Height <= base.DisplayRectangle.Height) {
						height = base.DisplayRectangle.Height;
						if (hscrollbar.VisibleInternal) {
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

				// DockPadding is the same as Padding (according to documentation) but is
				// calculated lazily, so we use Padding here instead.
				if (Padding != Padding.Empty) {
					display_rectangle.X += Padding.Left;
					display_rectangle.Y += Padding.Top;
					display_rectangle.Width -= Padding.Horizontal;
					display_rectangle.Height -= Padding.Vertical;
				}

				return display_rectangle;
			}
		}

		[MWFCategory("Layout")]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DockPaddingEdges DockPadding {
			get {
				if (dock_padding == null)
					CreateDockPadding ();

				return dock_padding;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public HScrollProperties HorizontalScroll {
			get { return horizontalScroll; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public VScrollProperties VerticalScroll {
			get { return verticalScroll; }
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
				return hscrollbar.VisibleInternal;
			}

			set {
				if (!AutoScroll && hscrollbar.VisibleInternal != value) {
					force_hscroll_visible = value;
					Recalculate (false);
				}
			}
		}

		protected bool VScroll {
			get {
				return vscrollbar.VisibleInternal;
			}

			set {
				if (!AutoScroll && vscrollbar.VisibleInternal != value) {
					force_vscroll_visible = value;
					Recalculate (false);
				}
			}
		}
		#endregion	// Protected Instance Methods

		#region Public Instance Methods
		public void ScrollControlIntoView(Control activeControl) {
			int	corner_x;
			int	corner_y;

			Rectangle within = new Rectangle ();
			within.Size = ClientSize;
			
			if (!AutoScroll || (!hscrollbar.VisibleInternal && !vscrollbar.VisibleInternal)) {
				return;
			}

			if (!Contains(activeControl)) {
				return;
			}

			if (vscrollbar.Visible) {
				within.Width -= vscrollbar.Width;
			}
			if (hscrollbar.Visible) {
				within.Height -= hscrollbar.Height;
			}

			// Don't scroll if already visible
			if (within.Contains (activeControl.Location) && within.Contains (activeControl.Right, activeControl.Bottom)) {
				return;
			}

			// If the control is above the top or the left, move it down and right until it aligns 
			// with the top/left.
			// If the control is below the bottom or to the right, move it up/left until it aligns
			// with the bottom/right, but do never move it further than the top/left side.
			int x_diff = 0, y_diff = 0;
			if (activeControl.Top <= 0 || activeControl.Height >= within.Height) {
				y_diff = -activeControl.Top;
			} else if (activeControl.Bottom > within.Height) {
				y_diff = within.Height - activeControl.Bottom;
			}
			if (activeControl.Left <= 0 || activeControl.Width >= within.Width) {
				x_diff = -activeControl.Left;
			} else if (activeControl.Right > within.Width) {
				x_diff = within.Width - activeControl.Right;
			}
			corner_x = hscrollbar.Value - x_diff;
			corner_y = vscrollbar.Value - y_diff;

			if (hscrollbar.VisibleInternal) {
				if (corner_x > hscrollbar.Maximum) {
					corner_x = hscrollbar.Maximum;
				} else if (corner_x < hscrollbar.Minimum) {
					corner_x = hscrollbar.Minimum;
				}
				if (corner_x != hscrollbar.Value) {
					hscrollbar.Value = corner_x;
				}
			}

			if (vscrollbar.VisibleInternal) {
				if (corner_y > vscrollbar.Maximum) {
					corner_y = vscrollbar.Maximum;
				} else if (corner_y < vscrollbar.Minimum) {
					corner_y = vscrollbar.Minimum;
				}
				if (corner_y != vscrollbar.Value) {
					vscrollbar.Value = corner_y;
				}
			}
		}

		public void SetAutoScrollMargin(int x, int y) {
			if (x < 0) {
				x = 0;
			}

			if (y < 0) {
				y = 0;
			}

			auto_scroll_margin = new Size(x, y);
			Recalculate (false);
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void AdjustFormScrollbars(bool displayScrollbars) {
			Recalculate (false);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected bool GetScrollState(int bit) {
			// Internal MS
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnLayout(LayoutEventArgs levent) {
			CalculateCanvasSize (true);

			AdjustFormScrollbars(AutoScroll);	// Dunno what the logic is. Passing AutoScroll seems to match MS behaviour
			base.OnLayout(levent);

			// The first time through, we just set the canvas to clientsize
			// so we could re-layout everything according to the flow.
			// This time we want to actually calculate the canvas.
			// If a child is autosized, we need to rethink scrollbars as well. (Xamarin bug 18874)
			if (this is FlowLayoutPanel || autosized_child) {
				CalculateCanvasSize (false);
				AdjustFormScrollbars (AutoScroll);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnMouseWheel(MouseEventArgs e) {
			if (vscrollbar.VisibleInternal) {
				if (e.Delta > 0) {
					if (vscrollbar.Minimum < (vscrollbar.Value - vscrollbar.LargeChange)) {
						vscrollbar.Value -= vscrollbar.LargeChange;
					} else {
						vscrollbar.Value = vscrollbar.Minimum;
					}
				} else {
					int maximum_scrollbar_value = vscrollbar.Maximum - vscrollbar.LargeChange + 1;
					if (maximum_scrollbar_value > (vscrollbar.Value + vscrollbar.LargeChange)) {
						vscrollbar.Value += vscrollbar.LargeChange;
					} else {
						vscrollbar.Value = maximum_scrollbar_value;
					}
				}
			}
			base.OnMouseWheel(e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnVisibleChanged(EventArgs e) {
			if (Visible) {
				UpdateChildrenZOrder ();
				PerformLayout(this, "Visible");
			}
			base.OnVisibleChanged(e);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override void ScaleCore(float dx, float dy) {
			if (dock_padding != null)
				dock_padding.Scale(dx, dy);

			base.ScaleCore(dx, dy);
		}

		protected override void ScaleControl (SizeF factor, BoundsSpecified specified)
		{
			base.ScaleControl (factor, specified);
		}
		
		protected virtual Point ScrollToControl (Control activeControl)
		{
			int corner_x;
			int corner_y;

			Rectangle within = new Rectangle ();
			within.Size = ClientSize;

			if (vscrollbar.Visible)
				within.Width -= vscrollbar.Width;

			if (hscrollbar.Visible)
				within.Height -= hscrollbar.Height;

			// If the control is above the top or the left, move it down and right until it aligns 
			// with the top/left.
			// If the control is below the bottom or to the right, move it up/left until it aligns
			// with the bottom/right, but do never move it further than the top/left side.
			int x_diff = 0, y_diff = 0;
			
			if (activeControl.Top <= 0 || activeControl.Height >= within.Height)
				y_diff = -activeControl.Top;
			else if (activeControl.Bottom > within.Height)
				y_diff = within.Height - activeControl.Bottom;

			if (activeControl.Left <= 0 || activeControl.Width >= within.Width)
				x_diff = -activeControl.Left;
			else if (activeControl.Right > within.Width)
				x_diff = within.Width - activeControl.Right;

			corner_x = AutoScrollPosition.X + x_diff;
			corner_y = AutoScrollPosition.Y + y_diff;
			
			return new Point (corner_x, corner_y);
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
		internal override IntPtr AfterTopMostControl ()
		{
			// order of scrollbars:
			// top = vertical
			//       sizegrid
			// bottom = horizontal
			if (hscrollbar != null && hscrollbar.Visible)
				return hscrollbar.Handle;
			// no need to check for sizegrip since it will only
			// be visible if hbar is visible.
			if (vscrollbar != null && vscrollbar.Visible)
				return hscrollbar.Handle;

			return base.AfterTopMostControl ();
		}

		internal virtual void CalculateCanvasSize (bool canOverride) {
			Control		child;
			int		num_of_children;
			int		width;
			int		height;
			int		extra_width;
			int		extra_height;

			num_of_children = Controls.Count;
			width = 0;
			height = 0;
			extra_width = hscrollbar.Value;
			extra_height = vscrollbar.Value;
			if (dock_padding != null) {
				extra_width += dock_padding.Right;
				extra_height += dock_padding.Bottom;
			}

			autosized_child = false;
			for (int i = 0; i < num_of_children; i++) {
				child = Controls[i];
				if (!child.VisibleInternal)
					continue;
				if (child.AutoSize)
					autosized_child = true;
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
				if (!child.VisibleInternal)
					continue;

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

			canvas_size.Width = width;
			canvas_size.Height = height;
		}

		// Normally DockPadding is created lazyly, as observed in the test cases, but some children
		// may need to have it always.
		internal void CreateDockPadding ()
		{
			if (dock_padding == null)
				dock_padding = new DockPaddingEdges (this);
		}

		private void Recalculate (object sender, EventArgs e) {
			Recalculate (true);
		}
				
		private void Recalculate (bool doLayout) {
			if (!IsHandleCreated) {
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

			bool hscroll_visible;
			bool vscroll_visible;

			do {
				prev_right_edge = right_edge;
				prev_bottom_edge = bottom_edge;

				if ((force_hscroll_visible || (canvas.Width > right_edge && auto_scroll)) && client.Width > 0) {
					hscroll_visible = true;
					bottom_edge = client.Height - SystemInformation.HorizontalScrollBarHeight;
				} else {
					hscroll_visible = false;
					bottom_edge = client.Height;
				}

				if ((force_vscroll_visible || (canvas.Height > bottom_edge && auto_scroll)) && client.Height > 0) {
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

			if (!vscrollbar.Visible) {
				vscrollbar.Value = vscrollbar.Minimum;
			}
			if (!hscrollbar.Visible) {
				hscrollbar.Value = hscrollbar.Minimum;
			}

			/* Manually setting the size of the thumb should be done before
			 * the other assignments */
			if (hscroll_visible) {
				hscrollbar.manual_thumb_size = right_edge;
				hscrollbar.LargeChange = right_edge;
				hscrollbar.SmallChange = 5;
				hscrollbar.Maximum = canvas.Width - 1;
			} else {
				if (hscrollbar != null && hscrollbar.VisibleInternal) {
					ScrollWindow (- scroll_position.X, 0);
				}
				scroll_position.X = 0;
			}

			if (vscroll_visible) {
				vscrollbar.manual_thumb_size = bottom_edge;
				vscrollbar.LargeChange = bottom_edge;
				vscrollbar.SmallChange = 5;
				vscrollbar.Maximum = canvas.Height - 1;
			} else {
				if (vscrollbar != null && vscrollbar.VisibleInternal) {
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
			
			SuspendLayout ();

			hscrollbar.SetBounds (hscroll_bounds.X, hscroll_bounds.Y, hscroll_bounds.Width, hscroll_bounds.Height, BoundsSpecified.All);
			hscrollbar.Visible = hscroll_visible;
			if (hscrollbar.Visible)
				XplatUI.SetZOrder (hscrollbar.Handle, IntPtr.Zero, true, false);

			vscrollbar.SetBounds (vscroll_bounds.X, vscroll_bounds.Y, vscroll_bounds.Width, vscroll_bounds.Height, BoundsSpecified.All);
			vscrollbar.Visible = vscroll_visible;
			if (vscrollbar.Visible)
				XplatUI.SetZOrder (vscrollbar.Handle, IntPtr.Zero, true, false);

			UpdateSizeGripVisible ();

			ResumeLayout (doLayout);
			
			// We should now scroll the active control into view, 
			// the funny part is that ScrollableControl does not have 
			// the concept of active control.
			ContainerControl container = this as ContainerControl;
			if (container != null && container.ActiveControl != null) {
				ScrollControlIntoView (container.ActiveControl);
			}
		}

		internal void UpdateSizeGripVisible ()
		{
			if (!IsHandleCreated) {
				return;
			}

			sizegrip.CapturedControl = Parent;
			// This is really wierd, the size grip is only showing up 
			// if the bottom right corner of the scrollable control is within
			// two pixels from the bottom right corner of its parent.
			bool show_sizegrip = hscrollbar.VisibleInternal && vscrollbar.VisibleInternal;
			bool enable_sizegrip = false;
			if (show_sizegrip && Parent != null) {
				Point diff = new Point (Parent.ClientRectangle.Bottom - Bottom, Parent.ClientRectangle.Right - Right);
				enable_sizegrip = diff.X <= 2 && diff.X >= 0 && diff.Y <= 2 && diff.Y >= 0;
			}
			sizegrip.Visible = show_sizegrip;
			sizegrip.Enabled = enable_sizegrip || sizegrip.Capture;
			if (sizegrip.Visible)
				XplatUI.SetZOrder (sizegrip.Handle, vscrollbar.Handle, false, false);
		}

		private void HandleScrollBar(object sender, EventArgs e) {
			if (sender == vscrollbar) {
				if (!vscrollbar.Visible)
					return;
				ScrollWindow(0, vscrollbar.Value- scroll_position.Y);
			} else {
				if (!hscrollbar.Visible)
					return;
				ScrollWindow(hscrollbar.Value - scroll_position.X, 0);
			}
		}

		private void HandleScrollEvent (object sender, ScrollEventArgs args)
		{
			OnScroll (args);
		}

		private void AddScrollbars (object o, EventArgs e)
		{
			Controls.AddRangeImplicit (new Control[] {hscrollbar, vscrollbar, sizegrip});
			HandleCreated -= new EventHandler (AddScrollbars);
		}

		private void CreateScrollbars ()
		{
			hscrollbar = new ImplicitHScrollBar ();
			hscrollbar.Visible = false;
			hscrollbar.ValueChanged += new EventHandler (HandleScrollBar);
			hscrollbar.Height = SystemInformation.HorizontalScrollBarHeight;
			hscrollbar.use_manual_thumb_size = true;
			hscrollbar.Scroll += new ScrollEventHandler (HandleScrollEvent);

			vscrollbar = new ImplicitVScrollBar ();
			vscrollbar.Visible = false;
			vscrollbar.ValueChanged += new EventHandler (HandleScrollBar);
			vscrollbar.Width = SystemInformation.VerticalScrollBarWidth;
			vscrollbar.use_manual_thumb_size = true;
			vscrollbar.Scroll += new ScrollEventHandler (HandleScrollEvent);

			sizegrip = new SizeGrip (this);
			sizegrip.Visible = false;
		}

		private void ScrollWindow(int XOffset, int YOffset) {
			int	num_of_children;

			if (XOffset == 0 && YOffset == 0) {
				return;
			}

			SuspendLayout();

			num_of_children = Controls.Count;

			for (int i = 0; i < num_of_children; i++) {
				Controls[i].Location = new Point (Controls[i].Left - XOffset, Controls[i].Top - YOffset);
				//Controls[i].Left -= XOffset;
				//Controls[i].Top -= YOffset;
				// Is this faster? Controls[i].Location -= new Size(XOffset, YOffset);
			}

			scroll_position.X += XOffset;
			scroll_position.Y += YOffset;

			XplatUI.ScrollWindow (Handle, ClientRectangle, -XOffset, -YOffset, false);
			ResumeLayout(false);
		}
		#endregion	// Internal & Private Methods

		static object OnScrollEvent = new object ();
		
		protected virtual void OnScroll (ScrollEventArgs se)
		{
			ScrollEventHandler eh = (ScrollEventHandler) (Events [OnScrollEvent]);
			if (eh != null)
				eh (this, se);
		}

		protected override void OnPaddingChanged (EventArgs e)
		{
			base.OnPaddingChanged (e);
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
	}
}
