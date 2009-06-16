// Compiler options: -langversion:future

using System;

public class Blah
{
	public delegate int MyDelegate (int i, int j = 7);
	
	public int Foo (int i, int j)
	{
		return i+j;
	}

	public static int Main ()
	{
		Blah i = new Blah ();
		MyDelegate del = new MyDelegate (i.Foo);

		int number = del (2);

		Console.WriteLine (number);
		if (number != 9)
			return 1;

		return 0;
	}
}
