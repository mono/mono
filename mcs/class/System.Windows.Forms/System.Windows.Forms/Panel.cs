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

		BorderStyle borderStyle = BorderStyle.None;
		//
		//  --- Constructor
		//
		[MonoTODO]
		public Panel() {
			controlStyles_ |= ControlStyles.UserPaint;
		}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public BorderStyle BorderStyle {
			get {
				return borderStyle;
			}
			set {
				borderStyle = value;
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
				CreateParams createParams = base.CreateParams;

				createParams.Style = (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE |
					WindowStyles.WS_CLIPCHILDREN |
					WindowStyles.WS_CLIPSIBLINGS);

				switch (BorderStyle) {
				case BorderStyle.Fixed3D:
					createParams.ExStyle |= (int)WindowExStyles.WS_EX_CLIENTEDGE;
				break;
				case BorderStyle.FixedSingle:
					createParams.Style   |= (int)WindowStyles.WS_BORDER;
				break;
				}

				return createParams;
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
