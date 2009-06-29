// CS1763: Optional parameter `c' of type `C' can only be initialized with `null'
// Line: 10
// Compiler options: -langversion:future

struct S
{
}

class C
{
	public static void Test (C c = new S ())
	{
	}
}
