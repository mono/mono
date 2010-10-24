// CS0121: The call is ambiguous between the following methods or properties: `C.Foo(int, long, params string[])' and `C.Foo(long, int, params string[])'
// Line: 9

class C
{
	public static void Main ()
	{
		var d = new C ();
		d.Foo (b: 1, x: "", a : 2);
	}

	public void Foo (int a, long b, params string[] x)
	{
	}

	public void Foo (long b, int a, params string[] x)
	{
	}
}
