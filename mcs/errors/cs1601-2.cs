// cs1601-2.cs: Method or delegate parameter cannot be of type `ref System.ArgIterator'
//
using System;

class X {
	static void Main ()
	{
	}

	static void M (ref ArgIterator a)
	{
	}
}
