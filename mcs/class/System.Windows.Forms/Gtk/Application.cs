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
using Gtk;
using GtkSharp;
using System.ComponentModel;

namespace System.Windows.Forms {

	public sealed class Application {
		public static void Run ()
		{
			Gtk.Application.Run ();
		}
	}
}
