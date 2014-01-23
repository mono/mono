// Compiler options: -langversion:default

using System;
using System.Collections;

public class Test
{
	public IEnumerable Foo (int a)
	{
		try {
			try {
				yield return a;
			} finally {
				Console.WriteLine ("Hello World");
			}

			Console.WriteLine ("Next block");

			try {
				yield return a * a;
			} finally {
				Console.WriteLine ("Boston");
			}
		} finally {
			Console.WriteLine ("Outer finally");
		}

		Console.WriteLine ("Outer block");
		yield break;
	}
}

class X
{
	public static int Main ()
	{
		Test test = new Test ();

		ArrayList list = new ArrayList ();
		foreach (object o in test.Foo (5))
			list.Add (o);

		if (list.Count != 2)
			return 1;
		if ((int) list [0] != 5)
			return 2;
		if ((int) list [1] != 25)
			return 3;

		IEnumerable a = test.Foo (5);

		IEnumerator b = a as IEnumerator;
		if (b != null) {
			if (b.MoveNext ())
				return 4;
		}

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
