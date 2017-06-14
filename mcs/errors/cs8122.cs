// CS8122: An expression tree cannot contain a pattern matching operator
// Line: 12

using System;
using System.Linq.Expressions;

class X
{
	public static void Main ()
	{
		object o = 1;
		Expression<Func<bool>> e = () => o is int y;
	}
}