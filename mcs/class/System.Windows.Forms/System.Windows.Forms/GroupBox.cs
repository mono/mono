//
// System.Windows.Forms.GroupBox.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
// (C) 2002/3 Ximian, Inc
//
using System.Drawing;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class GroupBox : Control {

		//
		//  --- Constructor
		//

		[MonoTODO]
		public GroupBox() {
			//SubClassWndProc_ = true;
		}

		//
		//  --- Public Properties
		//

		[MonoTODO]
		public override bool AllowDrop {
			get {
				//FIXME:
				return base.AllowDrop;
			}
			set {
				//FIXME:
				base.AllowDrop = value;
			}
		}

		[MonoTODO]
		public override Rectangle DisplayRectangle {
			get {
				//FIXME:
				return base.DisplayRectangle;
			}
		}

		[MonoTODO]
		public FlatStyle FlatStyle {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public override string ToString() {
			//FIXME:
			return base.ToString();
		}


		static private bool classRegistered = false;
		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				if (!classRegistered) {
					WNDCLASS wndClass = new WNDCLASS();
 
					wndClass.style = (int) (CS_.CS_DBLCLKS);
					wndClass.lpfnWndProc = NativeWindow.GetWindowProc();
					wndClass.cbClsExtra = 0;
					wndClass.cbWndExtra = 0;
					wndClass.hInstance = (IntPtr)0;
					wndClass.hIcon = (IntPtr)0;
					wndClass.hCursor = Win32.LoadCursor( (IntPtr)0, LC_.IDC_ARROW);
					wndClass.hbrBackground = (IntPtr)((int)GetSysColorIndex.COLOR_BTNFACE + 1);
					wndClass.lpszMenuName = "";
					wndClass.lpszClassName = "mono_static_control";
    
					if (Win32.RegisterClass(ref wndClass) != 0) 
						classRegistered = true; 
				}		

				CreateParams createParams = base.CreateParams;
	
				createParams.ClassName = "mono_static_control";

				createParams.Style = (int) (
					(int)WindowStyles.WS_CHILDWINDOW |
					(int)SS_Static_Control_Types.SS_LEFT |
					(int)WindowStyles.WS_CLIPCHILDREN |
					(int)WindowStyles.WS_CLIPSIBLINGS |
					(int)WindowStyles.WS_OVERLAPPED |
					(int)WindowStyles.WS_VISIBLE );

				return createParams;
			}
		}

		[MonoTODO]
		protected override Size DefaultSize {
			get {
				return new Size(200,100);//correct value
			}
		}

		//
		//  --- Protected Methods
		//

		[MonoTODO]
		protected override void OnFontChanged(EventArgs e) {
			//FIXME:
			base.OnFontChanged(e);
		}

		protected virtual void OnPaintBackground (PaintEventArgs e)
		{
		}

		[MonoTODO]
		protected override void OnPaint(PaintEventArgs e) {
			try {
				//FIXME: use TextMetrics to calculate coordinates in the method
				Rectangle bounds = new Rectangle(new Point(0,0), Size);

				Bitmap bmp = new Bitmap(bounds.Width, bounds.Height, e.Graphics);
				Graphics paintOn = Graphics.FromImage(bmp);

				Brush br = new SolidBrush(BackColor);
				paintOn.FillRectangle(br, bounds);
				bounds.Inflate(-4,-4);
				bounds.Y += 2;
				ControlPaint.DrawBorder(paintOn, bounds, SystemColors.ControlDark, 1, ButtonBorderStyle.Solid,
					SystemColors.ControlDark, 1, ButtonBorderStyle.Solid, SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid,
					SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid);
				bounds.Inflate(-1,-1);
				ControlPaint.DrawBorder(paintOn, bounds, SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid,
					SystemColors.ControlLightLight, 1, ButtonBorderStyle.Solid, SystemColors.ControlDark, 1, ButtonBorderStyle.Solid,
					SystemColors.ControlDark, 1, ButtonBorderStyle.Solid);
				SizeF sz = paintOn.MeasureString( Text, Font);
				sz.Width += 2.0F;
				paintOn.FillRectangle( br, new RectangleF(new PointF((float)bounds.Left + 3.0F, 0.0F), sz));
				paintOn.DrawString(Text, Font, SystemBrushes.ControlText, (float)bounds.Left + 5, 0);
				e.Graphics.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
				br.Dispose();
				bmp.Dispose();
			}
			catch(Exception ex) {
			}
			//base.OnPaint(e);
		}

		[MonoTODO]
		protected override bool ProcessMnemonic(char charCode) {
			//FIXME:
			return base.ProcessMnemonic(charCode);
		}

		[MonoTODO]
		protected override void WndProc(ref Message m) {
			switch (m.Msg) {
				case Msg.WM_ERASEBKGND:
					m.Result = (IntPtr)1;
					break;
				default:
					base.WndProc (ref m);
					break;
			}
		}

	}
}
