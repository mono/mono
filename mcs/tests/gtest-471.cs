class Q
{
	static void bar<T> (out T t) where T : struct
	{
		t = true ? new T () : new T ();
	}

	public static int Main ()
	{
		int d = 0;
		bar (out d);
		return 0;
	}
}   
