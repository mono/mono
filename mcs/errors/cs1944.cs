// CS1944: An expression tree cannot contain an unsafe pointer operation
// Line: 14
// Compiler options: -unsafe

using System;
using System.Linq.Expressions;

class C
{
	public static void Main ()
	{
		unsafe {
			int*[] p = null;
			Expression<Func<int>> e6 = () => (int)p [10];
		}
	}
}
