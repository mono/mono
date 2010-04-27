// CS0019: Operator `==' cannot be applied to operands of type `A.D' and `anonymous method'
// Line: 11

class A
{
	delegate void D ();

	static void Main ()
	{
		D d = null;
		bool r = d == (() => { });
	}
}
