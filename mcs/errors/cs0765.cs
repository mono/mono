// CS0765: Partial methods with only a defining declaration or removed conditional methods cannot be used in an expression tree
// Line: 15

using System;
using System.Diagnostics;
using System.Linq.Expressions;

public class C
{
	[Conditional ("DEBUG")]
	public static void TestMethod () { }

	static void Main ()
	{
		Expression<Action> e = () => TestMethod ();
	}
}
