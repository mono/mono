//
// FileDialog.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using GLib;
using Gtk;
using GtkSharp;

namespace Mono.NUnit.GUI
{
	public class FileDialog : FileSelection
	{
		bool ok;
		string filename;
		
		public FileDialog () : this ("Select an assembly to load") {}

		public FileDialog (string title)
			: base (title)
		{
			ShowFileops = false;
			SelectMultiple = false;
			filename = null;
			Complete ("*.dll");
			OkButton.Clicked += new EventHandler (OkClicked);
			Response += new ResponseHandler (OnResponse);
		}

		void OkClicked (object sender, EventArgs args)
		{
			ok = true;
			filename = base.Filename;
		}

		void OnResponse (object sender, ResponseArgs args)
		{
			Hide ();
		}

		public bool Cancelled {
			get { return !ok; }
		}

		public bool Ok {
			get { return ok; }
		}

		public new string Filename {
			get { return filename; }
		}
	}
}

