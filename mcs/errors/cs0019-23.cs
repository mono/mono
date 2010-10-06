// CS0019: Operator `==' cannot be applied to operands of type `anonymous method' and `anonymous method'
// Line: 8

public class C
{
	public static void Main ()
	{
		bool b = delegate () {} == delegate () {};
	}
}
