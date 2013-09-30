// CS1650: Fields of static readonly field `C.s' cannot be assigned to (except in a static constructor or a variable initializer)
// Line: 15

struct S
{
	public int x { get; set; }
}

class C
{
	static readonly S s;

	public static void Main (string[] args)
	{
		s.x = 42;
	}
}

