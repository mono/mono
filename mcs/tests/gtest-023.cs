class Foo<T>
{
	public void Hello ()
	{ }

	public void World (T t)
	{
		Hello ();
	}
}

//
// This is some kind of a `recursive' declaration:
//
// Note that we're using the class we're currently defining (Bar)
// as argument of its parent.
//
// Is is important to run the resulting executable since this is
// both a test for the compiler and the runtime.
//

class Bar : Foo<Bar>
{
	public void Test ()
	{
		Hello ();
		World (this);
	}
}

class X
{
	public static void Main ()
	{ }
}
