//
// System.Windows.Forms.Application
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2002 Ximian, Inc
//

using System;
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	public sealed class Application {
		public static void Run ()
		{
			Gtk.Application.Run ();
		}

		static void terminate_event_loop (object o, EventArgs args)
		{
			Gtk.Application.Quit ();
		}
		
		public static void Run (Form form)
		{
			form.Visible = true;
			form.Closed += new EventHandler (terminate_event_loop);
				
			Gtk.Application.Run ();
		}
	}
}
