// CS1944: An expression tree cannot contain an unsafe pointer operation
// Line: 14
// Compiler options: -unsafe

using System;
using System.Linq.Expressions;

class C
{
	unsafe delegate byte* D (int*[] d);
	public static void Main ()
	{
		unsafe {
			Expression<D> e6 = (p) => (byte*)p [10];
		}
	}
}
