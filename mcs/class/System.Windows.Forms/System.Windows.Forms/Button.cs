//
// System.Windows.Forms.Button.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	  implemented for Gtk+ by Rachel Hestilow (hestilow@ximian.com)
//	Dennis Hayes (dennish@raytek.com)
//   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
//
// (C) Ximian, Inc., 2002
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
		}
		
		// --- Properties ---
		protected override CreateParams CreateParams {
			get {
				// This is a child control, so it must have a parent for creation
				if( Parent != null) {
					CreateParams createParams = new CreateParams ();
					// CHECKME: here we must not overwrite window
					if( window == null) {
						window = new ControlNativeWindow (this);
					}

					createParams.Caption = Text;
					createParams.ClassName = "BUTTON";
					createParams.X = Left;
					createParams.Y = Top;
					createParams.Width = Width;
					createParams.Height = Height;
					createParams.ClassStyle = 0;
					createParams.ExStyle = 0;
					createParams.Param = 0;
					createParams.Parent = Parent.Handle;
					createParams.Style = (int) (
						WindowStyles.WS_CHILD | 
						WindowStyles.WS_VISIBLE |
						WindowStyles.WS_CLIPSIBLINGS );
					if(FlatStyle != FlatStyle.System) {
						createParams.Style |= (int) ButtonStyles.BS_OWNERDRAW;
					}
					createParams.Style |= (int)sysButtonStyles_;
					// CHECKME : this call is commented because (IMHO) Control.CreateHandle suppose to do this
					// and this function is CreateParams, not CreateHandle
					// window.CreateHandle (createParams);
					return createParams;
				}
				return null;
			}
		}
		
		// --- IButtonControl property ---
		public virtual DialogResult DialogResult {
			get { return dialogResult; }
			set { dialogResult = value; }
		}

		// --- IButtonControl method ---
		[MonoTODO]
		public virtual void NotifyDefault(bool value) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public void PerformClick() 
		{
			//FIXME:
		}
		
		// --- Button methods for events ---
		protected override void OnClick(EventArgs e) 
		{
			base.OnClick (e);
		}
		
		protected override void OnMouseUp(MouseEventArgs e) 
		{
			base.OnMouseUp (e);
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

		protected override void OnMouseEnter (EventArgs e) {
			base.OnMouseEnter(e);
			if( FlatStyle == FlatStyle.Flat || FlatStyle == FlatStyle.Popup) {
				Invalidate();
			}
		}
    
		protected override void OnMouseLeave (EventArgs e) {
			base.OnMouseLeave(e);
			if( FlatStyle == FlatStyle.Flat || FlatStyle == FlatStyle.Popup) {
				Invalidate();
			}
		}

		internal void OnDrawItem(DrawItemEventArgs e) {
			Graphics paintOn = e.Graphics;
			Rectangle rc = e.Bounds;
			Rectangle rcImageClip = e.Bounds;
			rcImageClip.Inflate(-2,-2);

			if( FlatStyle == FlatStyle.Flat) {
				if( (e.State & DrawItemState.Selected) != 0) {
					SolidBrush sb = new SolidBrush(ControlPaint.Light(SystemColors.Control));
					paintOn.FillRectangle(sb, rc);
					sb.Dispose();
				}
				else {
					if( mouseIsInside_) {
						SolidBrush sb = new SolidBrush(ControlPaint.Dark(SystemColors.Control));
						paintOn.FillRectangle(sb, rc);
						sb.Dispose();
					}
					else {
						paintOn.FillRectangle(SystemBrushes.Control, rc);
					}
				}

				ControlPaint.DrawBorder(paintOn, rc, SystemColors.ControlText, ButtonBorderStyle.Solid);
				rc.Inflate(-1,-1);

				if( (e.State & DrawItemState.Focus) != 0) {
					ControlPaint.DrawBorder(paintOn, rc, SystemColors.ControlText, ButtonBorderStyle.Solid);
				}
				else {
					rcImageClip.Inflate(1,1);
				}
				rc.Inflate(-3,-3);
			}
			else if( FlatStyle == FlatStyle.Popup) {
				if( (e.State & DrawItemState.Selected) != 0) {
					ControlPaint.DrawBorder(paintOn, rc, SystemColors.ControlText, ButtonBorderStyle.Solid);
					rc.Inflate(-1,-1);
					ControlPaint.DrawBorder(paintOn, rc, SystemColors.ControlText, ButtonBorderStyle.Solid);
					rc.Inflate(-1,-1);
				}
				else {
					if( (e.State & DrawItemState.Focus) != 0) {
						ControlPaint.DrawBorder(paintOn, rc, SystemColors.ControlText, ButtonBorderStyle.Solid);
						rc.Inflate(-1,-1);
					}

					if( mouseIsInside_) {
						Color colorLight = ControlPaint.Light(SystemColors.Control);
						ControlPaint.DrawBorder(paintOn, rc, colorLight, 1, ButtonBorderStyle.Solid,
							colorLight, 1, ButtonBorderStyle.Solid, SystemColors.ControlText, 1, ButtonBorderStyle.Solid,
							SystemColors.ControlText, 1, ButtonBorderStyle.Solid);
					}
					else {
						ControlPaint.DrawBorder(paintOn, rc, SystemColors.ControlText, ButtonBorderStyle.Solid);
					}
					rc.Inflate(-2,-2);
				}
				paintOn.FillRectangle(SystemBrushes.Control, rc);

				rc.Inflate(-1,-1);
			}
			else {
				ButtonState	btnState = ButtonState.Normal;

				if( (e.State & DrawItemState.Selected) != 0) {
					btnState = ButtonState.Pushed;
				}

				ControlPaint.DrawButton(paintOn, rc, btnState);

				rc.Inflate(-2,-2);
			}

			// Do not place Text and Images on the borders 
			paintOn.Clip = new Region(rcImageClip);
			if( BackgroundImage != null) {
				paintOn.DrawImage(BackgroundImage, e.Bounds.X, e.Bounds.Y, BackgroundImage.Width, BackgroundImage.Height);
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

			if( (e.State & DrawItemState.Selected) != 0) {
				// FlatStyle.Flat uses color and not text offset to show state
				// FIXME: use SysMetrics to determine offset values ?
				if( FlatStyle != FlatStyle.Flat) {
					rc.Offset(1,1);
				}
			}

			StringFormat format = new StringFormat();
			format.Alignment = horizontalAlign_;
			format.LineAlignment = verticalAlign_;
			paintOn.DrawString(Text, Font, SystemBrushes.ControlText, rc, format);

			if( (e.State & DrawItemState.Focus) != 0) {
				// FIXME: Draw focus rectangle in different colors
				ControlPaint.DrawFocusRectangle( paintOn, focusRC);
			}
		}

		protected override void WndProc (ref Message m) {
			switch (m.Msg) {
				case Msg.WM_DRAWITEM: {
					DRAWITEMSTRUCT dis = new DRAWITEMSTRUCT();
					dis = (DRAWITEMSTRUCT)Marshal.PtrToStructure(m.LParam, dis.GetType());
					Rectangle	rect = new Rectangle(dis.rcItem.left, dis.rcItem.top, dis.rcItem.right - dis.rcItem.left, dis.rcItem.bottom - dis.rcItem.top);
					DrawItemEventArgs args = new DrawItemEventArgs(Graphics.FromHdc(dis.hDC), Font,
						rect, dis.itemID, (DrawItemState)dis.itemState);
					OnDrawItem( args);
					//Marshal.StructureToPtr(dis, m.LParam, false);
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
