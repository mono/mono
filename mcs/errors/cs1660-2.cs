// CS1660: Cannot convert `anonymous method' to non-delegate type `int'
// Line: 9

using System;

class X {
	static void Main ()
	{
		int o = delegate {};
	}
}
