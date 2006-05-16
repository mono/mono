using System;

public class Foo {
	public static void Main (string[] args)
	{
		try {
			f ();
		}
		catch {}
	}

	static void f ()
	{
		throw new Exception ();
		string hi = "";
		try { }
		finally {
			Console.WriteLine ("hi = {0}", hi);
		}
	}
}
