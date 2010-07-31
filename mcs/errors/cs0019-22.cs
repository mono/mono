// CS0019: Operator `-' cannot be applied to operands of type `AA' and `long'
// Line: 11

enum AA : short { a, b = 200 }

public class C
{
	public static void Main ()
	{
		const long ul = 1;
		AA b = AA.a - ul;
	}
}
