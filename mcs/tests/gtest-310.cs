using System;
using System.Collections.Generic;

namespace MonoBugs
{
	public static class IncompleteGenericInference
	{
		public static void DoSomethingGeneric<T1, T2>(IEnumerable<T1> t1, IDictionary<T1,T2> t2)
		{
		}

		public static void Main()
		{
			List<int> list = new List<int>();
			System.Collections.Generic.Dictionary<int, float> dictionary = new Dictionary<int, float>();
			DoSomethingGeneric(list, dictionary);
		}
	}
}
