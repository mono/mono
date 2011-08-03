using System;

class MainClass
{
	public static void Main ()
	{
		Test1 (l => l.GetItems ());
		Test2 (l => l.GetItems2 ());
	}

	static T[] Test1<T> (Func<IB, T[]> arg)
	{
		return null;
	}

	static IA<T>[] Test2<T> (Func<IB, IA<T>[]> arg)
	{
		return null;
	}
}

interface IA<U>
{
}

interface IB
{
	string[] GetItems ();
	IA<string>[] GetItems2 ();
}
