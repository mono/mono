using System;

public class Test
{
	public static void Main ()
	{
		string[] aPath = {"a","b"};
		char c = '.';
		if (c.ToString () != ".")
			throw new Exception ("c.ToString () is not \".\"");
		string erg = "";
		erg += String.Join (c.ToString (), aPath);
		if (erg != "a.b")
			throw new Exception ("erg is " + erg);
	}
}
