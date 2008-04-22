// CS0019: Operator `==' cannot be applied to operands of type `int' and `null'
// Line: 10
// Compiler options: -langversion:ISO-1 

class C
{
	static int Foo { get { return 3; } set {} }
	
	static void Main ()
	{
		if (Foo == null) {}
	}
}
