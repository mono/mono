// CS1948: A range variable `T' conflicts with a method type parameter
// Line: 13

using System;
using System.Linq;

class C
{
	public static void Foo <T> ()
	{
		var s = "0";
		var e = from T in "a"
			select T + s;
	}
}
