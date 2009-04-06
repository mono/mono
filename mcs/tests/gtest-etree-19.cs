using System;
using System.Linq.Expressions;

public class Test
{
	public static int Main ()
	{
		if (Value<int> () != 0)
			return 1;

		var v = Value<Test> ();
		return 0;
	}

	static T Value<T> () where T : new ()
	{
		Expression<Func<T>> e = () => new T ();
		var et = e.Body.NodeType;
		if (et != ExpressionType.New)
			throw new ApplicationException (et.ToString ());

		return e.Compile ().Invoke ();
	}
}
