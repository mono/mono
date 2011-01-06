// CS0826: The type of an implicitly typed array cannot be inferred from the initializer. Try specifying array type explicitly
// Line: 13


public class Test
{
	static void TestMethod ()
	{
	}
	
	static void Main ()
	{
		var e = new[] { TestMethod () };
	}
}

