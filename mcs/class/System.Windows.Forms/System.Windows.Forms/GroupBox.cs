//
// System.Windows.Forms.GroupBox.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//      Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) 2002/3 Ximian, Inc
//
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>
	// Represents a Windows group box.
	// </summary>

	public class GroupBox : Control {

		FlatStyle flatStyle;

		[MonoTODO]
		public GroupBox() {
			SetStyle ( ControlStyles.UserPaint, true);
			SetStyle ( ControlStyles.Selectable, false );
			TabStop = false;
			flatStyle = FlatStyle.Standard;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override bool AllowDrop {
			get { return base.AllowDrop;  }
			set { base.AllowDrop = value; }
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
			get { return flatStyle;	}
			set {
				if ( !Enum.IsDefined ( typeof(FlatStyle), value ) )
					throw new InvalidEnumArgumentException( "FlatStyle",
						(int)value,
						typeof(FlatStyle));
				flatStyle = value;
			}
		}

		public override string ToString() {
			return GetType ( ).FullName.ToString ( ) + ", Text: " + Text;
		}

		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				RegisterDefaultWindowClass ( );

				CreateParams createParams = base.CreateParams;
					createParams.ClassName = Win32.DEFAULT_WINDOW_CLASS;;

				createParams.Style = (int) (
					(int)WindowStyles.WS_CHILDWINDOW |
					(int)WindowStyles.WS_CLIPCHILDREN |
					(int)WindowStyles.WS_CLIPSIBLINGS |
					(int)WindowStyles.WS_OVERLAPPED |
					(int)WindowStyles.WS_VISIBLE );

				return createParams;
			}
		}

		protected override Size DefaultSize {
			get { return new Size(200,100);	}
		}

		[MonoTODO]
		protected override void OnFontChanged(EventArgs e) {
			base.OnFontChanged(e);
			Invalidate ( );
		}

		protected virtual void OnPaintBackground (PaintEventArgs e)
		{
		}

		public override string Text {
			get { return base.Text; }
			set { 
				base.Text = value;
				Invalidate ( );
			}
		}

		[MonoTODO]
		protected override void OnPaint(PaintEventArgs e) {
			try {
				//FIXME: use TextMetrics to calculate coordinates in the method
				Rectangle bounds = DisplayRectangle;

				Bitmap bmp = new Bitmap(bounds.Width, bounds.Height, e.Graphics);
				Graphics paintOn = Graphics.FromImage(bmp);

				Brush br = new SolidBrush(BackColor);
				paintOn.FillRectangle(br, bounds);
				
				bounds.Y += 5;
				bounds.Height -= 5;

/*
				bounds.Inflate(-4,-4);
				bounds.Y += 2;
*/
				Color dark   = ControlPaint.DarkDark ( BackColor );
				Color light  = ControlPaint.LightLight ( BackColor );

				ControlPaint.DrawBorder(paintOn, bounds, dark, 1, ButtonBorderStyle.Solid,
					dark, 1, ButtonBorderStyle.Solid, light, 1, ButtonBorderStyle.Solid,
					light, 1, ButtonBorderStyle.Solid);
				bounds.Inflate(-1,-1);
				ControlPaint.DrawBorder(paintOn, bounds, light, 1, ButtonBorderStyle.Solid,
					light, 1, ButtonBorderStyle.Solid, dark, 1, ButtonBorderStyle.Solid,
					dark, 1, ButtonBorderStyle.Solid);

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
