// CS1944: An expression tree cannot contain an unsafe pointer operation
// Line: 15
// Compiler options: -unsafe

using System;
using System.Linq.Expressions;

class C
{
	unsafe delegate int* D (int i);
	
	public static void Main ()
	{
		unsafe {
			Expression<D> e = (int p) => &p;
		}
	}
}
