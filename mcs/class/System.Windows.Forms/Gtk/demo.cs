using System.Windows.Forms;

class X {
	static void Main ()
	{
		Gtk.Application.Init ();
		
		Form form = new Form ();

		form.Text = "hello";
		
		form.Show ();

		Application.Run ();
	}
}

