// Compiler options: -langversion:linq

using System;
using System.Linq;

class NestedQuery
{
	public static int Main ()
	{
		var e = from values in new [] { "aba", "bbb", "bab", "aaa" }
			where values.Length > 0
			select from type in values
				where type == 'a'
				select type;
		
		int counter = 0;
		foreach (var v in e)
			foreach (var vv in v) {
				++counter;
				Console.WriteLine (vv);
			}
			
		return counter == 6 ? 0 : 1;
	}
}
