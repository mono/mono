// Compiler options: -unsafe

using System;

unsafe class Test
{
	public static unsafe byte* GetFoo ()
	{
		byte *one = (byte*)1;
		return 1 + one;
	}

	public static unsafe byte* GetFoo2 ()
	{
		byte *one = (byte*)3;
		return one + 3;
	}

	public static int Main()
	{
		int b = (int)GetFoo ();
		Console.WriteLine (b);
		if (b != 2)
			return 1;

		b = (int)GetFoo2 ();
		Console.WriteLine (b);
		if (b != 6)
			return 2;

		return 0;
	}
}
