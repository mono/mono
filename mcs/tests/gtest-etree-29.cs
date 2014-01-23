using System.Linq.Expressions;
using System;

class C
{
	public static int Main ()
	{
		Expression<Func<bool, IA>> e = (arg) => arg ? new B2 () : (IA) new B1 ();
		var cond = (ConditionalExpression) e.Body;
		if (cond.NodeType != ExpressionType.Conditional)
			return 1;
		if (cond.IfTrue.NodeType != ExpressionType.Convert)
			return 2;
		if (cond.IfFalse.NodeType != ExpressionType.Convert)
			return 3;

		e.Compile () (true);
		return 0;
	}
}

interface IA
{
}

class B2 : IA
{
}

class B1 : IA
{
}
