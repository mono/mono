// cs1660.cs: Cannot convert anonymous method block to type `object' because it is not a delegate type
// Line: 9

using System;

class X {
	static void Main ()
	{
		object o = delegate {};
	}
}
