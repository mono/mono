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
						WindowStyles.WS_CLIPSIBLINGS);
					if(FlatStyle != FlatStyle.System) {
						createParams.Style |= (int) ButtonStyles.BS_OWNERDRAW;
					}
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

		internal void OnDrawItem(DrawItemEventArgs e) {

			Rectangle rc = e.Bounds;
			ButtonState	btnState = ButtonState.Normal;

			if( (e.State & DrawItemState.Selected) != 0) {
				btnState = ButtonState.Pushed;
			}

			if( FlatStyle == FlatStyle.Flat) {
				btnState |= ButtonState.Flat;
			}

			// FIXME: how to draw FlatStyle.Popup ?
			// FIXME: how to draw Pushed FlatStyle.Flat ?
			ControlPaint.DrawButton(e.Graphics, rc, btnState);

			StringFormat format = new StringFormat();
			format.Alignment = horizontalAlign;
			format.LineAlignment = verticalAlign;

			if( (e.State & DrawItemState.Selected) != 0) {
				// FIXME: FlatStyle.Flat uses color and not dext offset to show state
				// FIXME: use SysMetrics to determine offset values ?
				rc.Offset(2,2);
			}
			e.Graphics.DrawString(Text, Font, SystemBrushes.ControlText, rc, format);
		}

		protected override void WndProc (ref Message m) {
			switch (m.Msg) {
				case Msg.WM_DRAWITEM: {
					DRAWITEMSTRUCT dis = new DRAWITEMSTRUCT();
					Win32.CopyMemory(ref dis, m.LParam, 48);
					Rectangle	rect = new Rectangle(dis.rcItem.left, dis.rcItem.top, dis.rcItem.right - dis.rcItem.left, dis.rcItem.bottom - dis.rcItem.top);
					DrawItemEventArgs args = new DrawItemEventArgs(Graphics.FromHdc(dis.hDC), Font,
						rect, dis.itemID, (DrawItemState)dis.itemState);
					OnDrawItem( args);
					Win32.CopyMemory(m.LParam, ref dis, 48);
				}
					break;
				case Msg.WM_MOUSEMOVE: {
					MouseIsOver_ = true;
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
