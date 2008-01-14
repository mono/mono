// CS1946: An anonymous method cannot be converted to an expression tree
// Line: 11

using System;
using System.Linq.Expressions;

class C
{
	delegate string D ();

	public static void Main ()
	{
		Expression<D> e = delegate () { return "a"; };
	}
}
