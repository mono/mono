//		
//			System.Windows.Forms.FileDialog
//
//			Author: 
//						Joel Basson		(jstrike@mweb.co.za)
//
//

using System.Drawing;
using System.Drawing.Printing;
using System.ComponentModel;
using Gtk;
using GtkSharp;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows File Dialog.
	///
	/// </summary>

	public class FileDialog: Control{
		
		String name1; 
		Gtk.FileSelection file1 = new Gtk.FileSelection("");

	public FileDialog () : base () {
	}

	
	public void ShowDialog () {
		//Gtk.FileSelection file1 = new Gtk.FileSelection("");
		file1.OkButton.Clicked += new EventHandler (file_selection_ok);
		file1.CancelButton.Clicked += new EventHandler (file_selection_cancel);
		file1.Show();
	}	

	public string OpenFile {
		get {
					return name1;
		}
	}
	
	internal void file_selection_cancel (object o, EventArgs args){
			file1.Hide();
	} 
		
	internal void file_selection_ok (object o, EventArgs args){
			name1 = file1.Filename;
			file1.Hide();
	}
	
	}
	
}
