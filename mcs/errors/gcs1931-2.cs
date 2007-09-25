// CS1931: A range variable `i' conflicts with a previous declaration of `i'
// Line: 13
// Compiler options: -langversion:linq

using System;
using System.Linq;

class C
{
	public void Foo (int i)
	{
		var e = from v in "a"
			let i = 2
			select v;
	}
}
