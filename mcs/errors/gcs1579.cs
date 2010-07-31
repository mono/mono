// CS1579: foreach statement cannot operate on variables of type `Foo' because it does not contain a definition for `GetEnumerator' or is inaccessible
// Line: 12

using System;
using System.Collections;

public class Test
{
	public static void Main ()
	{
		Foo f = new Foo ();
		foreach (object o in f)
			Console.WriteLine (o);
	}
}

public class Foo
{
	public Func<IEnumerator> GetEnumerator;
}
