// CS0815: An implicitly typed local variable declaration cannot be initialized with `anonymous method'
// Line: 9


public class Test
{
	static void Main ()
	{
		var d = delegate {};
	}
}

