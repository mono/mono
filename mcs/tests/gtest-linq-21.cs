using System;
using System.Linq;

static class Program
{
	static int Main()
	{
		int i = 0;
		var input = new int[] { 1 };
		var input2 = new int[] { 5 };
		var r = 
			from _ in input
			from y in input2
			select (Action)(() => Console.WriteLine("{0} {1} {2}", i, _, y));
		
		r.ToList () [0] ();
		return 0;
	}
}

