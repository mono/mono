//
// System.Windows.Forms.ButtonBase.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	  implemented for Gtk+ by Rachel Hestilow (hestilow@ximian.com)
//   Dennis Hayes (dennish@Raytek.com)
//   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
//   Alexandre Pigokine (pigolkine@gmx.de)
//
// (C) Ximian, Inc., 2002/3
//

using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Implements the basic functionality common to button controls.
	/// </summary>

	public abstract class ButtonBase : Control {

		// private fields
		FlatStyle flatStyle;
		Image image;
		ContentAlignment imageAlign;
		int imageIndex;
		ContentAlignment textAlign;
		ImeMode imeMode;
		bool isDefault;
		bool isPushed;
		
		protected Label label;

//		
//		// --- Constructor ---
		protected ButtonBase() : base(){
			label = new Label();
			label.Text = Text;
			label.Visible = true;
			flatStyle = FlatStyle.Standard;
			image = null;
			imageAlign = ContentAlignment.MiddleCenter;
			imageIndex = -1;
			textAlign = ContentAlignment.MiddleCenter;
			imeMode = ImeMode.Inherit;
			isDefault = false;
			//isPushed = false;
		}

		// --- Properties ---
		//protected override CreateParams CreateParams {
		//	get { return base.CreateParams; }
		//}
		
		//protected override ImeMode DefaultImeMode {
		//	get {
		//		return ImeMode.Inherit;
		//	}
		//}
		
		public override String Text{
			get { return label.Text; }
			set{ label.Text = value;}
		}
		public override Color ForeColor {
			set{label.ForeColor = value;}
		}
		public override Font Font{
			set{label.Font = value;}
		}
		protected override Size DefaultSize {
			get {
				return new Size(75,23);// default size for button.
			}
		}
		[MonoTODO]
		public FlatStyle FlatStyle {
			get { return flatStyle; }
			set { flatStyle = value;}
		}
		[MonoTODO]
		public Image Image {
			get { return image; }
			set { 
				image = value; 
				Invalidate();
			}
		}
		[MonoTODO]
		public ImageList ImageList {
			get {throw new NotImplementedException ();}
			set{
				//fixme:
			}
		}
		[MonoTODO]
		public ContentAlignment ImageAlign {
			get { return imageAlign; }
			set { 
				if( imageAlign != value) {
					imageAlign = value;
					Invalidate();
				}
			}
		}
		[MonoTODO]
		public int ImageIndex {
			get { return imageIndex; }
			set { imageIndex=value; }
		}
		[MonoTODO]
		public new ImeMode ImeMode {
			get {return imeMode; }
			set {imeMode = value;}
		}
		[MonoTODO]
		protected bool IsDefault {
			get {return isDefault;}
			set {isDefault = value;}
		}

		//internal bool Pushed {
		//	get {return isPushed;}
		//}
		
		[MonoTODO]
		public virtual ContentAlignment TextAlign {
			get { return textAlign; }
			set { 
				if( textAlign != value) {
					textAlign = value;
					Invalidate();
				}
			}
		}

		/// --- Methods ---
		//protected override void Dispose(bool disposing){
		//	base.Dispose(disposing);
		//}

		protected void ResetFlagsandPaint(){
		}
		
		
		//protected override AccessibleObject CreateAccessibilityInstance() 
		//{
		//	return base.CreateAccessibilityInstance();
		//}
		
		/// [methods for events]
		//protected override void OnEnabledChanged (EventArgs e) 
		//{
		//	base.OnEnabledChanged (e);
		//}
		
		//protected override void OnGotFocus (EventArgs e) 
		//{
		//	base.OnGotFocus (e);
		//}
		
		//protected override void OnKeyDown (KeyEventArgs kevent) 
		//{
		//	base.OnKeyDown (kevent);
		//}
		
		//protected override void OnKeyUp (KeyEventArgs kevent) 
		//{
		//	base.OnKeyUp (kevent);
		//}
		
		//protected override void OnLostFocus (EventArgs e) 
		//{
		//	base.OnLostFocus (e);
		//}
		
		//protected override void OnMouseDown (MouseEventArgs mevent) 
		//{
		//	if ((mevent.Button & MouseButtons.Left) == MouseButtons.Left) {
		//		isPushed = true;
		//		Invalidate(); 
		//	}

		//	base.OnMouseDown (mevent);
		//}
		
		//protected override void OnMouseEnter (EventArgs eventargs) 
		//{
		//		base.OnMouseEnter(eventargs);
		//		if( FlatStyle == FlatStyle.Flat || FlatStyle == FlatStyle.Popup) {
		//			Invalidate();
		//		}
		//}
		
		//protected override void OnMouseLeave (EventArgs eventargs) 
		//{
		//	base.OnMouseLeave(eventargs);
		//	if( FlatStyle == FlatStyle.Flat || FlatStyle == FlatStyle.Popup) {
		//		Invalidate();
		//	}
		//}
		
		//protected override void OnMouseMove (MouseEventArgs mevent) 
		//{
		//	base.OnMouseMove (mevent);
		//}
		
		//protected override void OnMouseUp (MouseEventArgs mevent) 
		//{
		//	isPushed = false;
		//	Invalidate(); 
		//	base.OnMouseUp (mevent);
		//}
		
		//internal virtual void ButtonPaint (PaintEventArgs pevent) {
		//}

		//protected override void OnPaint (PaintEventArgs pevent) 
		//{
		//	base.OnPaint (pevent);
		//	ButtonPaint (pevent);
		//}
		
		//protected override void OnParentChanged (EventArgs e) 
		//{
		//	base.OnParentChanged (e);
		//}
		
		//protected override void OnTextChanged (EventArgs e) 
		//{
		//	base.OnTextChanged (e);
		//}
		
		//protected override void OnVisibleChanged (EventArgs e) 
		//{
		//	base.OnVisibleChanged (e);
		//}
		/// end of [methods for events]
		
	}
}
