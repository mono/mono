//		
//			System.Windows.Forms.GroupBox
//
//			Author: 
//						Joel Bason		(jstrike@mweb.co.za)
//
//

namespace System.Windows.Forms {

	/// <summary>
	/// 	Represents a Windows GroupBox.
	///
	/// </summary>

	public class GroupBox : Form {

	public GroupBox () : base ()
	{
	}

	internal override Gtk.Widget CreateWidget () {
		Gtk.Frame gbox1 = new Gtk.Frame(null);
		return gbox1;	
	}
	
	public override string Text {
		get {
					return ((Gtk.Frame)Widget).Label;
		}
		set {
					((Gtk.Frame)Widget).Label = value;
		}	
	
	}
	
	}
}
