//
// System.Windows.Forms.StructFormat.cs
//
// Author:
//   John Rebbeck <john@rebent.com>
//   Dennis Hayes (dennish@Raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
using System;
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	public class VScrollBar : ScrollBar {

		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;
				createParams.Style |= (int) ScrollBarFlags.SBS_VERT;
				return createParams;
			}
		}

		protected override Size DefaultSize {
			get { return new Size(13,80); }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override RightToLeft RightToLeft {
			get { return base.RightToLeft;  }
			set { base.RightToLeft = value; }
		}

		public VScrollBar()
		{
			RightToLeft = RightToLeft.No;
		}
	}
}
