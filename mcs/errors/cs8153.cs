// CS8153: An expression tree lambda cannot contain a call to a method, property, or indexer that returns by reference
// Line: 11

using System;
using System.Linq.Expressions;

class X
{
	void Foo ()
	{
		Expression<Func<int>> e = () => Test (ref this[0]);
	}

	static int Test (ref int y)
	{
		return y;
	}

	ref int this [int y] {
		get {
			throw null;
		}
	}
}
