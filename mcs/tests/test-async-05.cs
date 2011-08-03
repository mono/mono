// Compiler options: -langversion:future

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
		Expression<Action<int>> a = async l => Method ();
		a.Compile () (1);
	}
}
