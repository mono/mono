//
// System.Windows.Forms.Form
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//
// (C) 2002 Ximian, Inc
//

using System;
using System.Drawing;
using Gtk;
using GtkSharp;

namespace System.Windows.Forms {
	public class ScrollableControl : Control {
		//
		//  --- Constructor
		//
		//[MonoTODO]
		//public ScrollableControl()
		//{
		//	throw new NotImplementedException ();
		//}

		//
		//  --- Public Properties
		//
		//[MonoTODO]
		//public virtual bool AutoScroll {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public Size AutoScrollMargin {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public Size AutoScrollMinSize {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public Point AutoScrollPosition {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public override Rectangle DisplayRectangle {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public ScrollableControl.DockPaddingEdges DockPadding {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public override ISite Site {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}

		//
		//  --- Public Methods
		//
		//[MonoTODO]
		//public IAsyncResult BeginInvoke(Delegate del)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public IAsyncResult BeginInvoke(Delegate del, object[] objs)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void Dispose()
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public virtual bool Equals(object o);
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public static bool Equals(object o1, object o2);
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void Invalidate()
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void Invalidate(bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void Invalidate(Rectangle rect)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void Invalidate(Region reg)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void Invalidate(Rectangle rect, bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void Invalidate(Region reg, bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public object Invoke(Delegate del)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public object Invoke(Delegate del, object[] objs)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void PerformLayout()
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void PerformLayout(Control ctl, string str)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void ResumeLayout()
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void ResumeLayout(bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void Scale(float val)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void Scale(float val1, float val2)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void Select()
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void Select(bool val1, bool val2)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void SetBounds(int val1, int val2, int val3, int val4)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void SetBounds(int val1, int val2, int val3, int val4, BoundsSpecified bounds)
		//{
		//	throw new NotImplementedException ();
		//}

		//
		//  --- Protected Properties
		//
		//[MonoTODO]
		//protected override CreateParams CreateParams {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//protected bool HScroll {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//protected bool VScroll {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}

		//
		//  --- Protected Methods
		//
		//[MonoTODO]
		//protected virtual void AdjustFormScrollbars( bool displayScrollbars)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void Dispose(bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void OnLayout(LayoutEventArgs levent)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void OnMouseWheel(MouseEventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void OnVisibleChanged(EventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//ContentAlignment RtlTranslateAlignment(ContentAlignment calign)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//HorizontalAlignment RtlTranslateAlignment(HorizontalAlignment halign)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//LeftRightAlignment RtlTranslateAlignment(LeftRightAlignment lralign)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void ScaleCore(float dx, float dy)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected void UpdateBounds()
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected void UpdateBounds(int val1, int val2, int val3, int val4)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected void UpdateBounds(int val1, int val2, int val3, int val4, int val5, int val6)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void WndProc(ref Message m)
		//{
		//	throw new NotImplementedException ();
		//}
	 }
}
