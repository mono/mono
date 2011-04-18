// CS0019: Operator `==' cannot be applied to operands of type `dynamic' and `anonymous method'
// Line: 9

class C
{
	static void Main ()
	{
		dynamic x = null;
		var y = x == (() => 1);
	}
}
