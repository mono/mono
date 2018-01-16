// CS8207: An expression tree cannot contain a discard
// Line: 11

using System;
using System.Linq.Expressions;

class X
{
	void Test ()
	{
		Expression<Func<bool>> e = () => TryGetValue (out _);
	}

	bool TryGetValue (out int arg)
	{
		arg = 3;
		return true;
	}
}
