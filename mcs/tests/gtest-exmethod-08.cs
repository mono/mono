
// It tests collision between multiple external methods and also whether
// we import external methods when same namespace does not exist locally

using System.Collections.Generic;
using System.Linq;

class C
{
	public static void Main ()
	{
		List<int> first  = new List<int> ();
		List<int> second = new List<int> ();

		IEnumerable<int> q = first.Except(second);
	}
}
