// Bug #57014.
using System;

public class X {
	public const string Address = null;
	
	public static bool Resolve (string addr)
	{
		return true;
	}

	static string Test ()
	{
		return Address;
	}

	public static void Main ()
	{
		Resolve (Address);
	}
}
