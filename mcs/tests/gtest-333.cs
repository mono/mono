using System;

public static class Program
{
	public static void Main ()
	{
		Exception ex1 = null ?? new Exception ();
		Exception ex2 = new Exception() ?? null;
	}
}
