//
// System.Windows.Forms.Button.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	  implemented for Gtk+ by Rachel Hestilow (hestilow@ximian.com)
//	Dennis Hayes (dennish@raytek.com)
//   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
//	Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) Ximian, Inc., 2002/3
//

using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows button control.
	/// </summary>

	public class Button : ButtonBase, IButtonControl {

		// private fields
		DialogResult dialogResult;
		bool MouseIsOver_;

		// --- Constructor ---
		public Button() : base()
		{
			dialogResult = DialogResult.None;
			MouseIsOver_ = false;
			SubClassWndProc_ = true;
			SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
		}
		
		// --- Properties ---
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;

				createParams.ClassName = "BUTTON";

				createParams.Style = (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE |
					WindowStyles.WS_CLIPSIBLINGS );
				createParams.Style |= (int)Win32.ContentAlignment2SystemButtonStyle(TextAlign);
				//createParams.Style |= (int) ButtonStyles.BS_OWNERDRAW;
				// CHECKME : this call is commented because (IMHO) Control.CreateHandle suppose to do this
				// and this function is CreateParams, not CreateHandle
				// window.CreateHandle (createParams);
				return createParams;
			}
		}
		
		// --- IButtonControl property ---
		public virtual DialogResult DialogResult {
			get { return dialogResult; }
			set { 
				if ( !Enum.IsDefined ( typeof(DialogResult), value ) )
					throw new System.ComponentModel.InvalidEnumArgumentException( "DialogResult",
						(int)value,
						typeof(DialogResult));

				dialogResult = value;
			}
		}

		// --- IButtonControl method ---
		public virtual void NotifyDefault(bool value) 
		{
			//Sanity check...bail out if this ain't a window.
			if (!Win32.IsWindow(this.Handle)) return;
		
			//Figure what the caller wants, and set the value accordingly
			uint nDefPushButton;//this will hold the value of either BS_DEFPUSHBUTTON or it's logial opposite
			if (value)
				nDefPushButton = (uint)ButtonStyles.BS_DEFPUSHBUTTON;
			else   //Ok, so we gotta find the bitwise opposite of BS_DEFPUSHBUTTON;
				nDefPushButton = 0xFFFFFFFF ^ (uint)ButtonStyles.BS_DEFPUSHBUTTON; // a number's opposite is determined by xor-ing it with a full house of 1's, and since uint is 4Bytes that's FFFFFFFF
			
			uint nOldStyle = (uint)Win32.GetWindowLong(this.Handle, GetWindowLongFlag.GWL_STYLE);
			uint nNewStyle = nDefPushButton & nOldStyle; //Our chosen value, BitAnd-ed with the existing style
			Win32.SendMessage(this.Handle, (Msg)ButtonMessages.BM_SETSTYLE, (int)nNewStyle, (int)1);
		}
		
		public void PerformClick() 
		{
			EventArgs e = new EventArgs();
 			OnClick(e);
		}
		
		// --- Button methods for events ---
		protected override void OnClick(EventArgs e) 
		{
			if ( DialogResult != DialogResult.None ) {
				Form parent = Parent as Form;
				if ( parent != null )
					parent.DialogResult = this.DialogResult;
			}
			base.OnClick (e);
		}
		
		protected override void OnMouseUp(MouseEventArgs mevent) 
		{
			base.OnMouseUp (mevent);
		}
		
		// --- Button methods ---
		protected override bool ProcessMnemonic (char charCode) 
		{
			return base.ProcessMnemonic (charCode);
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString();
		}

		//protected virtual void OnMouseEnter (EventArgs mevent) {
		//	base.OnMouseEnter(mevent);
		//}
    
		//protected virtual void OnMouseLeave (EventArgs mevent) {
		//	base.OnMouseLeave(mevent);
		//}

		protected virtual void OnPaint (PaintEventArgs pevent) {

			Rectangle paintBounds = ClientRectangle;
			Bitmap bmp = new Bitmap( paintBounds.Width, paintBounds.Height,pevent.Graphics);
			Graphics paintOn = Graphics.FromImage(bmp);
			
			Color controlColor = BackColor; //SystemColors.Control;
			Color textColor = ForeColor; // SystemColors.ControlText;
			//Graphics paintOn = e.Graphics;
			Rectangle rc = paintBounds;
			Rectangle rcImageClip = paintBounds;
			rcImageClip.Inflate(-2,-2);

			if( FlatStyle == FlatStyle.Flat) {
				if( Pushed) {
					SolidBrush sb = new SolidBrush(ControlPaint.Light(controlColor));
					paintOn.FillRectangle(sb, rc);
					sb.Dispose();
				}
				else {
					if( mouseIsInside_) {
						SolidBrush sb = new SolidBrush(ControlPaint.Dark(controlColor));
						paintOn.FillRectangle(sb, rc);
						sb.Dispose();
					}
					else {
						SolidBrush sb = new SolidBrush( controlColor);
						paintOn.FillRectangle(sb, rc);
						sb.Dispose();
					}
				}

				ControlPaint.DrawBorder(paintOn, rc, textColor, ButtonBorderStyle.Solid);
				rc.Inflate(-1,-1);

				if( Focused) {
					ControlPaint.DrawBorder(paintOn, rc, textColor, ButtonBorderStyle.Solid);
				}
				else {
					rcImageClip.Inflate(1,1);
				}
				rc.Inflate(-3,-3);
			}
			else if( FlatStyle == FlatStyle.Popup) {
				if( Pushed) {
					ControlPaint.DrawBorder(paintOn, rc, textColor, ButtonBorderStyle.Solid);
					rc.Inflate(-1,-1);
					ControlPaint.DrawBorder(paintOn, rc, textColor, ButtonBorderStyle.Solid);
					rc.Inflate(-1,-1);
				}
				else {
					if( Focused) {
						ControlPaint.DrawBorder(paintOn, rc, textColor, ButtonBorderStyle.Solid);
						rc.Inflate(-1,-1);
					}

					if( mouseIsInside_) {
						Color colorLight = ControlPaint.Light(controlColor);
						ControlPaint.DrawBorder(paintOn, rc, colorLight, 1, ButtonBorderStyle.Solid,
							colorLight, 1, ButtonBorderStyle.Solid, textColor, 1, ButtonBorderStyle.Solid,
							textColor, 1, ButtonBorderStyle.Solid);
					}
					else {
						ControlPaint.DrawBorder(paintOn, rc, textColor, ButtonBorderStyle.Solid);
					}
					rc.Inflate(-1,-1);
				}
				SolidBrush sb = new SolidBrush( controlColor);
				paintOn.FillRectangle(sb, rc);
				sb.Dispose();

				rc.Inflate(-1,-1);
			}
			else {
				ButtonState	btnState = ButtonState.Normal;

				if( Pushed) {
					btnState = ButtonState.Pushed;
				}

				ControlPaint.DrawButton(paintOn, rc, btnState);

				rc.Inflate(-2,-2);
			}

			// Do not place Text and Images on the borders 
			paintOn.Clip = new Region(rcImageClip);
			if( BackgroundImage != null) {
				paintOn.DrawImage(BackgroundImage, 0, 0, BackgroundImage.Width, BackgroundImage.Height);
			}

			// Make "Focus" rectangle
			rc.Inflate(-3,-3);
			Rectangle	focusRC = rc;

			if(Image != null) {
				int X = rc.X;
				int Y = rc.Y;

				if( ImageAlign == ContentAlignment.TopCenter ||
					ImageAlign == ContentAlignment.MiddleCenter ||
					ImageAlign == ContentAlignment.BottomCenter) {
					X += (rc.Width - Image.Width) / 2;
				}
				else if(ImageAlign == ContentAlignment.TopRight ||
					ImageAlign == ContentAlignment.MiddleRight||
					ImageAlign == ContentAlignment.BottomRight) {
					X += (rc.Width - Image.Width);
				}

				if( ImageAlign == ContentAlignment.BottomCenter ||
					ImageAlign == ContentAlignment.BottomLeft ||
					ImageAlign == ContentAlignment.BottomRight) {
					Y += rc.Height - Image.Height;
				}
				else if(ImageAlign == ContentAlignment.MiddleCenter ||
						ImageAlign == ContentAlignment.MiddleLeft ||
						ImageAlign == ContentAlignment.MiddleRight) {
					Y += (rc.Height - Image.Height) / 2;
				}
				paintOn.DrawImage(Image, X, Y, Image.Width, Image.Height);
			}

			if( Pushed) {
				// FlatStyle.Flat uses color and not text offset to show state
				// FIXME: use SysMetrics to determine offset values ?
				if( FlatStyle != FlatStyle.Flat) {
					rc.Offset(1,1);
				}
			}

			// DrawString does not paint _ under character, so we can use Win32 function call
			if( Enabled) {
				Win32.DrawText(paintOn, Text, Font, textColor, rc, TextAlign);
			}
			else {
				ControlPaint.DrawStringDisabled(paintOn, Text, Font, textColor, rc, Win32.ContentAlignment2StringFormat(TextAlign));
			}

			if( Focused) {
				// FIXME: Draw focus rectangle in different colors
				ControlPaint.DrawFocusRectangle( paintOn, focusRC);
			}
			pevent.Graphics.DrawImage(bmp, 0, 0, paintBounds.Width, paintBounds.Height);
			paintOn.Dispose ();
			bmp.Dispose();
		}

		protected override void WndProc (ref Message m) {
			switch (m.Msg) {
				case Msg.WM_DRAWITEM: {
					m.Result = (IntPtr)1;
				}
					break;
				default:
					base.WndProc (ref m);
					break;
			}
		}
		
		/// --- Button events ---
		/// commented out, cause it only supports the .NET Framework infrastructure
		/*
		[MonoTODO]
		public new event EventHandler DoubleClick {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		*/
	}
}

