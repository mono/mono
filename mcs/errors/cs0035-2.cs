// CS0035: Operator `++' is ambiguous on an operand of type `MyType'
// Line: 31

public class MyType
{
	public static implicit operator float (MyType v)
	{
		return 0;
	}

	public static implicit operator decimal (MyType v)
	{
		return 0;
	}

	public static implicit operator MyType (float v)
	{
		return null;
	}

	public static implicit operator MyType (decimal v)
	{
		return null;
	}
}

class Test
{
	static void test (MyType x)
	{
		x++;
	}
}