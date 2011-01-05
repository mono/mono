// CS0029: Cannot implicitly convert type `anonymous type' to `bool'
// Line: 10


public class Test
{
	static void Main ()
	{
		var o = new { Value = 1 };
		bool b = o;
	}
}
