//
// System.Windows.Forms.HScrollBar.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
// (C) 2002 Ximian, Inc
//

using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>
	// Represents a standard horizontal scroll bar.
	// </summary>

	public class HScrollBar : ScrollBar {

		public HScrollBar() {
			RightToLeft = RightToLeft.No;
		}

		protected  override  CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;
				createParams.Style |= (int) ScrollBarFlags.SBS_HORZ;
				return createParams;
			}
		}

		protected override  Size DefaultSize {
			get { return new Size(80,13); }
		}
	}
}
