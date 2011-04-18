// CS1654: Cannot assign to members of `q' because it is a `foreach iteration variable'
// Line: 22

using System.Collections;

struct P {
	public int x;
}

struct Q {
	public P p;
}

class Test {
	static IEnumerable foo () { return null; }

	static void Main ()
	{
		IEnumerable f = foo ();
		if (f != null)
			foreach (Q q in f)
				q.p.x = 0;
	}
}
