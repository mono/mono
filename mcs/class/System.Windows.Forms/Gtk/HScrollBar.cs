//
// System.Windows.Forms.HScrollBar.cs
//
// Author:
//   Philip Van Hoof (me@freax.org)
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System.Drawing;

namespace System.Windows.Forms {

	public class HScrollBar : ScrollBar {
		//private Gtk.HScrollbar bar;
		// --- Properties ---

		internal override Gtk.Widget CreateWidget () {
			return new Gtk.HScrollbar(this.adj);
		}

		// --- Constructor ---
		public HScrollBar() : base (){
		}
		
		protected override Size DefaultSize{
			get{ return new System.Drawing.Size (80,17); }
		}
		
		public override string ToString(){
			String ret = String.Format (
				"System.Windows.Forms.HScrollBar, " +
				"Minimum: {0}, Maximum: {1}, Value: {2}",
				this.Minimum,
				this.Maximum,
				this.Value);
			return ret;
		}
	}
}
