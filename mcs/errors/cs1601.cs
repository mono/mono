// CS1601: Method or delegate parameter cannot be of type `ref System.TypedReference'
// Line: 10
using System;

class X {
	static void Main ()
	{
	}

	static void M (ref TypedReference a)
	{
	}
}
