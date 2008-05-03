// CS0019: Operator `??' cannot be applied to operands of type `null' and `method group'
// Line: 8

class C
{
	delegate void D ();

	static void Main ()
	{
		D d = null ?? Main;
	}
}
