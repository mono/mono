using System;
using System.Collections;
using System.Collections.Generic;

public delegate void Foo ();

public class Test
{
	public static implicit operator Foo (Test test)
	{
		return delegate { Console.WriteLine ("Hello World!"); };
	}

	public static IEnumerable<Test> operator + (Test test, Test foo)
	{
		yield return test;
		yield return foo;
	}

	public IEnumerable<int> Foo {
		get {
			yield return 3;
		}

		set {
			Console.WriteLine ("Foo!");
		}
	}

	static void Main ()
	{
		Test test = new Test ();
		Foo foo = test;
		foo ();
		foreach (Test t in test + test)
			Console.WriteLine (t);
	}
}
