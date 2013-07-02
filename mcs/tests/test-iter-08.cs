// Compiler options: -langversion:default

using System;
using System.Collections;

public class Foo : IDisposable
{
	public readonly int Data;

	public Foo (int data)
	{
		this.Data = data;
	}

	public bool disposed;

	public void Dispose ()
	{
		disposed = true;
	}
}

class X
{
	public static IEnumerable Test (int a, int b)
	{
		Foo foo3, foo4;

		using (Foo foo1 = new Foo (a), foo2 = new Foo (b)) {
			yield return foo1.Data;
			yield return foo2.Data;

			foo3 = foo1;
			foo4 = foo2;
		}

		yield return foo3.disposed;
		yield return foo4.disposed;
	}

	public static int Main ()
	{
		ArrayList list = new ArrayList ();
		foreach (object data in Test (3, 5))
			list.Add (data);

		if (list.Count != 4)
			return 1;
		if ((int) list [0] != 3)
			return 2;
		if ((int) list [1] != 5)
			return 3;
		if (!(bool) list [2])
			return 4;
		if (!(bool) list [3])
			return 5;

		return 0;
	}
}
