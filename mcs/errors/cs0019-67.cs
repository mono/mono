// CS0019: Operator `+=' cannot be applied to operands of type `dynamic' and `anonymous method'
// Line: 9

class MainClass
{
	public static void Main ()
	{
		dynamic d = null;
		d += delegate {};
	}
}
