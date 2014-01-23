using System;

abstract class Base
{
	internal static T EndExecute<T> (object source, string method) where T : Base
	{
		return null;
	}
}

class Derived : Base
{
	internal static Derived EndExecute<TElement> (object source)
	{
		return null;
	}
}

class a
{
	public static int Main ()
	{
		Derived.EndExecute<Derived> (null, "something");
		return 0;
	}
}
