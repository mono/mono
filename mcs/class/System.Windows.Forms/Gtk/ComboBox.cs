//		
//			System.Windows.Forms.ComboBox
//
//			Author: 
//						Joel Basson		(jstrike@mweb.co.za)
//
//

using System.Drawing;
using System.Drawing.Printing;
using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows ComboBox control.
	///
	/// </summary>

	public class ComboBox: Control{

	public ComboBox () : base ()
	{
	}

	internal override Gtk.Widget CreateWidget () {
		Gtk.Combo com1 = new Gtk.Combo();
		return com1;	
	}
	
	}
}