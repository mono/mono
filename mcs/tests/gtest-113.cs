using System;

public delegate V Mapper<T,V> (T item);

public class List<T>
{
	public void Map<V> (Mapper<T,V> mapper)
	{ }
}

class X
{
	public static void Main ()
	{
		List<int> list = new List<int> ();
		list.Map (new Mapper<int,double> (delegate (int i) { return i/10.0; }));
	}
}

