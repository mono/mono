// CS0121: The call is ambiguous between the following methods or properties: `Test.Foo<int,int>(int, System.Linq.Expressions.Expression<System.Func<int,int>>)' and `Test.Foo<int,int>(int, System.Func<int,int>)'
// Line: 22

using System;
using System.Linq;
using System.Linq.Expressions;

class Test
{
	static int Foo<T, R> (T t, Expression<Func<T, R>> e)
	{
		return 5;
	}
	
	static int Foo<T, R> (T t, Func<T, R> e)
	{
		return 0;
	}

	static void Main ()
	{
		Foo (1, i => i);
	}
}

