// CS1660: Cannot convert `lambda expression' to type `object' because it is not a delegate type
// Line: 10
// Compiler options: -langversion:linq

using System;

class X {
	static void Main ()
	{
		object o = () => true;
	}
}
