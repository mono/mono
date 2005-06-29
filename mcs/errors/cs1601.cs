// cs1601.cs: Method or delegate parameter cannot be of type `ref System.TypedReference'
//
using System;

class X {
	static void Main ()
	{
	}

	static void M (ref TypedReference a)
	{
	}
}
