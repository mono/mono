interface I<T>
{
}

class Foo<T1, T2> where T2 : I<I<T1>>
{
	public Foo (T2 t2)
	{
	}
}

class Bar : I<I<string>>
{
	public static int Main ()
	{
		var foo = new Foo<string, Bar> (new Bar ());
		return 0;
	}
}
