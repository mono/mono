// CS8198: An expression tree cannot contain out variable declaration
// Line: 11

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