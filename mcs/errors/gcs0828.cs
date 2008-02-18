// CS0828: An anonymous type property `Value' cannot be initialized with `null'
// Line: 9


public class Test
{
	static void Main ()
	{
		int A = 9;
		var v1 = new { A, Value = null };
	}
}
