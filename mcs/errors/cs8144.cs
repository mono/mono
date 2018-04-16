// CS8144:
// Line: 12

using System;
using System.Linq.Expressions;

class X
{
	public static void Main ()
	{
		(byte b, short d) t = (0, 0);
		Expression<Func<(int a, int aa)>> e = () => t;
	}
}
