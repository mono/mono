// CS1920: An element initializer cannot be empty
// Line: 11


using System.Collections.Generic;

public class Test
{
	static void Main ()
	{
		var d = new Dictionary <string, int> { { } };
	}
}
