// CS0665: Assignment in conditional expression is always constant. Did you mean to use `==' instead ?
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

