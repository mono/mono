public class MyList<T>
{
	public class Helper<U, V> { }

	public Helper<U, V> GetHelper<U, V> ()
	{
		return null;
	}
}

class C
{
	public static int Main ()
	{
		new MyList<int> ().GetHelper<string, bool> ();
		return 0;
	}
}
