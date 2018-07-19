// CS8129:
// Line:

using System;

class C
{
	static void Main ()
	{
		long x;
		string y;
		(x, y) = new C ();
	}

	public static void Deconstruct (out int a, out string b)
	{
		a = 1;
		b = "hello";
	}
}