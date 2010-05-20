using System;

static class E
{
	public static string Test<T> (this C c, T s, Func<T> f)
	{
		return "s";
	}
}

public class C
{
	int Test<T> (T b, Func<bool> f)
	{
		return 1;
	}

	static string Foo<T> (T t, Action<T> a)
	{
		a (t);
		return "f";
	}

	public static void Main ()
	{
		var c = new C ();
		Action<string> f = l => Foo ("v", l2 => c.Test ("a", () => ""));
		f ("-");
	}
}
