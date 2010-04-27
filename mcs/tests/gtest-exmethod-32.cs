using System;
using System.Collections;
using System.Linq;

class A : IEnumerable
{
	protected int Count
	{
		get { return 0; }
	}

	IEnumerator IEnumerable.GetEnumerator ()
	{
		return null;
	}
}

class G<T> where T : A
{
	void Test ()
	{
		T var = null;
		int i = var.Count ();
	}
}

public static class Extensions
{
	public static int Count (this IEnumerable seq)
	{
		return 0;
	}

	public static void Main ()
	{
	}
}
