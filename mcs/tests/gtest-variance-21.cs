interface I<out T1, out T2, out T3>
{
}

class C
{
	public static bool M<T> ()
	{
		I<T, string, bool> a = null;
		I<T, object, bool> b = a;
		return a == b;
	}
	
	public static int Main ()
	{
		I<object, string, dynamic[]> a = null;
		I<object, object, object[]> b = a;
		
		I<dynamic, string, long> a2 = null;
		I<object, object, long> b2 = a2;

		M<uint> ();
		return 0;
	}
}