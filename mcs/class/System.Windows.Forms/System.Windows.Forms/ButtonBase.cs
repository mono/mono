//
// System.Windows.Forms.ButtonBase.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	  implemented for Gtk+ by Rachel Hestilow (hestilow@ximian.com)
//   Dennis Hayes (dennish@Raytek.com)
//   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
//
// (C) Ximian, Inc., 2002
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
		CreateParams createParams;
		Label label;
		protected StringAlignment	horizontalAlign_;
		protected StringAlignment	verticalAlign_;
		internal ButtonStyles	sysButtonStyles_;

//		
//		// --- Constructor ---
		protected ButtonBase() : base() 
		{
			flatStyle = FlatStyle.Standard;
			image = null;
			imageAlign = ContentAlignment.MiddleCenter;
			imageIndex = -1;
			textAlign = ContentAlignment.MiddleCenter;
			horizontalAlign_ = StringAlignment.Center;
			verticalAlign_ = StringAlignment.Center;
			sysButtonStyles_ = ButtonStyles.BS_CENTER | ButtonStyles.BS_VCENTER;
			imeMode = ImeMode.Inherit;
			isDefault = false;
		}

		// --- Properties ---
		protected override CreateParams CreateParams {
			get { return createParams; }
		}
		
		protected override ImeMode DefaultImeMode {
			get {
				return ImeMode.Inherit;
			}
		}
		
		protected override Size DefaultSize {
			get {
				return new Size(75,23);// default size for button.
			}
		}
		
		public FlatStyle FlatStyle {
			get { return flatStyle; }
			set { 
				if( flatStyle != value) {
					flatStyle = value; 

					if( flatStyle == FlatStyle.System) {
						Win32.UpdateWindowStyle(Handle, (int)ButtonStyles.BS_OWNERDRAW, 0);
					}
					else {
						Win32.UpdateWindowStyle(Handle, 0, (int)ButtonStyles.BS_OWNERDRAW);
					}
					Invalidate();
				}
			}
		}
		
		public Image Image {
			get { return image; }
			set { 
				image = value; 
				Invalidate();
			}
		}
		
		public ContentAlignment ImageAlign {
			get { return imageAlign; }
			set { 
				if( imageAlign != value) {
					imageAlign = value;
					Invalidate();
				}
			}
		}
		
		public int ImageIndex {
			get { return imageIndex; }
			set { imageIndex=value; }
		}
		
		public new ImeMode ImeMode {
			get {
				return imeMode; }
			set {
				imeMode = value;
			}
		}
		
		protected bool IsDefault {
			get {
				return isDefault;
			}
			set {
				isDefault = value;
			}
		}

		[MonoTODO]
		public virtual ContentAlignment TextAlign {
			get { 
				return textAlign; 
			}
			set { 
				if( textAlign != value) {
					textAlign = value;
					sysButtonStyles_ = 0;
					if( textAlign == ContentAlignment.BottomCenter ||
						textAlign == ContentAlignment.BottomLeft ||
						textAlign == ContentAlignment.BottomRight) {
						verticalAlign_ = StringAlignment.Far;
						sysButtonStyles_ |= ButtonStyles.BS_BOTTOM;
					}
					else if(textAlign == ContentAlignment.TopCenter ||
						textAlign == ContentAlignment.TopLeft ||
						textAlign == ContentAlignment.TopRight) {
						verticalAlign_ = StringAlignment.Near;
						sysButtonStyles_ |= ButtonStyles.BS_TOP;
					}
					else {
						verticalAlign_ = StringAlignment.Center;
						sysButtonStyles_ |= ButtonStyles.BS_VCENTER;
					}

					if( textAlign == ContentAlignment.BottomLeft ||
						textAlign == ContentAlignment.MiddleLeft ||
						textAlign == ContentAlignment.TopLeft) {
						horizontalAlign_ = StringAlignment.Near;
						sysButtonStyles_ |= ButtonStyles.BS_LEFT;
					}
					else if(textAlign == ContentAlignment.BottomRight ||
						textAlign == ContentAlignment.MiddleRight ||
						textAlign == ContentAlignment.TopRight) {
						horizontalAlign_ = StringAlignment.Far;
						sysButtonStyles_ |= ButtonStyles.BS_RIGHT;
					}
					else {
						horizontalAlign_ = StringAlignment.Center;
						sysButtonStyles_ |= ButtonStyles.BS_CENTER;
					}

					Win32.UpdateWindowStyle(Handle, (int)0xF00, (int)sysButtonStyles_);
					Invalidate();
				}
			}
		}

		/// --- Methods ---
		/// internal .NET framework supporting methods, not stubbed out:
		/// - protected override void Dispose(bool);
		/// - protected void ResetFlagsandPaint();
		
		
		// I do not think this is part of the spec.
		//protected override AccessibleObject CreateAccessibilityInstance() 
		//{
		//	throw new NotImplementedException ();
		//}
		
		/// [methods for events]
		protected override void OnEnabledChanged (EventArgs e) 
		{
			base.OnEnabledChanged (e);
		}
		
		protected override void OnGotFocus (EventArgs e) 
		{
			base.OnGotFocus (e);
		}
		
		protected override void OnKeyDown (KeyEventArgs kevent) 
		{
			base.OnKeyDown (kevent);
		}
		
		protected override void OnKeyUp (KeyEventArgs kevent) 
		{
			base.OnKeyUp (kevent);
		}
		
		protected override void OnLostFocus (EventArgs e) 
		{
			base.OnLostFocus (e);
		}
		
		protected override void OnMouseDown (MouseEventArgs e) 
		{
			base.OnMouseDown (e);
		}
		
		protected override void OnMouseEnter (EventArgs e) 
		{
			base.OnMouseEnter (e);
		}
		
		protected override void OnMouseLeave (EventArgs e) 
		{
			base.OnMouseLeave (e);
		}
		
		protected override void OnMouseMove (MouseEventArgs e) 
		{
			base.OnMouseMove (e);
		}
		
		protected override void OnMouseUp (MouseEventArgs e) 
		{
			base.OnMouseUp (e);
		}
		
		protected override void OnPaint (PaintEventArgs e) 
		{
			base.OnPaint (e);
		}
		
		protected override void OnParentChanged (EventArgs e) 
		{
			base.OnParentChanged (e);
		}
		
		protected override void OnTextChanged (EventArgs e) 
		{
			base.OnTextChanged (e);
		}
		
		protected override void OnVisibleChanged (EventArgs e) 
		{
			base.OnVisibleChanged (e);
		}
		/// end of [methods for events]
		
		protected override void WndProc (ref Message m) 
		{
			switch (m.Msg) {
				case Msg.WM_COMMAND: {
					switch(m.HiWordWParam) {
						case (uint)ButtonNotification.BN_CLICKED:
							OnClick(new ControlEventArgs(this));
							CallControlWndProc(ref m);
							break;
					}
					break;
				}
				default:
					base.WndProc (ref m);
					break;
			}
		}


		/// --- ButtonBase.ButtonBaseAccessibleObject ---
		/// the class is not stubbed, cause it's only used for .NET framework
	}
}
