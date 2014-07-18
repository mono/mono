// CS0837: The `is' operator cannot be applied to a lambda expression, anonymous method, or method group
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