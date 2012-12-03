
// Tests variable type inference with the var keyword when using the foreach statement with an array
using System;
using System.Collections;

public class Test
{
	public static int Main ()
	{
		string [] strings = new string [] { "Foo", "Bar", "Baz" };
		foreach (var item in strings)
			if (item.GetType() != typeof (string))
				return 1;
		
		int [] ints = new int [] { 2, 4, 8, 16, 42 };
		foreach (var item in ints)
			if (item.GetType() != typeof (int))
				return 2;
		
		return 0;
	}
}
