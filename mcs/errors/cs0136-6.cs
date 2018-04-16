// CS0136: A local variable named `s' cannot be declared in this scope because it would give a different meaning to `s', which is already used in a `parent or current' scope to denote something else
// Line: 11

using System;

class X
{
	void Test2 (object o)
	{
		if (o is ValueType s) {
			if (o is long s) {
				Console.WriteLine (s);
			}
		}
	}
}