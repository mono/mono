// CS0154: The property or indexer `B.Foo' cannot be used in this context because it lacks the `get' accessor
// Line: 13
// this is bug 55780.

class A {
	public int Foo { get { return 1; } }
}

class B : A {
	public new int Foo { set { } }
	static void Main ()
	{
		System.Console.WriteLine (new B ().Foo);
	}
}