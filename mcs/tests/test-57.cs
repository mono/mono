using System;

public delegate void EventHandler (object sender, EventArgs e);

public class Button {

	public event EventHandler Click;

	protected void OnClick (EventArgs e)
	{
		if (Click != null)
			Click (this, e);
	}

	public void Reset ()
	{
		Click = null;
	}
}

public class Blah {

	Button Button1 = new Button ();

	public Blah ()
	{
		Button1.Click += new EventHandler (Button1_Click);
	}

	public void Button1_Click (object sender, EventArgs e)
	{
		Console.WriteLine ("Button1 was clicked !");
	}

	public void Disconnect ()
	{
		Console.WriteLine ("Disconnecting ...");
		Button1.Click -= new EventHandler (Button1_Click);
	}

	public static int Main ()
	{
		Blah b = new Blah ();

		return 0;
	}
	
}
