//
// Test for bug reported on the mono-devel-list 
//
using System;

namespace BadRefTest
{

public class CtorInc
{
	static int x, y;

	static int IncByRef(ref int i) { return ++i; }

	public CtorInc() { IncByRef(ref x); ++y; }

	public static bool Results(int total)
	{
		Console.WriteLine("CtorInc test {0}: x == {1}, y == {2}",
				x == y && x == total? "passed": "failed", x, y);

		return x == y && x == total;
	}
}

public class Runner
{
	public static int Main()
	{
		int i = 0;
		for (; i < 5; i++)
		{
			CtorInc t = new CtorInc();
		}
		return CtorInc.Results(i) ? 0 : 1;
	}

}
}

