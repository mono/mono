public struct KeyValuePair<X,Y>
{
	public KeyValuePair (X x, Y y)
	{ }
}

public interface IComparer<T>
{
	int Compare (T x);
}

public class KeyValuePairComparer<K,V> : IComparer<KeyValuePair<K,V>>
{
	public int Compare (KeyValuePair<K,V> a)
	{
		return 0;
	}
}

public class TreeBag<T>
{
	IComparer<T> comparer;
	T item;

	public TreeBag (IComparer<T> comparer, T item)
	{
		this.comparer = comparer;
		this.item = item;
	}

	public int Find ()
	{
		return comparer.Compare (item);
	}
}

public class X
{
	public static void Main ()
	{
		KeyValuePair<int,int> pair = new KeyValuePair<int,int> (3, 89);
		KeyValuePairComparer<int,int> comparer = new KeyValuePairComparer<int,int> ();
		TreeBag<KeyValuePair<int,int>> bag = new TreeBag<KeyValuePair<int,int>> (comparer, pair);
		bag.Find ();
	}
}
