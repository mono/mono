// CS8188: An expression tree cannot not contain a throw expression
// Line: 11

using System;
using System.Linq.Expressions;

class C
{
	public static void Main ()
	{
		Expression<Func<object>> l = () => throw null;
	}
}