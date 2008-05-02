// CS0019: Operator `+' cannot be applied to operands of type `method group' and `method group'
// Line: 10

using System;

public class Test
{
	public static void Main ()
	{
		Console.WriteLine (Main + Main);
	}
}
