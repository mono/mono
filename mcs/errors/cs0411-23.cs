// CS0411: The type arguments for method `System.Linq.Enumerable.OrderBy<TSource,TKey>(this System.Collections.Generic.IEnumerable<TSource>, System.Func<TSource,TKey>)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 20

using System.Collections.Generic;
using System.Linq;

public class C
{
	public string Name ()
	{
		return "aa";
	}
}

class Z
{
	void Test ()
	{
		List<C> l = null;
		var r = l.OrderBy (f => f.Name).ToList ();
	}
}