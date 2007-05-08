// Compiler options: -langversion:linq
// Tests anonymous type consolidation
using System;
using System.Collections;

public class Test
{
	static int Main ()
	{
		var v1 = new { Name = "Scott", Age = 21 };
		var v2 = new { Age = 20, Name = "Sam" };
		
		if (v1.GetType () != v2.GetType ())
			return 1;
		
		return 0;
	}
}
