delegate void FooHandler ();

class X
{
	public static void foo ()
	{ }

	public static void Main ()
	{
		object o = new FooHandler (foo);
		((FooHandler) o) ();
	}
}
