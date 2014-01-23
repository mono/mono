
// Tests variable type inference with the var keyword when using the foreach statement with generic collections
using System;
using System.Collections.Generic;

public class Test
{
	public static int Main ()
	{
		string[] strings = new string[] { "Foo", "Bar", "Baz" };
		
		foreach (var v in strings)
			if (v.GetType () != typeof (string))
				return 1;
			
		Dictionary<int, string> dict = new Dictionary<int, string> ();
		dict.Add (0, "boo");
		dict.Add (1, "far");
		dict.Add (2, "faz");
		
		foreach (var v in dict)
			if (v.GetType () != typeof (KeyValuePair<int, string>))
				return 2;
		
		return 0;
	}
}
