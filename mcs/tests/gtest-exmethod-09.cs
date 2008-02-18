

static class Test
{
	public static void Foo<T> (this string p1)
	{
	}
}

class C
{
	public static void Main ()
	{
		//int x = Test.Foo<bool> ("bb");
		"a".Foo<bool> ();
	}
}
