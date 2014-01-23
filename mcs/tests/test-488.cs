using System.Collections;

class P {
	public int x;
}

struct Q {
	public P p;
	public Q (P p) { this.p = p; }
}

class Test {
	static IEnumerable foo () { return null; }

	public static void Main ()
	{
		IEnumerable f = foo ();
		if (f != null)
			foreach (P p in f)
				p.x = 0;
		if (f != null)
			foreach (Q q in f)
				q.p.x = 0;
	}
}
