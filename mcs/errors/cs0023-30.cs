// CS0023: The `is' operator cannot be applied to operand of type `default'
// Line: 9
// Compiler options: -langversion:latest

class C
{
	static void Main ()
	{
		bool d = default is C;
	}
}