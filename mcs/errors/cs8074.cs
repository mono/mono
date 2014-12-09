// CS8074: Expression tree cannot contain a dictionary initializer
// Line: 13

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

class C
{
	public static void Main ()
	{
		Expression<Func<Dictionary<string, int>>> l = () => new Dictionary<string, int> {
			["a"] = 1
		};
	}
}