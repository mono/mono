//
// System.Windows.Forms.Form
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   ScrollableControl.DockPaddingEdges stub added by Jaak Simm (jaaksimm@firm.ee)
//	Dennis Hayes (dennish@raytek.com)
//   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
//	CE Complete.
// (C) 2002 Ximian, Inc
//

using System;
using System.Drawing;
using System.ComponentModel;
namespace System.Windows.Forms {

	public class ScrollableControl : Control {

		private ScrollableControl.DockPaddingEdges dockPadding;

		//
		//  --- Constructor
		//
		public ScrollableControl () : base ()
		{
			dockPadding = new ScrollableControl.DockPaddingEdges();
		}
		//
		//  --- Public Properties
		//
		[MonoTODO]
		public virtual bool AutoScroll {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Size AutoScrollMargin {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Size AutoScrollMinSize {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Point AutoScrollPosition {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public override Rectangle DisplayRectangle {
			get {
				return base.DisplayRectangle;
			}
		}

		[MonoTODO]
		public ScrollableControl.DockPaddingEdges DockPadding {
			get {
				return dockPadding;
			}
		}

		//
		//  --- Protected Properties
		//

		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				createParams.Caption = "";
				createParams.ClassName = "mono_scrollable_control";
				createParams.X = Left;
				createParams.Y = Top;
				createParams.Width = Width;
				createParams.Height = Height;
				createParams.ClassStyle = 0;
				createParams.ExStyle = 0;
				createParams.Param = 0;
  				
				//if (parent != null)
				//	createParams.Parent = parent.Handle;
				//else 
					createParams.Parent = (IntPtr) 0;
	  
				createParams.Style = (int) Win32.WS_OVERLAPPEDWINDOW;
	  
				return createParams;
			}
		}

		[MonoTODO]
		protected bool HScroll {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected bool VScroll {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected virtual void AdjustFormScrollbars (
			bool displayScrollbars)
		{
			throw new NotImplementedException ();
		}

		protected override void OnLayout (LayoutEventArgs e) {
			base.OnLayout (e);
		}

		protected override void OnMouseWheel (MouseEventArgs e) {
			base.OnMouseWheel (e);
		}

		protected override void OnVisibleChanged (EventArgs e) {
			base.OnVisibleChanged (e);
		}

		protected override void ScaleCore (float dx, float dy) {
			base.ScaleCore (dx, dy);
		}

		protected override void WndProc (ref Message m) {
			base.WndProc (ref m);
		}
		

		/// ScrollableControl.DockPaddingEdges
		/// Determines the border padding for docked controls.
		
		public class DockPaddingEdges : ICloneable {
			// --- Fields ---
			int all;
			int bottom;
			int left;
			int right;
			int top;
			
			
			// --- public Properties ---
			public int All {
				get {
					return all;
				}
				set {
					all = value;
					left = value;
					right = value;
					bottom = value;
					top = value;
				}
			}
			
			public int Bottom {
				get { return bottom; }
				set { bottom = value; }
			}
			
			public int Left {
				get { return left; }
				set { left = value; }
			}
			
			public int Right {
				get { return right; }
				set { right = value; }
			}
			
			public int Top {
				get { return top; }
				set { top = value; }
			}
			
			
			/// --- public Methods ---

			/// <summary>
			///	Equality Operator
			/// </summary>
			///
			/// <remarks>
			///	Compares two DockPaddingEdges objects. The return value is
			///	based on the equivalence of the  
			///	properties of the two DockPaddingEdges.
			/// </remarks>

			public static bool operator == (DockPaddingEdges objA, DockPaddingEdges objB) {
				return ((objA.left == objB.left) && 
					(objA.right == objB.right) && 
					(objA.top == objB.top) && 
					(objA.bottom == objB.bottom) && 
					(objA.all == objB.all));
			} 			
			/// <summary>
			///	Equals Method
			/// </summary>
			///
			/// <remarks>
			///	Checks equivalence of this DockPaddingEdges and another object.
			/// </remarks>
		
			public override bool Equals (object obj) {
				if (!(obj is DockPaddingEdges))
					return false;

				return (this == (DockPaddingEdges) obj);
			}

			/// <summary>
			///	Inequality Operator
			/// </summary>
			///
			/// <remarks>
			///	Compares two DockPaddingEdges objects. The return value is
			///	based on the equivalence of the  
			///	properties of the two Sizes.
			/// </remarks>

			public static bool operator != (DockPaddingEdges objA, DockPaddingEdges objB) {
				return ((objA.left != objB.left) ||
					(objA.right != objB.right) ||
					(objA.top != objB.top) ||
					(objA.bottom != objB.bottom) ||
					(objA.all != objB.all));
			} 		
			/// <summary>
			///	GetHashCode Method
			/// </summary>
			///
			/// <remarks>
			///	Calculates a hashing value.
			/// </remarks>
		
			public override int GetHashCode () {
				unchecked{
					return all * top * bottom * right * left;
				}
			}

			
			/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
			object ICloneable.Clone () 
			{
				DockPaddingEdges dpe = new DockPaddingEdges();
				dpe.all = all;
				dpe.top = top;
				dpe.right = right;
				dpe.bottom = bottom;
				dpe.left = left;
				return (object) dpe;
			}
			
			/// <summary>
			///	ToString Method
			/// </summary>
			///
			/// <remarks>
			///	Formats the DockPaddingEdges as a string.
			/// </remarks>
		
			public override string ToString () 
			{
				return "All = " + all.ToString() + " Top = " + top.ToString() + 
					" Right = " + right.ToString() + " Bottom = " + bottom.ToString() + 
					" Left = " + left.ToString();
			}
		}
		
	}
}

