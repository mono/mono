// CS0838: An expression tree cannot contain a multidimensional array initializer
// Line: 11

using System;
using System.Linq.Expressions;

class C
{
	void Foo ()
	{
		Expression<Func<char [,]>> e = () => new char [,] { { 'x', 'y' }, { 'a', 'b' }};
	}
}
