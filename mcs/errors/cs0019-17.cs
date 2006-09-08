// CS0019: Operator `==' cannot be applied to operands of type `int' and `string'
// Line: 8

class S
{
	static int i;
	
	static bool Foo ()
	{
		return i == "";
	}
}
