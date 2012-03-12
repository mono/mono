// CS1989: Async lambda expressions cannot be converted to expression trees
// Line: 17

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

class C
{
	static Task Method ()
	{
		return null;
	}
	
	public static void Main ()
	{
		Expression<Action<int>> a = async l => await Method ();
	}
}
