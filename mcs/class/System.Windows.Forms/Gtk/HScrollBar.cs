//
// System.Windows.Forms.HScrollBar.cs
//
// Author:
//   Philip Van Hoof (me@freax.org)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

namespace System.Windows.Forms {

	public class HScrollBar : ScrollBar {
		private Gtk.HScrollbar bar;
		// --- Properties ---

		internal override Gtk.Widget CreateWidget () {
			return bar;
		}

		// --- Constructor ---
		public HScrollBar() : base ()
		{
			this.bar = new Gtk.HScrollbar ( adj );
		}
	}
}
