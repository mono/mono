//
// System.Windows.Forms.Form
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   ScrollableControl.DockPaddingEdges stub added by Jaak Simm (jaaksimm@firm.ee)
//	Dennis Hayes (dennish@raytek.com)
//   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
//
// (C) 2002 Ximian, Inc
//

using System;
using System.Drawing;
using System.ComponentModel;
namespace System.Windows.Forms {

	public class ScrollableControl : Control {

		//
		//  --- Constructor
		//
		public ScrollableControl () : base ()
		{
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
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//

		//
		//  --- Protected Properties
		//

		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
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
		

		//[MonoTODO]
		//public override ISite Site {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		//public IAsyncResult BeginInvoke(Delegate del)
		//{
		//	throw new NotImplementedException ();
		//}
		//public IAsyncResult BeginInvoke(Delegate del, object[] objs)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Dispose()
		//{
		//	throw new NotImplementedException ();
		//}

		//[MonoTODO]
		//public override bool Equals (object o)
		//{
		//	throw new NotImplementedException ();
		//}

		//public static bool Equals(object o1, object o2)
		//{
		//	throw new NotImplementedException ();
		//}

		//[MonoTODO]
		//public override int GetHashCode () {
		//	//FIXME add our proprities
		//	return base.GetHashCode ();
		//}

		//public void Invalidate()
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(Rectangle rect)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(Region reg)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(Rectangle rect, bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(Region reg, bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		//public object Invoke(Delegate del)
		//{
		//	throw new NotImplementedException ();
		//}
		//public object Invoke(Delegate del, object[] objs)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void PerformLayout()
		//{
		//	throw new NotImplementedException ();
		//}
		//public void PerformLayout(Control ctl, string str)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void ResumeLayout()
		//{
		//	throw new NotImplementedException ();
		//}
		//public void ResumeLayout(bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Scale(float val)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Scale(float val1, float val2)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Select()
		//{
		//	throw new NotImplementedException ();
		//}
		//public void Select(bool val1, bool val2)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void SetBounds(int val1, int val2, int val3, int val4)
		//{
		//	throw new NotImplementedException ();
		//}
		//public void SetBounds(int val1, int val2, int val3, int val4, BoundsSpecified bounds)
		//{
		//	throw new NotImplementedException ();
		//}
		//protected override void Dispose(bool disposing)
		//{
		//	throw new NotImplementedException ();
		//}
		//ContentAlignment RtlTranslateAlignment(ContentAlignment calign)
		//{
		//	throw new NotImplementedException ();
		//}
		//HorizontalAlignment RtlTranslateAlignment(HorizontalAlignment halign)
		//{
		//	throw new NotImplementedException ();
		//}
		//LeftRightAlignment RtlTranslateAlignment(LeftRightAlignment lralign)
		//{
		//	throw new NotImplementedException ();
		//}
		//protected void UpdateBounds()
		//{
		//	throw new NotImplementedException ();
		//}
		//protected void UpdateBounds(int val1, int val2, int val3, int val4)
		//{
		//	throw new NotImplementedException ();
		//}
		//protected void UpdateBounds(int val1, int val2, int val3, int val4, int val5, int val6)
		//{
		//	throw new NotImplementedException ();
		//}
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
				get { return all; }
				set { all=value; }
			}
			
			public int Bottom {
				get { return bottom; }
				set { bottom=value; }
			}
			
			public int Left {
				get { return left; }
				set { left=value; }
			}
			
			public int Right {
				get { return right; }
				set { right=value; }
			}
			
			public int Top {
				get { return top; }
				set { top=value; }
			}
			
			
			/// --- public Methods ---
			[MonoTODO]
			public override bool Equals (object other) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public override int GetHashCode () 
			{
				throw new NotImplementedException ();
			}
			
			/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
			[MonoTODO]
			object ICloneable.Clone () 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public override string ToString () 
			{
				throw new NotImplementedException ();
			}
		}
		
	}
}

