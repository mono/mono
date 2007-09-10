// CS1660: Cannot convert `anonymous method' to type `object' because it is not a delegate type
// Line: 9

using System;

class X {
	static void Main ()
	{
		object o = delegate {};
	}
}
