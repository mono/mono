using System;

public delegate void Foo<V> (V v);

public delegate void Bar<W> (W w);


class Test<T>
{
	public static void Hello<S> (T t, S s)
	{
		Foo<long> foo = delegate (long r) {
			Console.WriteLine (r);
			Bar<T> bar = delegate (T x) {
				Console.WriteLine (r);
				Console.WriteLine (t);
				Console.WriteLine (s);
				Console.WriteLine (x);
			};
			bar (t);
		};
		foo (5);
	}
}

class X
{
	public static void Main ()
	{
		Test<string>.Hello ("World", 3.1415F);
	}
}
