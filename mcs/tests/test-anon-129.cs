using System.Collections.Generic;

class Test
{
	delegate T Creator<T> ();

	static bool TryAction<T> (Creator<T> creator, out T output)
	{
		output = default (T);
		return false;
	}

	static bool Func1<T> (IList<T> list, bool arg, out T value) where T : new ()
	{
		return TryAction<T> (delegate { return Item (list); }, out value);
	}

	public static T Item<T> (IList<T> list)
	{
		return GetSingleItem<T> (list);
	}

	public static T GetSingleItem<T> (IList<T> list)
	{
		return default (T);
	}

	public static void Main ()
	{
		Test value;
		Func1 (new List<Test> (), false, out value);
	}
}
