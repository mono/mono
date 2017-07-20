// CS8143: An expression tree cannot contain a tuple literal
// Line: 11

using System;
using System.Linq.Expressions;

class C
{
	public static void Main ()
	{
		Expression<Func<object>> l = () => (-1, 2);
	}
}