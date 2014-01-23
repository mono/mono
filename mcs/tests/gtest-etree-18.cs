using System;
using System.Linq.Expressions;

public class FooBase { }
public class Foo : FooBase { }

public interface IHelper
{
	void DoIt (FooBase foo);
}

public class Program
{
	public static int Main ()
	{
		Expression<Action<IHelper>> e = (helper => helper.DoIt (new Foo ()));
		var mce = e.Body as MethodCallExpression;
		var et = mce.Arguments[0].NodeType;

		Console.WriteLine (et);
		if (et != ExpressionType.New)
			return 1;

		return 0;
	}
}
