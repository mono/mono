class Foo<T>
{
	public void Hello ()
	{ }

	public void World (T t)
	{
		Hello ();
	}
}

class Bar : Foo<int>
{
	public void Test ()
	{
		Hello ();
		World (4);
	}
}

class X
{
	static void Main ()
	{
		Bar bar = new Bar ();
		bar.Test ();
	}
}
