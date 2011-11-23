// CS0815: An implicitly typed local variable declaration cannot be initialized with `void'
// Line: 17

using System;
using System.Collections.Generic;

class A
{
	static void Test (Action a)
	{
	}

	public static void Main ()
	{
		Test (() => {
			List<string> l = null;
			var res = l.ForEach (g => { });
		});
	}
}
