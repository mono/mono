// CS0822: An implicitly typed local variable cannot be a constant
// Line: 9
// Compiler options: -langversion:linq

public class Test
{
	static void Main ()
	{
		const var v = 0;
	}
}

