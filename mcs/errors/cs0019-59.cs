// CS0019: Operator `^' cannot be applied to operands of type `S2' and `float'
// Line: 9

public class Test
{
	public static void Main()
	{
		S2 s2 = new S2 ();
		int r = s2 ^ 5.04f;
	}
}

struct S2
{
	public static int operator ^ (double? p1, S2 s2)
	{
		return 1;
	}
	
	public static implicit operator int? (S2 s1)
	{ 
		return int.MinValue;
	}
}
