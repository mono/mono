using System;

public class Foo
{
	static void GenericLock<T> (T t) where T : class
	{
		lock (t)
		{
		}
	}
	
	public static void Main ()
	{
		GenericLock ("s");
	}
}