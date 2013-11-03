using System;
using System.Collections.Generic;

public class Tests
{
	static void A<T>(IReadOnlyCollection<T> otherList)
	{
	}

	static void B<T>(IReadOnlyList<T> otherList)
	{
	}

	public static void Main ()
	{
		var ifacers = typeof(int[]).GetInterfaces ();

		var args = new string [0];
		A (args);
		B (args);

		IReadOnlyList<int> e1 = new int[0];
		IReadOnlyCollection<int> e2 = new int[0];
	}
}
