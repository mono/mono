//		
//			System.Windows.Forms.FontDialog
//
//			Author: 
//						Joel Basson		(jstrike@mweb.co.za)
//
//

using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows Font Dialog.
	///
	/// </summary>

	public class FontDialog: Control{
		
		//String name1; 
		Gtk.FontSelectionDialog font1 = new Gtk.FontSelectionDialog("");

	public FontDialog () : base () {
	}

	
	public void ShowDialog () {
		//Gtk.FontSelection file1 = new Gtk.FontSelection("");
		//font1.OkButton.Clicked += new EventHandler (font_selection_ok);
		//font1.CancelButton.Clicked += new EventHandler (font_selection_cancel);
		font1.Show();
	}	

		
	//internal void font_selection_cancel (object o, EventArgs args){
	//
	//		font1.Hide();
	//} 
		
	//internal void font_selection_ok (object o, EventArgs args){
	//
	//		font1.Hide();
	//}
	
	}
	
}
