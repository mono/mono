// CS8176: Iterators cannot use by-reference variables
// Line: 12

using System.Collections.Generic;

class X
{
	int x;

	IEnumerable<int> Test ()
	{
		ref int y = ref x;
		yield break;
	}
}