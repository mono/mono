// CS0828: An anonymous type property `Value' cannot be initialized with `lambda expression'
// Line: 9


public class Test
{
	static void Main ()
	{
		var v = new { Value = i => 1 };
	}
}
