// CS0854: An expression tree cannot contain an invocation which uses optional parameter
// Line: 15

using System;
using System.Linq.Expressions;

class M
{
	public M (string s = "value")
	{
	}
	
	public static void Main ()
	{
		Expression<Action> e = () => new M ();
	}
}
