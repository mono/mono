struct Foo {
	public int x;
	public override int GetHashCode ()
	{
		return base.GetHashCode ();
	}
}

class Test {
	static void Main ()
	{
		Foo foo = new Foo ();
		System.Console.WriteLine (foo.GetHashCode ());
	}
}
