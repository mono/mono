using System;
using System.Linq;

class C
{
	public static void Main ()
	{
		string[] values = new string [] { "1" };
		string[] values2 = new string [] { "Z" };
		
		var q = values.Select (l => l).Select (all =>
			(from b in values2
			let t = "".Any(_ => b == "a")
			select t));
		
		foreach (var e in q)
		{
		}
	}
}

