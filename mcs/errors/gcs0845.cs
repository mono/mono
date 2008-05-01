// CS0845: An expression tree cannot contain a coalescing operator with null left side
// Line: 11

using System;
using System.Linq.Expressions;

class C
{
	public static void Main ()
	{
		Expression<Func<bool?, bool?>> e = (a) => null ?? a;
	}
}