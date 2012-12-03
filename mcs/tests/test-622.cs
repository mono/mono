struct A
{
	public A (int a)
	{
	}
}

class B
{
	public B (int a)
	{
	}
}

class X {
	static void Foo (out A value)
	{
		value = new A (1);
	}
	
	static void Foo (out object value)
	{
		value = new B (1);
	}
	
	public static int Main ()
	{
		A o;
		Foo (out o);

		object b;
		Foo (out b);

		return 0;
	}
}
