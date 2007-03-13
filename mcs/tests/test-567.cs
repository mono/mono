// Test for bug 63841 -- GetElementType() returns underlying enum type
// instead of null

using System;

class Test
{
	static void Main ()
	{
		Type a = typeof (A).GetElementType ();
		if (a != null)
			Console.WriteLine ("ERROR a != null");

		Type b = typeof (B).GetElementType ();
		if (b != null)
			Console.WriteLine ("ERROR b != null");

		Type c = typeof (C).GetElementType ();
		if (c != null)
			Console.WriteLine ("ERROR c != null");
	}

	enum A { }
	enum B : byte { }
	enum C : byte { X, Y, Z }
}
