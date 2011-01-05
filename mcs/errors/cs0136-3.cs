// CS0136: A local variable named `i' cannot be declared in this scope because it would give a different meaning to `i', which is already used in a `parent or current' scope to denote something else
// Line: 10

using System;

class T
{
	public void Foo (int i)
	{
		Action<int> v = x => { int i = 9; };
	}
}
