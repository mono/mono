using System;

public class Blah {

	public void Connect ()
	{
	}

	public void Button1_Click (int i, int j)
	{
		Console.WriteLine ("Button1 was clicked !");
		Console.WriteLine ("Answer : " + (i+j));
	}

	public static int Main ()
	{
		Blah b = new Blah ();

		b.Connect ();

		b.OnClick ();

		Console.WriteLine ("Test passes");
		return 0;
	}
	
}
