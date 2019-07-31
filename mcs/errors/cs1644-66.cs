// CS1644: Feature `expression body event accessor' cannot be used because it is not part of the C# 6.0 language specification 
// Line: 11
// Compiler options: -langversion:6

using System;

class C
{
	public event EventHandler Event
	{
		add => Ignore ();
	}

	static void Ignore ()
	{
	}
}