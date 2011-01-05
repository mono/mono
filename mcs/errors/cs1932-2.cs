// CS1932: A range variable `ii' cannot be initialized with `method group'
// Line: 12

using System;
using System.Linq;

class C
{
	public void Foo (int i)
	{
		var e = from v in "a"
			let ii = Foo
			select v;
	}
}
