// CS0815: An implicitly typed local variable declaration cannot be initialized with `anonymous method'
// Line: 8

public class Test
{
	static void Main ()
	{
		var l = x => x + 1;
	}
}

