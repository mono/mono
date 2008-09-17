using System;

[assembly: LibA ("b")]

public class LibBAttribute : Attribute {
	public LibBAttribute (string s)
	{
	}
}

public class LibB : LibA {
	public static new void A ()
	{
		LibA.A ();
	}

	public static void B ()
	{
		Console.WriteLine ("B");
	}
}
