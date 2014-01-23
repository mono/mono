namespace C5
{
	public class HashedArrayList<T>
	{
		public void Test ()
		{
			new HashSet <KeyValuePair<T,int>> (new KeyValuePairHasher<T,int> ());
		}
	}

	public class HashSet<T>
	{
		public HashSet (IHasher<T> itemhasher)
		{ }
	}

	public interface IHasher<T>
	{
	}

	public struct KeyValuePair<K,V>
	{
	}

	public sealed class KeyValuePairHasher<K,V>: IHasher<KeyValuePair<K,V>>
	{
	}
}

class X
{
	public static void Main ()
	{ }
}
