// CS1763: The expression being assigned to optional parameter `c' must be a constant or default value
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
