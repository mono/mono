using System;
using System.Collections.Generic;

public abstract class BaseDataObjectFactory
{
	protected static T GetBusinessQueryObjectFromReader<T> ()
		where T : BusinessQueryObject, new ()
	{
		T t = new T ();
		return t;
	}

	public abstract T [] GetQueryObjects<T> (string query)
		where T : BusinessQueryObject, new ();
}

public class BusinessQueryObject
{
}

public class MySqlDataObjectFactory : BaseDataObjectFactory
{
	public override T [] GetQueryObjects<T> (string query)
	{
		List<T> list = new List<T> ();
		list.Add (GetBusinessQueryObjectFromReader<T> ());
		ExecuteReader(5,
			delegate() {
				list.Add(GetBusinessQueryObjectFromReader<T>());
			});
		return list.ToArray ();
	}

	static void ExecuteReader (int a, PerformActionWithReader action)
	{
	}

	delegate void PerformActionWithReader ();
}

public class C
{
	public static void Main ()
	{
	}
}

