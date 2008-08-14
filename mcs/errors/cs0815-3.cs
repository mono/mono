// CS0815: An implicitly typed local variable declaration cannot be initialized with `lambda expression'
// Line: 9


public class Test
{
	static void Main ()
	{
		var l = x => x + 1;
	}
}

