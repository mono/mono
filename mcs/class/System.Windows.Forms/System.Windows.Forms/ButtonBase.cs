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
		Label label;
		bool isPushed;

//		
//		// --- Constructor ---
		protected ButtonBase() : base() 
		{
			flatStyle = FlatStyle.Standard;
			image = null;
			imageAlign = ContentAlignment.MiddleCenter;
			imageIndex = -1;
			textAlign = ContentAlignment.MiddleCenter;
			imeMode = ImeMode.Inherit;
			isDefault = false;
			isPushed = false;
		}

		// --- Properties ---
		protected override CreateParams CreateParams {
			get { return base.CreateParams; }
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

		internal bool Pushed {
			get {
				return isPushed;
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
					Win32.UpdateWindowStyle(Handle, (int)0xF00, (int)Win32.ContentAlignment2SystemButtonStyle(textAlign));
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
		
		protected override void OnMouseDown (MouseEventArgs mevent) 
		{
			if ((mevent.Button & MouseButtons.Left) == MouseButtons.Left) {
				isPushed = true;
				Invalidate(); 
			}

			base.OnMouseDown (mevent);
		}
		
		protected override void OnMouseEnter (EventArgs eventargs) 
		{
				base.OnMouseEnter(eventargs);
				if( FlatStyle == FlatStyle.Flat || FlatStyle == FlatStyle.Popup) {
					Invalidate();
				}
		}
		
		protected override void OnMouseLeave (EventArgs eventargs) 
		{
			base.OnMouseLeave(eventargs);
			if( FlatStyle == FlatStyle.Flat || FlatStyle == FlatStyle.Popup) {
				Invalidate();
			}
		}
		
		protected override void OnMouseMove (MouseEventArgs mevent) 
		{
			base.OnMouseMove (mevent);
		}
		
		protected override void OnMouseUp (MouseEventArgs mevent) 
		{
			isPushed = false;
			Invalidate(); 
			base.OnMouseUp (mevent);
		}
		
		protected override void OnPaint (PaintEventArgs pevent) 
		{
			base.OnPaint (pevent);
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
						case (uint)ButtonNotification.BN_DOUBLECLICKED:
							OnClick(new ControlEventArgs(this));
							CallControlWndProc(ref m);
							break;
						default:
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
