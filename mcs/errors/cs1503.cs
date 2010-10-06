// CS1503: Argument `#1' cannot convert `int' expression to type `bool'
// Line: 15

class A
{
	public static void Foo (bool test)
	{
	}
}

class B
{
	public static void Main()
	{
		A.Foo (1);
	}
}
