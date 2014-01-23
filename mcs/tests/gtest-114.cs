using System;
using System.Collections.Generic;

public class NaturalComparer<T> : IComparer<T>
	where T: IComparable<T>
{
	public int Compare (T a, T b)
	{
		return a.CompareTo (b);
	}
}

public class X
{
	class Test : IComparable<Test>
	{
		public int CompareTo (Test that)
		{
			return 0;
		}

		public bool Equals (Test that)
		{
			return false;
		}
	}

	public static void Main ()
	{
		IComparer<Test> cmp = new NaturalComparer<Test> ();
		Test a = new Test ();
		Test b = new Test ();
		cmp.Compare (a, b);
	}
}
