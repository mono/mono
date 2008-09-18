// CS0120: An object reference is required to access non-static member `X.Y'
// Line: 11

using System;

class X {
	// Public properties and variables.
	public string Y;

	// Constructors.
	public X()
	{
	}

	// Public static methods.
	public static void Main(string[] Arguments)
	{
		X.Y = "";
	}
}






