class C
{
	internal static int Foo<T> (T name, params object [] args)
	{
		return 2;
	}
	
	internal static int Foo (string name, params object [] args)
	{
		return 0;
	}
	
	internal static int InvokeMethod (string name, params object [] args)
	{
		return Foo (name, args);
	}
	
	public static int Main ()
	{
		return InvokeMethod ("abc");
	}

}
