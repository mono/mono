class G<T>
{
	public T def () { return default (T); }
}

class C
{
	delegate void DF ();
	static DF df;
	static void foo (object o) { }
	static void cf<T> ()
	{
		G<T> g = new G<T> ();
		df = delegate { foo (g.def ()); };
	}

	public static int Main ()
	{
		cf<int> ();
		df ();
		return 0;
	}
}
