// CS0019: Operator `==' cannot be applied to operands of type `C.E1' and `C.E2'
// Line: 21

class C
{
	enum E1 : long
	{
		A
	}
	
	enum E2 : sbyte
	{
		A
	}
	
	public static void Main ()
	{
		E1 b = E1.A;
		E2 d = E2.A;
		
		bool brr = b == d;
	}
}
