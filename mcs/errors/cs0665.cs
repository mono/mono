// cs0665.cs : Assignment in conditional expression is always constant; did you mean to use == instead of = ?
// Line: 10
// Compiler options: /warnaserror

class Test
{
	public void Foo (bool x)
	{
		bool b;
		if (b = true)
			System.Console.WriteLine (b);
	}
}

