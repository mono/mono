// CS0019: Operator `+' cannot be applied to operands of type `Program' and `Program'
// Line: 8

public class Program
{
	static void Main ()
	{
		Program b = default (Program) + default (Program);
	}
}
