// CS0121: The call is ambiguous between the following methods or properties: `C.Foo(int, params string[])' and `C.Foo(string[], int)'
// Line: 9

class C
{
	public static void Main ()
	{
		var d = new C ();
		d.Foo (x: 1, y: new [] { "" });
	}

	public void Foo (int x, params string[] y)
	{
	}

	public void Foo (string[] y, int x)
	{
	}
}
