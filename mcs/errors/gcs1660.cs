// CS1660: Cannot convert `lambda expression' to non-delegate type `object'
// Line: 9

using System;

class X {
	static void Main ()
	{
		object o = () => true;
	}
}
