using System;

public delegate void EventHandler (int i, int j);

public class Button {

	public event EventHandler Click;

        public void OnClick (int i, int j)
  	{
  		if (Click != null)
  			Click (i, j);
 	}

	public void Reset ()
	{
		Click = null;
	}
}

public class Blah {

	Button Button1 = new Button ();

	public void Connect ()
	{
		Button1.Click += new EventHandler (Button1_Click);
	}

	public void Button1_Click (int i, int j)
	{
		Console.WriteLine ("Button1 was clicked !");
		Console.WriteLine ("Answer : " + (i+j));
	}

	public void Disconnect ()
	{
		Console.WriteLine ("Disconnecting ...");
		// Button1.Click -= new EventHandler (Button1_Click);
	}

	public static int Main ()
	{
		Blah b = new Blah ();

		b.Connect ();

		b.Button1.OnClick (2, 3);

		b.OnClick ();

		Console.WriteLine ("Events test passes");
		return 0;
	}
	
}
