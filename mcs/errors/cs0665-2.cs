// Cs0665: Assignment in conditional expression is always constant. Did you mean to use `==' instead ?
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

