// cs0175: wrong context for use of `base' keyword
// Line: 8
using System.Collections;
class Collection : CollectionBase
{
	public int Add (int x)
	{
		return ((IList) base).Add (x);
	}
}


