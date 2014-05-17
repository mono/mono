// CS1654: Cannot assign to members of `p' because it is a `foreach iteration variable'
// Line: 14

using System.Collections;

struct P {
	public int x { get; set; }
}

class Test {
	static void Foo (IEnumerable f)
	{
		foreach (P p in f)
			p.x += 2;
	}
}
