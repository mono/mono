// CS0037: Cannot convert null to `int' because it is a value type
// Line: 9
// Compiler options: -langversion:linq

public class Test
{
	static void Main ()
	{
		var e = new[] { 1, null };
	}
}

