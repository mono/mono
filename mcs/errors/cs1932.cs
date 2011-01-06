// CS1932: A range variable `ii' cannot be initialized with `void'
// Line: 13


using System;
using System.Linq;

class C
{
	public void Foo (int i)
	{
		var e = from v in "a"
			let ii = Foo (2)
			select v;
	}
}
