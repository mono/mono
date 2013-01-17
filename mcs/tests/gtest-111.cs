using System;

public struct KeyValuePair<K,V>
{
	public K key;
	public V value;

	public KeyValuePair(K k, V v) { key = k; value = v; }

	public KeyValuePair(K k) { key = k; value = default(V); }
}

public class Collection<T>
{
	public readonly T Item;

	public Collection (T item)
	{
		this.Item = item;
	}

	public void Find (ref T item)
	{
		item = Item;
	}
}

class X
{
	public static int Main ()
	{
		KeyValuePair<int,long> p = new KeyValuePair<int,long> (3);
		KeyValuePair<int,long> q = new KeyValuePair<int,long> (5, 9);

		Collection<KeyValuePair<int,long>> c = new Collection<KeyValuePair<int,long>> (q);
		c.Find (ref p);

		if (p.key != 5)
			return 1;
		if (p.value != 9)
			return 2;

		return 0;
	}
}
