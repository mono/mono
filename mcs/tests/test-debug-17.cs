using System;
using System.Linq.Expressions;

class C
{
	public static void Main ()
	{
	}
	
	void Test_1 ()
	{
		Expression<Func<int, int, int>> e = 
			(a, b) =>
			a + b;
		
		return;
	}
	
	void Test_2 ()
	{
		Expression<Func<Expression<Func<int>>>> e = () => () => 2;
	}
}