using System.Windows.Forms;

class X {
	static void Demo_Window ()
	{
		Form form = new Form ();

		form.Text = "hello";
		
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
