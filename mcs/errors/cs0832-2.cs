// CS0832: An expression tree cannot contain an assignment operator
// Line: 13

using System;
using System.Linq.Expressions;

class C
{
	delegate void D (string s);
	
	public static void Main ()
	{
		Expression<D> e = (a) => a = "a";
	}
}
