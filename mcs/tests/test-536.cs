public delegate void FooHandler ();

public static class Test
{
	private static void OnFooTest ()
	{ 
	}

	public static event FooHandler Foo;

	public static void Main()
	{
		FooHandler foo = delegate {
			Foo += OnFooTest;
		};
	}
}
