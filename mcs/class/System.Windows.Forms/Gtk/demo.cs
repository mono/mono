using System;
using System.Drawing;
using System.Windows.Forms;

class X {
	static void Clicked (object o, EventArgs args)
	{
		Console.WriteLine ("the button was clicked");
	}
	
	static void Demo_Window ()
	{
		Form form = new Form ();

		form.Text = "hello";

		Label l = new Label ();
		l.Location = new Point (20, 20);
		l.Text = "Hello world";
		form.Controls.Add (l);

		Button b = new Button ();
		b.Text = "a button";
		b.Location = new Point (20, 60);
		b.Click += new EventHandler (Clicked);
		form.Controls.Add (b);

		form.Visible = true;

		Application.Run ();
	}

	static void Demo_AppRun ()
	{
		Form form = new Form ();

		form.Text = "hello";
		
		Application.Run (form);
	}
	
	static void Main (string [] args)
	{
		string demo = "window";

		if (args.Length > 0)
			demo = args [0];
		
		switch (demo){
		case "window":
			Demo_Window ();
			break;
		case "app_run":
			Demo_AppRun ();
			break;
		}
	}
}
