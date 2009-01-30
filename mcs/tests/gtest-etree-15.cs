using System;
using System.Linq.Expressions;
using System.Collections.Generic;

static public class Foo
{
	static public int Helper (Expression<Predicate<int>> match)
	{
		return 0;
	}

	static public void Main ()
	{
		Expression<Action<List<int>>> exp = x => x.Add (Helper (i => true));
		exp.Compile () (new List<int> { 1 });
	}
}
