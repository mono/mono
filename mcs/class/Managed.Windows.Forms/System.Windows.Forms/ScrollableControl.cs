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


// NOT COMPLETE

using System;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {
	public class ScrollableControl : Control {
		#region Local Variables
		private bool			auto_vscroll;
		private bool			auto_hscroll;
		private bool			hscroll_visible;
		private bool			vscroll_visible;
		private bool			auto_scroll;
		private Size			auto_scroll_margin;
		private Size			auto_scroll_min_size;
		private Point			auto_scroll_position;
		private DockPaddingEdges	dock_padding;
		private ScrollBar		hscrollbar;
		private ScrollBar		vscrollbar;
		#endregion	// Local Variables

		[MonoTODO("Need to use the edge values when performing the layout")]
		#region Subclass DockPaddingEdges
		public class DockPaddingEdges : ICloneable {
			#region DockPaddingEdges Local Variables
			private int all;
			private int left;
			private int right;
			private int top;
			private int bottom;
			#endregion	// DockPaddingEdges Local Variables

			#region DockPaddingEdges Constructor
			internal DockPaddingEdges() {
				all = 0;
				left = 0;
				right = 0;
				top = 0;
				bottom = 0;
			}
			#endregion	// DockPaddingEdges Constructor

			#region DockPaddingEdges Public Instance Properties
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
				}
			}

			public int Bottom {
				get {
					return bottom;
				}

				set {
					bottom = value;
					all = 0;
				}
			}

			public int Left {
				get {
					return left;
				}

				set {
					left=value;
					all = 0;
				}
			}

			public int Right {
				get {
					return right;
				}

				set {
					right=value;
					all = 0;
				}
			}

			public int Top {
				get {
					return top;
				}

				set {
					top=value;
					all = 0;
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

			object ICloneable.Clone() {
				DockPaddingEdges padding_edge;

				padding_edge=new DockPaddingEdges();

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
				throw new NotImplementedException();
			}

			public override bool GetPropertiesSupported(System.ComponentModel.ITypeDescriptorContext context) {
				throw new NotImplementedException();
			}
		}
		#endregion	// Subclass DockPaddingEdgesConverter

		#region Public Constructors
		public ScrollableControl() {
			base.SetStyle(ControlStyles.ContainerControl, true);
			auto_scroll = false;
			auto_hscroll = false;
			auto_vscroll = false;
			hscroll_visible = false;
			vscroll_visible = false;
			auto_scroll_margin = new Size(0, 0);
			auto_scroll_min_size = new Size(0, 0);
			auto_scroll_position = new Point(0, 0);
			dock_padding = new DockPaddingEdges();

			hscrollbar = new ScrollBar();
			hscrollbar.Visible = false;

			vscrollbar = new ScrollBar();
			vscrollbar.Visible = false;
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
		public virtual bool AutoScroll {
			get {
				return	auto_scroll;
			}

			set {
				if (auto_scroll == value) {
					return;
				}

				auto_scroll = value;
			}
		}

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

		public Size AutoScrollMinSize {
			get {
				return auto_scroll_min_size;
			}

			set {
				auto_scroll_min_size = value;
			}
		}

		public Point AutoScrollPosition {
			get {
				return auto_scroll_position;
			}

			set {
				auto_scroll_position = value;
			}
		}

		public override Rectangle DisplayRectangle {
			get {
				Rectangle	rect;

				rect = base.DisplayRectangle;

				if (vscroll_visible) {
					rect.Width -= vscrollbar.Width;
					if (rect.Width < 0) {
						rect.Width = 0;
					}
				}

				if (hscroll_visible) {
					rect.Height -= hscrollbar.Height;
					if (rect.Height < 0) {
						rect.Height = 0;
					}
				}
				return rect;
			}
		}

		public DockPaddingEdges DockPadding {
			get {
				return dock_padding;
			}

			// DockPadding is documented as 'get' only ( http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/html/frlrfSystemWindowsFormsScrollableControlClassAutoScrollTopic.asp )
			// but Microsoft's on that pageexamples show 'set' usage
			set {
				dock_padding = value;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
		protected override CreateParams CreateParams {
			get {
				CreateParams	ret;

				ret = base.CreateParams;

				ret.Style |= (int)(WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_VISIBLE);

				return ret;
			}
		}

		protected bool HScroll {
			get {
				return hscroll_visible;
			}

			set {
				if (hscroll_visible != value) {
					hscroll_visible = value;
				}
			}
		}

		protected bool VScroll {
			get {
				return vscroll_visible;
			}

			set {
				if (vscroll_visible != value) {
					vscroll_visible = value;
				}
			}
		}
		#endregion	// Protected Instance Methods

		#region Public Instance Methods
		public void ScrollControlIntoView(Control activeControl) {
		}

		public void SetAutoScrollMargin(int x, int y) {
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected virtual void AdjustFormScrollbars(bool displayScrollbars) {
		}

		protected bool GetScrollState(int bit) {
			throw new NotImplementedException();
		}

		protected override void OnLayout(LayoutEventArgs levent) {
			base.OnLayout(levent);
		}

		protected override void OnMouseWheel(MouseEventArgs e) {
			base.OnMouseWheel(e);
		}

		protected override void OnVisibleChanged(EventArgs e) {
			;; // Nothing to do yet
		}

		protected override void ScaleCore(float dx, float dy) {
			throw new NotImplementedException();
		}

		protected void SetDisplayRectLocation(int x, int y) {
			throw new NotImplementedException();
		}

		protected void SetScrollState(int bit, bool value) {
			throw new NotImplementedException();
		}

		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);
		}
		#endregion	// Protected Instance Methods
	}
}
