// cs0665.cs : Assignment in conditional expression is always constant; did you mean to use == instead of = ?
// Compiler options: /warnaserror
// Line: 9
class Test
{
	public void Foo (bool x)
	{
		bool b;
		if (b = true)
			System.Console.WriteLine (b);
	}
}

