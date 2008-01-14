// CS1951: An expression tree parameter cannot use `ref' or `out' modifier
// Line: 13

using System;
using System.Linq.Expressions;

class C
{
	delegate int D (out int a);
	
	public static void Main ()
	{
		Expression<D> e = (out int a) => a = 0;
	}
}
