// CS0019: Operator `&=' cannot be applied to operands of type `bool' and `byte?'
// Line: 10

public class Test
{
	public static void Main()
	{
		bool b = false;
		byte? b2 = 0;
		b &= b2;
	}
}
