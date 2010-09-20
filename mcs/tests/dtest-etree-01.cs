using System;
using System.Linq.Expressions;

public class C
{
	static void Conv1(Expression<Func<object, object>> l)
	{
	}
	
	static void Conv2(Expression<Func<dynamic, dynamic>> l)
	{
	}
	
	public static void Main ()
	{
		Expression<Func<object>> e1 = () => (dynamic) 1;
		Expression<Func<dynamic>> e2 = () => (object) 1;
		
		Conv1 ((d) => (dynamic) 1);
		Conv1 ((dynamic d) => d);
		
		Conv2 ((o) => (object) 1);
		Conv2 ((object o) => o);
	}
}