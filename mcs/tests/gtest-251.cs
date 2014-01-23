using System;
using SCG = System.Collections.Generic;

public interface ISorted<S>
{
	void AddSorted<T> (SCG.IEnumerable<T> items)
		where T : S;
}

public class SortedIndexedTester<T>
{
	public void Test (ISorted<int> sorted)
	{
		sorted.AddSorted (new int[] { 31, 62, 63, 93 });
	}
}

class X
{
	public static void Main ()
	{ }
}
