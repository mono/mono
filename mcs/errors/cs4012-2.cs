// CS4012: Parameters or local variables of type `System.TypedReference' cannot be declared in async methods or iterators
// Line: 9

using System;
using System.Collections;

class C
{
	public IEnumerable Iter ()
	{
		int i = 2;
		TypedReference tr = __makeref (i);
		yield return 1;
	}
}
