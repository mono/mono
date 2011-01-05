// CS0832: An expression tree cannot contain an assignment operator
// Line: 11

using System;
using System.Linq.Expressions;

class C
{
	public static void Main ()
	{
		Expression<Func<int, int>> e = (a) => --a;
	}
}
