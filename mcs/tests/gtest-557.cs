using System.Collections.Generic;

class Test
{
	static U[] Foo<T, U> (T[] arg) where T : class, U
	{
		return arg;
	}

	static void TestByRef<T> ()
	{
		T[] array = new T[10];
		PassByRef (ref array[0]);
	}

	static void PassByRef<T> (ref T t)
	{
		t = default (T);
	}

	public static int Main ()
	{
		foreach (var e in Foo<string, object> (new string[] { "as" })) {
		}

		TestByRef<byte> ();

		return 0;
	}
}
