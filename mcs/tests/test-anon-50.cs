// Compiler options: -langversion:default

using System;
using System.Collections;

public class Test
{
	public IEnumerable Foo (int a)
	{
		yield return a;
		yield return a * a;
		yield break;
	}
}

class X
{
	public static int Main ()
	{
		Test test = new Test ();

		IEnumerable a = test.Foo (5);

		IEnumerator c = a.GetEnumerator ();
		if (!c.MoveNext ())
			return 5;
		if ((int) c.Current != 5)
			return 6;
		if (!c.MoveNext ())
			return 7;
		if ((int) c.Current != 25)
			return 8;

		IEnumerator d = a.GetEnumerator ();

		if ((int) c.Current != 25)
			return 9;
		if (!d.MoveNext ())
			return 10;
		if ((int) c.Current != 25)
			return 11;
		if ((int) d.Current != 5)
			return 12;

		if (c.MoveNext ())
			return 13;

		((IDisposable) a).Dispose ();
		return 0;
	}
}
