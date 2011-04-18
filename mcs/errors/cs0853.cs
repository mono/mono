// CS0853: An expression tree cannot contain named argument
// Line: 15

using System;
using System.Linq.Expressions;

class M
{
	static void Named (int i)
	{
	}
	
	public static void Main ()
	{
		Expression<Action> e = () => Named (i : 1);
	}
}
