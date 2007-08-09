// CS0828: An anonymous type property `Value' cannot be initialized with `lambda expression'
// Line: 9
// Compiler options: -langversion:linq

public class Test
{
	static void Main ()
	{
		var v = new { Value = i => 1 };
	}
}
