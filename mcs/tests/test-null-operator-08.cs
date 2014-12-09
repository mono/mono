using System;

interface I
{
	void Foo (bool expected);
}

struct S : I
{
	bool flag;

	public void Foo (bool expected)
	{
		Console.WriteLine (flag);
		if (expected != flag)
			throw new ApplicationException ();

		flag = true;
	}
}

class Program
{
	static void M<T> (T x)
	{
		object s = x?.ToString ();
		System.Console.WriteLine (s);

		var h = x?.GetHashCode ();
		System.Console.WriteLine (h);
	}

	static void M2<T> (T[] x)
	{
		object s = x?.ToString ();
		System.Console.WriteLine (s);

		var h = x?.GetHashCode ();
		System.Console.WriteLine (h);
	}

	static void M2_2<T> (T[] x)
	{
		object s = x[0]?.ToString ();
		System.Console.WriteLine (s);

		var h = x[0]?.GetHashCode ();
		System.Console.WriteLine (h);
	}

	static void M3<T> (T? x) where T : struct
	{
		object s = x?.ToString ();
		System.Console.WriteLine (s);

		var h = x?.GetHashCode ();
		System.Console.WriteLine (h);
	}

	static void TestAddress_1<T> (T t) where T : I
	{
		t?.Foo (false);
		t?.Foo (true);
	}

	static void TestAddress_2<T> (T[] t) where T : I
	{
		t[0]?.Foo (false);
		t[0]?.Foo (true);
	}

	static void Main()
	{
		M<string> (null);
		M (1);
		M("X");

		M2<int> (null);
		M2<string> (null);
		M2 (new [] { 1 });
		M2 (new [] { "x" });

		M2_2 (new string [1]);
		M2_2 (new int [1]);

		M3<int> (1);
		M3<byte> (null);

		TestAddress_1 (new S ());
		var ar = new [] { new S () };
		TestAddress_2 (ar);
	}
}