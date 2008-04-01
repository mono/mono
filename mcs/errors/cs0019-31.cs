// CS0019: Operator `==' cannot be applied to operands of type `S' and `S'
// Line: 14

struct S
{
}

class C
{
	public static void Main ()
	{
		S s;
		S x;
		bool b = s == x;
	}
}
