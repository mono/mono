

using System;
using System.Collections.Generic;
using System.Linq;

class TestGroupBy
{
	public static int Main ()
	{
		int[] int_array = new int [] { 0, 1, 2, 3, 4 };
		
		IEnumerable<IGrouping<int, int>> e;
		
		// group by i % 2 from 1
		e = from int i in int_array group 1 by i % 2;

		List<IGrouping<int, int>> output = e.ToList ();
		if (output.Count != 2)
			return 1;
		
		foreach (IGrouping<int, int> ig in e) {
			Console.WriteLine (ig.Key);
			foreach (int value in ig) {
				Console.WriteLine ("\t" + value);
				if (value != 1)
					return 2;
			}
		}

		// group by i % 2 from i
		e = from int i in int_array group i by i % 2;

		output = e.ToList ();
		if (output.Count != 2)
			return 1;
		
		int[] results_a = new int[] { 0, 2, 4, 1, 3 };
		int pos = 0;
		
		foreach (IGrouping<int, int> ig in e) {
			Console.WriteLine (ig.Key);
			foreach (int value in ig) {
				Console.WriteLine ("\t" + value);
				if (value != results_a [pos++])
					return 3;
			}
		}
		
		Console.WriteLine ("OK");
		return 0;
	}
}
