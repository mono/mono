//		
//			System.Windows.Forms.ColorDialog
//
//			Author: 
//						Joel Basson		(jstrike@mweb.co.za)
//
//

using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows Color Dialog.
	///
	/// </summary>

	public class ColorDialog: Control{
		
		Gtk.ColorSelectionDialog color1 = new Gtk.ColorSelectionDialog("");
		
	public ColorDialog () : base () {
	}

	
	public void ShowDialog () {
		//Gtk.ColorSelectionDialog color1 = new Gtk.ColorSelectionDialog("");
		color1.OkButton.Clicked += new EventHandler (color_selection_ok);
		color1.CancelButton.Clicked += new EventHandler (color_selection_cancel);
		color1.Show();
	}	

		
	internal void color_selection_cancel (object o, EventArgs args){
			color1.Hide();				
	} 
		
	internal void color_selection_ok (object o, EventArgs args){

			color1.Hide();
	}
	
	}
	
}
