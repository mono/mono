//
// System.Windows.Forms.Panel.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;
using System.Runtime.Remoting;
using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class Panel : ScrollableControl {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public Panel() {
			
		}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public BorderStyle BorderStyle {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		[MonoTODO]
		public override ISite Site {
			get {
				//FIXME:
				return base.Site;
			}
			set {
				//FIXME:
				base.Site = value;
			}
		}

		[MonoTODO]
		public override string Text {
			get {
				return base.Text;
			}
			set {
				base.Text = value;
			}
		}

		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				if( Parent != null) {
					CreateParams createParams = base.CreateParams;
					if( window == null) {
						window = new ControlNativeWindow (this);
					}

					createParams.Caption = Text;
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
						WindowStyles.WS_VISIBLE);
					return createParams;
				}
				return null;
			}		
		}
		[MonoTODO]
		protected override Size DefaultSize {
			get {
				return new Size(219,109);
			}
		}
		[MonoTODO]
		protected override void OnResize(EventArgs e) {
			//FIXME:
			base.OnResize(e);
		}
	}
}
