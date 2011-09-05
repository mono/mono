using System;
using System.Collections.Generic;

class HS<T>
{
	public HS (IEqualityComparer<T> comparer)
	{
	}
}

class Test
{
	static void Foo<T> (IEqualityComparer<T> c)
	{
		Func<HS<T>> a = () => {
			return new HS<T> (c);
		};
	}
	
	public static int Main ()
	{
		Foo<object> (null);
		return 0;
	}
}