//		
//			System.Windows.Forms.PictureBox
//
//			Author: 
//						Joel Basson		(jstrike@mweb.co.za)
//
//

using System.Drawing;

namespace System.Windows.Forms {
	
	/// <summary>
	/// Represents a Windows PictureBox control.
	///
	/// </summary>

	public class PictureBox: Control{
		bool stretch;
		int height, width, dwidth, dheight;
		Gdk.Pixbuf pic1, pic2;
		string filevalue;
	public PictureBox () : base ()
	{
	}

	internal override Gtk.Widget CreateWidget () {
		Gtk.Image ibox = new Gtk.Image();
		return ibox;	
	}
	
	public string File {	
		set {
				filevalue = value;
				Gdk.Pixbuf pic1 = new Gdk.Pixbuf(filevalue);
				((Gtk.Image)Widget).Pixbuf = pic1;					
		}
	}
	
	public bool Stretch {
		
		get {
					return stretch;
		}
		set {
				if (value){ 
						Gdk.Pixbuf pic1 = new Gdk.Pixbuf(filevalue);
						height = pic1.Height;
						width = pic1.Width;	
						dheight = ((Gtk.Image)Widget).HeightRequest;
						dwidth = ((Gtk.Image)Widget).WidthRequest;
						((Gtk.Image)Widget).Pixbuf = pic1.ScaleSimple(dwidth, dheight, Gdk.InterpType.Bilinear);
						stretch = value;
				}
				else{
						Gdk.Pixbuf pic1 = new Gdk.Pixbuf(filevalue);
						((Gtk.Image)Widget).Pixbuf = pic1;
						stretch = value;
				} 
		}
	}

	}
}
