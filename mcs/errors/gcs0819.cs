// CS0819: An implicitly typed local variable declaration cannot include multiple declarators
// Line: 9
// Compiler options: -langversion:linq

public class Test
{
	static void Main ()
	{
		var v = "foo", w = "bar";
	}
}
