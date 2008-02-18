// CS1948: A range variable `T' conflicts with a method type parameter
// Line: 12


using System;
using System.Linq;

class C
{
	public static void Foo <T> ()
	{
		var e = from T in "a"
			select T;
	}
}
