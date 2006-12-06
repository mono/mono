// CS0019: Operator `+' cannot be applied to operands of type `AA' and `uint'
// Line: 11

enum AA : byte { a, b = 200 }

public class C
{
	public static void Main ()
	{
		const uint ul = 1;
		const AA b = AA.a + ul;
	}
}