class Foo<T>
{
	public void Hello ()
	{ }

	public void World (T t)
	{
		Hello ();
	}
}

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
	static void Main ()
	{ }
}
