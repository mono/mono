using System;
using System.Linq.Expressions;

class C
{
	public static void Main ()
	{
		Expression<Func<dynamic, int, dynamic>> e = (dynamic da, int xa) => true ? da : xa;
		string s = e.Compile () ("in", 1);
	}
}
