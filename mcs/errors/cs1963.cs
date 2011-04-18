// CS1963: An expression tree cannot contain a dynamic operation
// Line: 12

using System;
using System.Linq.Expressions;

class C
{
	public static void Main ()
	{
		dynamic d = 1;
		Expression<Func<int>> e = () => d + 1;
	}
}
