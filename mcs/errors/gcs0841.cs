// CS0841: The variable `v' cannot be used in an initializer because it refers to itself
// Line: 9
// Compiler options: -langversion:linq

public class Test
{
	static void Main ()
	{
		var v = ++v;
	}
}

