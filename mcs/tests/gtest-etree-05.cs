using System;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;

class C
{
	static void AssertNodeType (LambdaExpression e, ExpressionType et)
	{
		if (e.Body.NodeType != et)
			throw new ApplicationException (e.Body.NodeType + " != " + et);
	}

	static void Assert<T> (T expected, T value)
	{
		if (!EqualityComparer<T>.Default.Equals (expected, value)) {
			throw new ApplicationException (expected + " != " + value);
		}
	}
	
	public static int Main()
	{
		// It also tests constant boxing
		Expression<Func<ArrayList>> e1 = () => new ArrayList { null, "Hello", "World", 5 };
		AssertNodeType (e1, ExpressionType.ListInit);
		/* Verification exception on .NET */
		var re1 = e1.Compile ().Invoke ();
		
		Assert (null, re1 [0]);
		Assert ("Hello", re1 [1]);
		Assert ("World", re1 [2]);
		Assert (5, re1 [3]);
		
		Console.WriteLine ("OK");
		return 0;
	}
}
