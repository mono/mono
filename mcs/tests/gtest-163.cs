using System;
using System.Collections;
using System.Collections.Generic;

public class Foo<T>
{
	public IEnumerator<T> getEnumerator (int arg)
	{
		if (arg == 1) {
			int foo = arg;
			Console.WriteLine (foo);
		}

		if (arg == 2) {
			int foo = arg;
			Console.WriteLine (foo);
		}

		yield break;
	}
}

class X
{
	public static void Main ()
	{ }
}
