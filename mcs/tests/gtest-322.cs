public class MyBase<K, V>
{
	public delegate void Callback (K key, V value);
    
	public MyBase (Callback insertionCallback)
	{ }
}

public class X : MyBase<string, int>
{
	public X (Callback cb)
		: base (cb)
	{ }

	public static void Main ()
	{ }
}
