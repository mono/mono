// CS1656: Cannot assign to `i' because it is a `foreach iteration variable'
// Line: 14

using System.Collections;

class Test {
	static IEnumerable foo () { return null; }

	static void Main ()
	{
		IEnumerable f = foo ();
		if (f != null)
			foreach (int i in f)
				i = 0;
	}
}
