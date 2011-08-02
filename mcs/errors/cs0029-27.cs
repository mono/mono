// CS0029: Cannot implicitly convert type `T[]' to `System.Collections.Generic.IList<U>'
// Line: 10

using System.Collections.Generic;

class Program
{
	static IList<U> Foo<T, U> (T[] arg) where T : U
	{
		return arg;
	}
}
