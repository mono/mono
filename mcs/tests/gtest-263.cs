using System;
using System.Collections.Generic;

class Foo<S>
{
	public ICloneable Test (S t)
	{
		return (ICloneable) t;
	}
}

public static class ConvertHelper
{
	public static IEnumerator<T> Test<S,T> (S s)
		where T : S
	{
		yield return (T) s;
	}

	public static void Main ()
	{ }
}
