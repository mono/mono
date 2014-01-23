using System;
using System.Linq.Expressions;

public class Program
{
	public static int Main ()
	{
		Expression<Action<IHelper>> e = (helper => helper.DoIt (null));
		var mce = e.Body as MethodCallExpression;
		var et = mce.Arguments[0].NodeType;
		Console.WriteLine (et);
		if (et != ExpressionType.Constant)
			return 1;

		return 0;
	}
}

public class Foo { }

public interface IHelper
{
	void DoIt (Foo foo);
}
