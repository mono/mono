// CS0828: An anonymous type property `Value' cannot be initialized with `anonymous method'
// Line: 8

public class Test
{
	static void Main ()
	{
		var v = new { Value = i => 1 };
	}
}
