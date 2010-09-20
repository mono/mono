using System.Collections;
class Foo {
	static public IEnumerable foo ()
	{
		try { yield break; } catch { } finally { }
	}
	static int Main ()
	{
		int i = 0;
		foreach (object o in foo ())
			++i;
		return i;
	}
}
