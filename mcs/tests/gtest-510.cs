interface IA<T>
{
}

class CA<U, V>
{
}

public class Map<K, T> : IA<CA<K, IA<T>>>, IA<T>
{
}

class S
{
	public static void Main ()
	{
		new Map<double, short> ();
	}
}