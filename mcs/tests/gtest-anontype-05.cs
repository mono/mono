
// Tests anonymous type consolidation

using System;
using System.Collections;

public class Test
{
	static string Null ()
	{
		return null;
	}
	
	public static int Main ()
	{
		var v1 = new { Name = "Scott", Age = 21 };
		var v2 = new { Age = 20, Name = "Sam" };
		var v3 = new { Name = Null (), Age = 33 };
		
		if (v1.GetType () == v2.GetType ())
			return 1;
			
		if (v1.Equals (v2))
			return 2;
			
		if (v1.GetType () != v3.GetType ())
			return 3;
			
		if (!v1.Equals (v1))
			return 4;
					
		if (v1.GetHashCode () != v1.GetHashCode ())
			return 5;
		
		Console.WriteLine (v1);
		Console.WriteLine (v3);
		
		if (v1.ToString () != "{ Name = Scott, Age = 21 }")
			return 6;
			
		if (v3.ToString () != "{ Name = , Age = 33 }")
			return 7;

		var v4 = new {};

		if (v4.ToString () != "{ }")
			return 8;

		var v5 = new { Foo = "Bar" };
		var v6 = new { Foo = Null () };

		if (v5.ToString () != "{ Foo = Bar }")
			return 9;

		if (v6.ToString () != "{ Foo =  }")
			return 10;

		return 0;
	}
}
