// CS0030: Cannot convert type `T[,]' to `System.Collections.Generic.IEnumerable<T>'
// Line: 10

using System.Collections.Generic;

class C
{
	IEnumerable<T> Foo<T> (T [,] a)
	{
		return (IEnumerable<T>) a;
	}
}
