// cs0120-4.cs: `X.Y': An object reference is required for the nonstatic field, method or property
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






