//
// FileSelectionDialog.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//         Daniel Morgan <danmorg@sc.rr.com>
//
// Copyright (C) 2002, Duncan Mak, Ximian Inc.
// Copyright (C) 2002, Daniel Morgan
//

using System;

using Gtk;
using GtkSharp;

namespace Mono.GtkSharp.Goodies 
{
	public class FileSelectionEventArgs 
	{
		private string filename;

		public FileSelectionEventArgs (string filename) 
		{
			this.filename = filename;
		}

		public string Filename {
			get {
				return filename;
			}
		}
	}

	public delegate void FileSelectionEventHandler (object sender, FileSelectionEventArgs e);

	public class FileSelectionDialog 
	{
		FileSelection window = null;
		ToggleButton toggle_button = null;
		CheckButton check_button = null;

		public event FileSelectionEventHandler fh;
		
		public FileSelectionDialog (string title, FileSelectionEventHandler fileSelectedHandler) 
		{
			window = new FileSelection (title);
			window.OkButton.Clicked += new EventHandler (OnFileSelectionOk);
			window.CancelButton.Clicked += new EventHandler (OnFileSelectionCancel);
			if(fileSelectedHandler == null)
				throw new Exception ("FileSelectionDialog fileSelectedHandler is null");
			fh = fileSelectedHandler;

			window.ShowAll ();
		}

		void OnFileSelectionOk(object o, EventArgs args) 
		{
			Gtk.Button fsbutton = (Gtk.Button) o;
			string filename = window.Filename;
			FileSelectionEventArgs fa = new FileSelectionEventArgs (filename);
			if (fh != null) {
				fh (this, fa); 
			}
			window.Destroy ();
		}

		void OnFileSelectionCancel (object o, EventArgs args) 
		{
			window.Destroy ();
		}
	}
}
