//
// System.Windows.Forms.HScrollBar.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc
//
// CE Complete
using System.Drawing;
using System.ComponentModel;
namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

	public class HScrollBar : ScrollBar {

		//
		//  --- Constructor
		//

		[MonoTODO]
		public HScrollBar() {
			//FIXME: implment
		}

		//
		//  --- Protected Properties
		//

		[MonoTODO]
		protected  override  CreateParams CreateParams {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected override  Size DefaultSize {
			get {
				return new Size(80,16);
			}
		}
	}
}
