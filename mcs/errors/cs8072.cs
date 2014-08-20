// CS8072: An expression tree cannot contain a null propagating operator
// Line: 11

using System;
using System.Linq.Expressions;

class C
{
	static int Main ()
	{
		Expression<Func<string, char?>> e = l => l?[1];
		return 0;
	}
}