using System;

public delegate void EventHandler (int i, int j);

public class Button {

	private EventHandler click;

	public event EventHandler Click {
		add    { click += value; }
		remove { click -= value; }
	}

        public void OnClick (int i, int j)
  	{
  		if (click == null) {
			Console.WriteLine ("Nothing to click!");
			return;
		}

		click (i, j);
 	}

	public void Reset ()
	{
		click = null;
	}
}

public class Blah {

	Button Button1 = new Button ();

	public void Connect ()
	{
		Button1.Click += new EventHandler (Button1_Click);
		Button1.Click += new EventHandler (Foo_Click);
		Button1.Click += null;
	}

	public void Button1_Click (int i, int j)
	{
		Console.WriteLine ("Button1 was clicked !");
		Console.WriteLine ("Answer : " + (i+j));
	}

	public void Foo_Click (int i, int j)
	{
		Console.WriteLine ("Foo was clicked !");
		Console.WriteLine ("Answer : " + (i+j));
	}

	public void Disconnect ()
	{
		Console.WriteLine ("Disconnecting Button1's handler ...");
		Button1.Click -= new EventHandler (Button1_Click);
	}

	public static int Main ()
	{
		Blah b = new Blah ();

		b.Connect ();

		b.Button1.OnClick (2, 3);

		b.Disconnect ();

		Console.WriteLine ("Now calling OnClick again");
		b.Button1.OnClick (3, 7);

		Console.WriteLine ("Events test passes");
		return 0;
	}
	
}
