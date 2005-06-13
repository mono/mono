// cs1660.cs: Cannot convert anonymous method to `object', since it is not a delegate
// Line: 9

using System;

class X {
	static void Main ()
	{
		object o = delegate {};
	}
}
