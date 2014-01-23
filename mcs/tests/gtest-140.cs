using System;

class X
{
	public static void Main ()
	{
		int?[] bvals = new int?[] { null, 3, 4 };
		foreach (long? x in bvals) 
			Console.WriteLine (x);
	}
}
