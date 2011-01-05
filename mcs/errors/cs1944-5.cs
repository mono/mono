// CS1944: An expression tree cannot contain an unsafe pointer operation
// Line: 13
// Compiler options: -unsafe

using System;
using System.Linq.Expressions;

class C
{
	public static void Main ()
	{
		unsafe {
			Expression<Func<int>> e = () => sizeof (long*);
		}
	}
}
