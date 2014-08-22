using System;

class Program
{
	public static void Main ()
	{
		goto L1;
		int z;
	L1: 
		z = 3;
		Console.WriteLine (z);
	}
}