//
// System.Windows.Forms.StructFormat.cs
//
// Author:
//   John Rebbeck <john@rebent.com>
//   Dennis Hayes (dennish@Raytek.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
using System;
using System.Drawing;

namespace System.Windows.Forms {

	public class VScrollBar : ScrollBar {

		private RightToLeft rightToLeft;
		// --- Properties ---
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams cp = base.CreateParams;
				//Modify cp before returning it.
				return cp;
			}
		}

		[MonoTODO]
		protected override Size DefaultSize {
			get {
				//Set to Microsoft Default
				return new Size(16,80);
			}
		}

		[MonoTODO]
		public override RightToLeft RightToLeft {

			get {
				return rightToLeft;
			}
			set {
				rightToLeft = value;
				//FixMe: invalidate to force redraw.
				//Invalidate();
			}
		}

		// --- Constructor ---
		[MonoTODO]
		public VScrollBar()
		{
			rightToLeft = RightToLeft.Inherit;
		}
	}
}
