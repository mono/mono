namespace A
{
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

		public TreeBag (IComparer<T> comparer)
		{
			this.comparer = comparer;
		}

		public int Find (ref T item)
		{
			return comparer.Compare (item);
		}
	}

	public class X
	{
		public static void Test ()
		{
			KeyValuePair<int,int> pair = new KeyValuePair<int,int> (3, 89);
			KeyValuePairComparer<int,int> comparer = new KeyValuePairComparer<int,int> ();
			TreeBag<KeyValuePair<int,int>> bag = new TreeBag<KeyValuePair<int,int>> (comparer);
			bag.Find (ref pair);
		}
	}
}

namespace B
{
	public class KeyValuePair<X,Y>
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

		public TreeBag (IComparer<T> comparer)
		{
			this.comparer = comparer;
		}

		public int Find (ref T item)
		{
			return comparer.Compare (item);
		}
	}

	public class X
	{
		public static void Test ()
		{
			KeyValuePair<int,int> pair = new KeyValuePair<int,int> (3, 89);
			KeyValuePairComparer<int,int> comparer = new KeyValuePairComparer<int,int> ();
			TreeBag<KeyValuePair<int,int>> bag = new TreeBag<KeyValuePair<int,int>> (comparer);
			bag.Find (ref pair);
		}
	}
}

class X
{
	public static void Main ()
	{
		A.X.Test ();
		B.X.Test ();
	}
}
