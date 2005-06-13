// cs0175.cs: Use of keyword `base' is not valid in this context
// Line: 8
using System.Collections;
class Collection : CollectionBase
{
	public int Add (int x)
	{
		return ((IList) base).Add (x);
	}
}


