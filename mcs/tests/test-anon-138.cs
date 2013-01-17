using System;
using System.Collections.Generic;
using System.Linq.Expressions;

class Foo<T>
{
	public bool ContainsAll<U> (IEnumerable<U> items) where U : T
	{
		Func<bool> d = delegate () {
			foreach (U item in items) {
				Expression<Func<bool>> e = () => !Contains (item);
				if (!e.Compile () ())
					return false;
			}
			
			return true;
		};

		return d ();
	}

	public bool Contains (T t)
	{
		return false;
	}
}

class Program
{
	public static int Main ()
	{
		var x = new Foo<int> ();
		return x.ContainsAll (new [] { 4, 6, 78 }) ? 0 : 1;
	}
}

