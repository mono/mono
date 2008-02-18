

using System.Collections.Generic;

public class C
{
	public static int Main ()
	{
		var o = new Dictionary<string, int>() { { "Foo", 3 } };
		if (o ["Foo"] != 3)
			return 1;
		
		o = new Dictionary<string, int>() { { "A", 1 }, { "B", 2 } };

		return 0;
	}
}

