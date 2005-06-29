// cs1660-2.cs: Cannot convert anonymous method block to type `int' because it is not a delegate type
// Line: 9

using System;

class X {
	static void Main ()
	{
		int o = delegate {};
	}
}
