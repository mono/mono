//
// System.Windows.Forms.LinkLabel.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Collections;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

    public class LinkLabel : Label, IButtonControl {

		//
		//  --- Constructor
		//
		//[MonoTODO]
		//public LinkLabel()
		//{
		//	throw new NotImplementedException ();
		//}

		//
		//  --- Public Properties
		//
		//[MonoTODO]
		//public Color ActiveLinkcolor {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public Color DisabledLinkColor {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public LinkArea LinkArea {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public LinkBehavior LinkBehavior {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public Color LinkColor {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public LinkLabel.LinkCollection Links {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public bool LinkVisited {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
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
		//[MonoTODO]
		//public override string Text {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public Color VisitedLinkColor {
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
		//public void Select(bool val2, bool val2)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void SetBounds(int b1, int b2, int b3, int b4)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void SetBounds(int b1, int b2, int b3, int b4, int b5, int b6)
		//{
		//	throw new NotImplementedException ();
		//}

		//
		//  --- Public Events
		//
		//[MonoTODO]
		//public event LinkLabelLinkClickedEventHandler LinkClicked {
		//	add {
		//		throw new NotImplementedException ();
		//	}
		//	remove {
		//		throw new NotImplementedException ();
		//	}
		//}

		//
		//  --- Protected Properties
		//
		//[MonoTODO]
		//protected override DefaultImeMode {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}

		//
		//  --- Protected Methods
		//
		//[MonoTODO]
		//protected override AccessabilityObject CreateAccessabilityInstance()
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void CreateHandle()
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override virtual void Dispose(bool val)
		//{
		//	throw new NotImplementedException ();
		//}

		//[MonoTODO]
		//protected override void OnEnabledChanged(EventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void OnFontChanged( EventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void OnGotFocus( EventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void OnKeyDown (KeyEventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void OnLostFocus (EventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void OnMouseDown (MouseEventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		// I think that this should be 'MouseEventArgs' 
		// but the documentation says EventArgs.
		//[MonoTODO]
		//protected override void OnMouseLeave(EventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void OnMouseMove (MouseEventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void OnMouseUp (MouseEventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void OnPaint (PaintEventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void OnPaintBackground(PaintEventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void OnTextAlignChanged( EventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void OnTextChanged( EventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}

		//[MonoTODO]
		//protected override bool ProcessDialogKey(Keys keyData)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected ContentAlignment RtlTranslateAlignment(ContentAlignment align)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected HorizontalAlignment RtlTranslateAlignment( HorizontalAlignment align)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected LeftRightAlignment RtlTranslateAlignment( LeftRightAlignment align)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void Select(bool val1, bool val2)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void SetBoundsCare(  int x, int y, int width, int height,  BoundsSpecified specified)
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
		//protected override WndProc(ref Message msg)
		//{
		//	throw new NotImplementedException ();
		//}
//
// System.Windows.Forms.LinkLabel.LinkCollection.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//
// (C) 2002 Ximian, Inc
//

			// <summary>
			//	This is only a template.  Nothing is implemented yet.
			//
			// </summary>

			public class LinkCollection :  IList, ICollection, IEnumerable {

			//
			//  --- Constructor
			//
			//[MonoTODO]
			//public LinkLabel.LinkCollection(LinkLabel owner)
			//{
			//	throw new NotImplementedException ();
			//}

			//
			//  --- Public Properties
			//
			//[MonoTODO]
			//public int Count {
			//	get {
			//		throw new NotImplementedException ();
			//	}
			//}
			//[MonoTODO]
			//public bool IsReadOnly {
			//	get {
			//		throw new NotImplementedException ();
			//	}
			//}
			//[MonoTODO]
			//public virtual LinkLabel.Link this[ int index]  {
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
			//public Link Add(int val1, int val2)
			//{
			//	throw new NotImplementedException ();
			//}
			//[MonoTODO]
			//public Link Add(int val1, int val2, object o)
			//{
			//	throw new NotImplementedException ();
			//}
			//[MonoTODO]
			//public virtual void Clear()
			//{
			//	throw new NotImplementedException ();
			//}
			//[MonoTODO]
			//public bool Contains(LinkLabel.Link link)
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
			//public IEnumerator GetEnumerator()
			//{
			//	throw new NotImplementedException ();
			//}
			//[MonoTODO]
			//public int IndexOf(LinkLabel.Link link)
			//{
			//	throw new NotImplementedException ();
			//}
			//[MonoTODO]
			//public void Remove(LinkLabel.Link value)
			//{
			//	throw new NotImplementedException ();
			//}
			//[MonoTODO]
			//public void RemoveAt(int index)
			//{
			//	throw new NotImplementedException ();
			//}
		}//End of subclass
//
// System.Windows.Forms.LinkLabel.Link
//
// Author:
//   stubbed out by Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

			// <summary>
			//	This is only a template.  Nothing is implemented yet.
			//
			// </summary>
			[MonoTODO]
			public class Link {
			//	throw new NotImplementedException ();
		}//End of subclass
	}// End of Class
}
