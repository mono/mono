// cs0665-2.cs : Assignment in conditional expression is always constant; did you mean to use == instead of = ?
// Line: 10
// Compiler options: /warnaserror

class Test
{
	public bool Foo (bool x)
	{
		bool b;
		return (b = true) ? true : b;
	}
}

