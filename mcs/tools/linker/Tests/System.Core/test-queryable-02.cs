using System;
using System.Linq.Expressions;
using System.Linq;

public class QueryableUsedViaExpression {
	public static void Main ()
	{
		var q = "Test".AsQueryable ();
		var count = CallQueryableCount (q);
		Console.WriteLine ($"count: {count}");
	}

	public static int CallQueryableCount (IQueryable source)
	{
		return source.Provider.Execute<int> (
			Expression.Call (
				typeof (Queryable), "Count",
				new Type [] { source.ElementType }, source.Expression));
	}
}