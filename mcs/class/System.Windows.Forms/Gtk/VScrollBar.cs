//
// System.Windows.Forms.VScrollBar.cs
//
// Author:
//   Philip Van Hoof (me@freax.org)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System.Drawing;

namespace System.Windows.Forms {

	public class VScrollBar : ScrollBar {

		private RightToLeft rightToLeft;
		// --- Properties ---


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

		internal override Gtk.Widget CreateWidget () {
			return new Gtk.VScrollbar ( adj );
		}

		// --- Constructor ---
		public VScrollBar() : base (){
			rightToLeft = RightToLeft.Inherit;
		}
		protected override Size DefaultSize{
			get{ return new System.Drawing.Size (17,80); }
		}
		public override string ToString(){
			String ret = String.Format (
				"System.Windows.Forms.VScrollBar, " +
				"Minimum: {0}, Maximum: {1}, Value: {2}",
				this.Minimum,
				this.Maximum,
				this.Value);
			return ret;
		}
	}
}
