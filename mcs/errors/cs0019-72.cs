// CS0019: Operator `??' cannot be applied to operands of type `void' and `throw expression'
// Line: 20

class C
{
	public static void Main ()
	{
		var s = Main () ?? throw null;
	}
}