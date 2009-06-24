// CS1525: Unexpected symbol `)', expecting `.', or `['
// Line: 10

using System.Collections;

class Collection : CollectionBase
{
	public int Add (int x)
	{
		return ((IList) base).Add (x);
	}
}


