

using System;
using System.Collections.Generic;

public class C
{
	static readonly List<int> values = new List<int> { 1, 2, 3 };
	
	public static int Main ()
	{
		if (values.Count != 3)
			return 1;
		
		Console.WriteLine ("OK");
		return 0;
	}
}
