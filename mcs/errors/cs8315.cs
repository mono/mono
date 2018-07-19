// CS8315: Operator `==' is ambiguous on operands `default' and `default'
// Line: 9
// Compiler options: -langversion:latest

class C
{
	static void Main ()
	{
		bool d = default == default;
	}
}