using System.Collections.Generic;
using System.Linq;

class X
{
	public static void Main ()
	{
		ICollection<int> coll = new List<int> ();
		coll.Add (0);
		coll.Add (1);
		coll.Add (2);

		coll.AsParallel ().WithExecutionMode (ParallelExecutionMode.ForceParallelism).ToArray ();
	}
}