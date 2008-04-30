// CS1945: An expression tree cannot contain an anonymous method expression
// Line: 11

using System;
using System.Linq.Expressions;

class C
{
	public static void Main ()
	{
		Expression<Func<Func<int>>> e = () => delegate () { return 1; };		
	}
}
