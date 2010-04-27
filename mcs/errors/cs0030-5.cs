// CS0030: Cannot convert type `System.Enum' to `Blah.S'
// Line: 12

using System;

public class Blah {
	struct S {}
	enum E { Val }
	
	public static void Main ()
	{
		S s = (S)(Enum)E.Val;
	}
}