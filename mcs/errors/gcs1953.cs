// CS1953: An expression tree cannot contain an expression with method group
// Line: 11

using System;
using System.Linq.Expressions;

class C
{
	public static void Main ()
	{
		Expression<Func<bool>> e = () => "1".ToString is string;
	}
}