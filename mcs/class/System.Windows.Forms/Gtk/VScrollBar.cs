//
// System.Windows.Forms.VScrollBar.cs
//
// Author:
//   Philip Van Hoof (me@freax.org)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms {

	public class VScrollBar : ScrollBar {
		private Gtk.VScrollbar bar;
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
			return bar;
		}

		// --- Constructor ---
		public VScrollBar() : base ()
		{
			this.bar = new Gtk.VScrollbar ( adj );
			rightToLeft = RightToLeft.Inherit;
		}
	}
}
