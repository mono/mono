// CS1655: Cannot pass members of `q' as ref or out arguments because it is a `foreach iteration variable'
// Line: 23

using System.Collections;

struct P {
	public int x;
}

struct Q {
	public P p;
}

class Test {
	static void bar (out int x) { x = 0; }
	static IEnumerable foo () { return null; }

	static void Main ()
	{
		IEnumerable f = foo ();
		if (f != null)
			foreach (Q q in f)
				bar (out q.p.x);
	}
}
