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
int System.Rang => GetRank () => ves_icall_System_Array_GetRank
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

public struct small
{
	public small (int j) { i = j; }
	public static bool operator == (small a, small b) { return a.i == b.i; }
	public static bool operator != (small a, small b) { return a.i != b.i; }

	int i;
}

class test
{
	small [] array1 = new small [10]{
		new small (1), new small (2), new small (3), new small (4), new small (5),
		new small (6), new small (7), new small (8), new small (9), new small (10)
	};
	small [,] array2 = new small [10,3] {
		{new small (10), new small (20), new small (30)},
		{new small (100), new small (200), new small (300)},
		{new small (1000), new small (2000), new small (3000)},
		{new small (10000), new small (20000), new small (30000)},
		{new small (100000), new small (200000), new small (300000)},
		{new small (11), new small (21), new small (31)},
		{new small (101), new small (201), new small (301)},
		{new small (1001), new small (2001), new small (3001)},
		{new small (10001), new small (20001), new small (30001)},
		{new small (100001), new small (200001), new small (300001)}};
	small [][] array3 = new small [10][] {
		new small [1]{new small (2)}, new small [1]{new small (3)}, new small [1]{new small (4)}, new small [1]{new small (5)}, new small [1]{new small (6)},
		new small [1]{new small (7)}, new small [1]{new small (8)}, new small [1]{new small (9)}, new small [1]{new small (10)}, new small [1]{new small (11)} };

	static void assert (bool expr)
	{
		if (expr)
			return;
		System.Console.WriteLine ("failure");
		Environment.Exit (1);
	}

	void test_clear ()
	{
		small [] array1 = new small [10]{
			new small (1), new small (2), new small (3), new small (4), new small (5),
			new small (6), new small (7), new small (8), new small (9), new small (10)
		};
		var dt0 = new small (0);

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

		small [][] array3 = new small [10][] {
			new small [1]{new small (2)}, new small [1]{new small (3)}, new small [1]{new small (4)}, new small [1]{new small (5)}, new small [1]{new small (6)},
			new small [1]{new small (7)}, new small [1]{new small (8)}, new small [1]{new small (9)}, new small [1]{new small (10)}, new small [1]{new small (11)} };

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
		assert ((small)array1.GetValue (0) == array1[0]);
		assert ((small)array1.GetValue (3) == array1[3]);
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
	        MethodInfo mi = mig.MakeGenericMethod (typeof (small));
		assert (mi != null);

		var args = new object [2];
		for (int i = 0; i < array1.Length; ++i) {
			args [0] = i;
			args [1] = null;
			mi.Invoke (array1, args);
			assert (array1 [i] == (small)args [1]);
		}
	}

	void test_set_generic_value ()
	{
		small [] array2 = new small [10];

		for (int i = 0; i < array1.Length; ++i)
			assert (array1 [i] != array2 [i]);

		Type type = typeof (System.Array);
		MethodInfo mig = type.GetMethod ("SetGenericValueImpl", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		assert (mig != null);
	        MethodInfo mi = mig.MakeGenericMethod (typeof (small));
		assert (mi != null);

		var args = new object [2];
		for (int i = 0; i < array1.Length; ++i) {
			args [0] = i;
			args [1] = new small (i + 1);
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
		new test().main ();
	}
}
