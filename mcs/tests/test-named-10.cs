using System;

internal class Program
{
	public static void Main ()
	{
		Method (1, 2, paramNamed: 3);
	}
	
	static void Method (int p1, int paramNamed, int p2)
	{
		throw new ApplicationException ();
	}
	
	static void Method (int p1, int p2, object paramNamed)
	{
	}
}
