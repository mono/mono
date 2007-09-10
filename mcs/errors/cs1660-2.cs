// CS1660: Cannot convert `anonymous method' to type `int' because it is not a delegate type
// Line: 9

using System;

class X {
	static void Main ()
	{
		int o = delegate {};
	}
}
