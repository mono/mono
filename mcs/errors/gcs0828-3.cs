// CS0828: An anonymous type property `Value' cannot be initialized with `anonymous method'
// Line: 9
// Compiler options: -langversion:linq

public class Test
{
	static void Main ()
	{
		var v = new { Value = delegate () { return 1; } };
	}
}
