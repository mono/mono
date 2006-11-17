using System;

public delegate void Foo<R> (R r);

class Test<T>
{
	public static void Hello<S> (T t, S s)
	{
		Foo<long> foo = delegate (long r) {
			Console.WriteLine (r);
			Foo<T> bar = delegate (T x) {
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
	static void Main ()
	{
	}
}
