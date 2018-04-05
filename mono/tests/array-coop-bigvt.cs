/*
array-coop.cs

Author:
    Jay Krell (jaykrell@microsoft.com)

Copyright 2018 Microsoft
Licensed under the MIT license. See LICENSE file in the project root for full license information.

This gets coverage of metadata/icall.c changes for coop conversion.
Some of these functions are inlined by the JIT, so reflection is used.
As well, there is printf in the implementation to verify coverage.

System.Array.GetValue (int index) => ves_icall_System_Array_GetValueImpl
int System.Rank => GetRank () => ves_icall_System_Array_GetRank
System.Array.Clear (array, index, length) => ves_icall_System_Array_ClearInternal
System.Array.SetGenericValueImpl<T> => ves_icall_System_Array_SetGenericValueImpl
System.Array.GetGenericValueImpl<T> => ves_icall_System_Array_GetGenericValueImpl

See
https://docs.microsoft.com/en-us/dotnet/api/system.reflection.methodinfo.makegenericmethod?view=netframework-4.7.1.
https://msdn.microsoft.com/en-us/library/system.array.rank(v=vs.110).aspx
https://stackoverflow.com/questions/1067312/how-to-use-methodinfo-invoke-to-set-property-value
*/

using System;
using System.Reflection;

struct big
{
	public big (int aa)
	{
		a = b = c = d = e = f = g = h = i = j = k = l = m = n = o = p = q = r = s = t = u = v = w = x = y = z = aa;
	}

	public static bool operator == (big a, big b) { return a.i == b.i; }
	public static bool operator != (big a, big b) { return a.i != b.i; }

	long a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, u, v, w, x, y, z;
}

class test
{

	big [] array1 = new big [10]{
		new big (1), new big (2), new big (3), new big (4), new big (5),
		new big (6), new big (7), new big (8), new big (9), new big (10)
	};
	big [,] array2 = new big [10,3] {
		{new big (10), new big (20), new big (30)},
		{new big (100), new big (200), new big (300)},
		{new big (1000), new big (2000), new big (3000)},
		{new big (10000), new big (20000), new big (30000)},
		{new big (100000), new big (200000), new big (300000)},
		{new big (11), new big (21), new big (31)},
		{new big (101), new big (201), new big (301)},
		{new big (1001), new big (2001), new big (3001)},
		{new big (10001), new big (20001), new big (30001)},
		{new big (100001), new big (200001), new big (300001)}};
	big [][] array3 = new big [10][] {
		new big [1]{new big (2)}, new big [1]{new big (3)}, new big [1]{new big (4)}, new big [1]{new big (5)}, new big [1]{new big (6)},
		new big [1]{new big (7)}, new big [1]{new big (8)}, new big [1]{new big (9)}, new big [1]{new big (10)}, new big [1]{new big (11)} };

	static void assert (bool expr)
	{
		if (expr)
			return;
		System.Console.WriteLine ("failure");
		Environment.Exit (1);
	}

	void test_clear ()
	{
		big [] array1 = new big [10]{
			new big (1), new big (2), new big (3), new big (4), new big (5),
			new big (6), new big (7), new big (8), new big (9), new big (10)
		};
		var dt0 = new big (0);

		assert (array1 [0] != dt0);
		assert (array1 [1] != dt0);
		assert (array1 [2] != dt0);
		assert (array1 [3] != dt0);
		System.Array.Clear (array1, 0, 2);
		assert (array1 [0] == dt0);
		assert (array1 [1] == dt0);
		assert (array1 [2] != dt0);
		assert (array1 [3] != dt0);
		System.Array.Clear (array1, 3, 1);
		assert (array1 [0] == dt0);
		assert (array1 [1] == dt0);
		assert (array1 [2] != dt0);
		assert (array1 [3] == dt0);

		big [][] array3 = new big [10][] {
			new big [1]{new big (2)}, new big [1]{new big (3)}, new big [1]{new big (4)}, new big [1]{new big (5)}, new big [1]{new big (6)},
			new big [1]{new big (7)}, new big [1]{new big (8)}, new big [1]{new big (9)}, new big [1]{new big (10)}, new big [1]{new big (11)} };

		assert (array3 [0] != null);
		assert (array3 [1] != null);
		assert (array3 [2] != null);
		assert (array3 [3] != null);
		System.Array.Clear (array3, 1, 2);
		assert (array3 [0] != null);
		assert (array3 [1] == null);
		assert (array3 [2] == null);
		assert (array3 [3] != null);
	}

	void test_get_value ()
	{
		assert ((big)array1.GetValue (0) == array1[0]);
		assert ((big)array1.GetValue (3) == array1[3]);
	}

	void test_get_rank ()
	{
		Type type = typeof (System.Array);
		PropertyInfo pi = type.GetProperty ("Rank");
		assert ((int)pi.GetValue (array1) == array1.Rank);
		assert ((int)pi.GetValue (array2) == array2.Rank);
		assert ((int)pi.GetValue (array3) == array3.Rank);
		assert ((int)pi.GetValue (array3 [0]) == array3 [0].Rank);
	}

	void test_get_generic_value ()
	{
		Type type = typeof (System.Array);
		MethodInfo mig = type.GetMethod ("GetGenericValueImpl", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		assert (mig != null);
	        MethodInfo mi = mig.MakeGenericMethod (typeof (big));
		assert (mi != null);

		var args = new object [2];
		for (int i = 0; i < array1.Length; ++i) {
			args [0] = i;
			args [1] = null;
			mi.Invoke (array1, args);
			assert (array1 [i] == (big)args [1]);
		}
	}

	void test_set_generic_value ()
	{
		big [] array2 = new big [10];

		for (int i = 0; i < array1.Length; ++i)
			assert (array1 [i] != array2 [i]);

		Type type = typeof (System.Array);
		MethodInfo mig = type.GetMethod ("SetGenericValueImpl", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		assert (mig != null);
	        MethodInfo mi = mig.MakeGenericMethod (typeof (big));
		assert (mi != null);

		var args = new object [2];
		for (int i = 0; i < array1.Length; ++i) {
			args [0] = i;
			args [1] = new big (i + 1);
			mi.Invoke (array2, args);
		}

		for (int i = 0; i < array1.Length; ++i)
			assert (array1 [i] == array2 [i]);
	}

	void main ()
	{
		Console.WriteLine ("test_set_generic_value");
		try {
			test_set_generic_value ();
		} catch (System.Reflection.TargetInvocationException) // for FullAOT
		{ }
		Console.WriteLine ("test_get_generic_value");
		test_get_generic_value ();
		Console.WriteLine ("test_clear");
		test_clear ();
		Console.WriteLine ("test_get_value");
		test_get_value ();
		Console.WriteLine ("test_get_rank");
		test_get_rank ();
	}

	public static void Main (string[] args)
	{
		new test().main();
	}
}
