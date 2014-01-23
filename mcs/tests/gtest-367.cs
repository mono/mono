using System;

class Foo {}

class Repro {

	public static void Main ()
	{
	}

	static void Bar<TFoo> (TFoo foo) where TFoo : Repro
	{
		Baz (foo, Gazonk);
	}

	static void Baz<T> (T t, Action<T> a)
	{
		a (t);
	}

	static void Gazonk (Repro f)
	{
	}
}
