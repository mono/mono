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
//
// $Revision: 1.2 $
// $Modtime: $
// $Log: ScrollableControl.cs,v $
// Revision 1.2  2004/08/11 22:20:59  pbartok
// - Signature fixes
//
// Revision 1.1  2004/07/09 05:21:25  pbartok
// - Initial check-in
//
//

// NOT COMPLETE

using System;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {
	public class ScrollableControl : Control {
		#region Public Constructors
		public ScrollableControl() {
		}
		#endregion	// Public Constructors

		#region Protected Static Fields
		protected const int ScrollStateAutoScrolling = 1;
		protected const int ScrollStateFullDrag = 16;
		protected const int ScrollStateHScrollVisible = 2;
		protected const int ScrollStateUserHasScrolled = 8;
		protected const int ScrollStateVScrollVisible= 4;
		#endregion	// Protected Static Fields

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
			private DockPaddingEdges() {
			}
			#endregion	// DockPaddingEdges Constructor

			#region DockPaddingEdges Public Instance Properties
			public int All {
				get {
					return all;
				}

				set {
					all=value;
				}
			}

			public int Bottom {
				get {
					return bottom;
				}

				set {
					bottom=value;
				}
			}

			public int Left {
				get {
					return left;
				}

				set {
					left=value;
				}
			}

			public int Right {
				get {
					return right;
				}

				set {
					right=value;
				}
			}

			public int Top {
				get {
					return top;
				}

				set {
					top=value;
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
		#endregion

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
		#endregion

		#region Public Instance Properties
		public virtual bool AutoScroll {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		public Size AutoScrollMargin {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		public Size AutoScrollMinSize {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		public Point AutoScrollPosition {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		public override Rectangle DisplayRectangle {
			get {
				return base.DisplayRectangle;
				throw new NotImplementedException();
			}
		}

		public DockPaddingEdges DockPadding {
			get {
				throw new NotImplementedException();
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
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}

		protected bool VScroll {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
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
			return;
			throw new NotImplementedException();
		}

		protected override void OnMouseWheel(MouseEventArgs e) {
			throw new NotImplementedException();
		}

		protected override void OnVisibleChanged(EventArgs e) {
			throw new NotImplementedException();
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
