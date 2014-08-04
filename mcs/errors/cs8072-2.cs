// CS8072: An expression tree cannot contain a null propagating operator
// Line: 14

using System;
using System.Linq.Expressions;

public class C
{
	public void TestMethod () { }

	static void Main ()
	{
		C c = null;
		Expression<Action> e = () => c?.TestMethod ();
	}
}
