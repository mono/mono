// CS0151: A switch expression of type `S1?' cannot be converted to an integral type, bool, char, string, enum or nullable type
// Line: 25
// Compiler options: -langversion:5

using System;

struct S1
{
	public static implicit operator int? (S1? s)
	{
		throw new ApplicationException ();
	}

	public static implicit operator int (S1? s)
	{
		throw new ApplicationException ();
	}
}

class C
{
	public static int Main ()
	{
		S1? s1 = new S1 ();
		switch (s1)
		{
			default:
				return 1;
		}
	}
}