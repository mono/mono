partial class Foo<T> {}
partial class Foo<T> {
	public delegate int F ();
}

class Bar {
	static int g () { return 0; }
	public static int Main ()
	{
		Foo<int>.F f = g;
		return f ();
	}
}
