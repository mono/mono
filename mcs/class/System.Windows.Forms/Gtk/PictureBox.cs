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
	
	public PictureBoxSizeMode SizeMode {
		set{
			if (value == PictureBoxSizeMode.StretchImage){
				Stretch();
			}
			if 	(value == PictureBoxSizeMode.Normal){
				Normal();
			}	
			if 	(value == PictureBoxSizeMode.CenterImage){
				CenterImage();
			}	
			if 	(value == PictureBoxSizeMode.AutoSize){
				AutoSize();
			}	
		}			
	}
	
	private void Stretch () {
	
						Gdk.Pixbuf pic1 = new Gdk.Pixbuf(filevalue);
						height = pic1.Height;
						width = pic1.Width;	
						dheight = ((Gtk.Image)Widget).HeightRequest;
						dwidth = ((Gtk.Image)Widget).WidthRequest;
						((Gtk.Image)Widget).Pixbuf = pic1.ScaleSimple(dwidth, dheight, Gdk.InterpType.Bilinear);
						
	}
	
	private void Normal () {
	
						Gdk.Pixbuf pic1 = new Gdk.Pixbuf(filevalue);
						((Gtk.Image)Widget).Pixbuf = pic1;
						((Gtk.Image)Widget).SetAlignment(0, 1);
	} 
	
	private void CenterImage () {
	
						Gdk.Pixbuf pic1 = new Gdk.Pixbuf(filevalue);
						((Gtk.Image)Widget).Pixbuf = pic1;
						((Gtk.Image)Widget).SetAlignment((float)0.5, (float)0.5);
	}

	private void AutoSize () {
	
						Gdk.Pixbuf pic1 = new Gdk.Pixbuf(filevalue);
						height = pic1.Height;
						width = pic1.Width;	
						((Gtk.Image)Widget).HeightRequest = height;
						((Gtk.Image)Widget).WidthRequest = width;
						((Gtk.Image)Widget).Pixbuf = pic1;
	}
	
	}
}
