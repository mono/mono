

using System;
using System.Collections;

public class Test
{
	public static int Main ()
	{
		var v1 = new {  };
		var v2 = new {  };
		
		if (v1.GetType () != v2.GetType ())
			return 1;
			
		if (!v1.Equals (v2))
			return 2;
			
		Console.WriteLine (v1);
		Console.WriteLine (v2);
		return 0;
	}
}

