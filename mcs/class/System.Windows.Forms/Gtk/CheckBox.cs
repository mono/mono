//		
//			System.Windows.Forms.CheckBox
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
	/// Represents a Windows CheckBox control.
	///
	/// </summary>

	public class CheckBox: ButtonBase{

	public CheckBox () : base ()
	{
	}

	internal override Gtk.Widget CreateWidget () {
		Gtk.CheckButton cbox = new Gtk.CheckButton();
		cbox.Add (label.Widget);
		return cbox;	
	}
	
		public bool Checked {
			get {
		   	return ((Gtk.CheckButton)Widget).Active; 
			}
			set {
				((Gtk.CheckButton)Widget).Active = value;
			}
		}
	}
}