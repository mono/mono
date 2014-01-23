// CS0246: The type or namespace name `T' could not be found. Are you missing an assembly reference?
// Line: 13

using System;
using System.Collections.Generic;

class X
{
	public static void Main ()
	{
		Foo (() => {
			IEnumerable<object> f = null;
			foreach (KeyValuePair<int, T> e in f) {
			}
		});

	}

	static void Foo (Action a)
	{
	}
}