class Test {
	static object Foo (int? i)
	{
		return (object)i;
	}
	
	static object FooG<T> (T? i) where T : struct
	{
		return (object)i;
	}
	
	public static int Main ()
	{
		object o = Foo (null);
		if (o != null)
			return 1;

		o = FooG<bool> (null);
		if (o != null)
			return 2;
		
		System.Console.WriteLine ("OK");
		return 0;
	}
}
