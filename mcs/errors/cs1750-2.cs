// CS1750: Optional parameter expression of type `S' cannot be converted to parameter type `C'
// Line: 10

struct S
{
}

class C
{
	public static void Test (C c = new S ())
	{
	}
}
