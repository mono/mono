//
// System.Windows.Forms.GroupBox.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc
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
			SubClassWndProc_ = true;
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


		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				if( Parent != null) {
					CreateParams createParams = new CreateParams ();
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
						(int)WindowStyles.WS_CHILD | 
						(int)WindowStyles.WS_VISIBLE | 
						(int)ButtonStyles.BS_GROUPBOX |
						(int)SS_Static_Control_Types.SS_LEFT );
					return createParams;
				}
				return null;
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

		[MonoTODO]
		protected override void OnPaint(PaintEventArgs e) {
			//FIXME:
			base.OnPaint(e);
		}

		[MonoTODO]
		protected override bool ProcessMnemonic(char charCode) {
			//FIXME:
			return base.ProcessMnemonic(charCode);
		}

		[MonoTODO]
		protected override void WndProc(ref Message m) {
			//FIXME:
			base.WndProc(ref m);
		}

	}
}
