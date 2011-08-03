// CS1654: Cannot assign to members of `p' because it is a `foreach iteration variable'
// Line: 18

using System.Collections;

struct P {
	public int x;
}

class Test {
	static IEnumerable foo () { return null; }

	static void Main ()
	{
		IEnumerable f = foo ();
		if (f != null)
			foreach (P p in f)
				p.x = 0;
	}
}
