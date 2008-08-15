// CS0828: An anonymous type property `Value' cannot be initialized with `method group'
// Line: 12

public class Test
{
	static void Error ()
	{
	}
	
	static void Main ()
	{
		var v = new { Value = Error };
	}
}
