// CS0833: `Value': An anonymous type cannot have multiple properties with the same name
// Line: 9
// Compiler options: -langversion:linq

public class Test
{
	static void Main ()
	{
		var v1 = new { Value = 1, Value = 0 };
	}
}
