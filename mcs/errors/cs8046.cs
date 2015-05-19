// CS8046: An expression tree cannot contain a declaration expression
// Line: 11
// Compiler options: -langversion:experimental

using System;
using System.Linq.Expressions;

class C
{
	static void Main()
	{
		Expression<Func<bool>> e = () => Out (out int x);
	}

	static bool Out (out int value)
	{
		value = 3;
		return true;
	}
}