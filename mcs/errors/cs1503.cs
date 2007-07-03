// CS1503: Argument 1: Cannot convert type `int' to `bool'
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
