//
// This test just verifies that we generate the proper signature for
// EndInvoke, something that we were not doing before in the presence
// of out parameters

using System;

class Test
{
	delegate int D (int x, out int y);
	
	static int M (int x, out int y)
	{
		y = x + 2;
		return ++x;
	}
	
	public static int Main ()
	{
		int x = 1;
		int y = 0;

		D del = new D (M);
		IAsyncResult ar = del.BeginInvoke (x, out y, null, null);
		if (del.EndInvoke (out y, ar) != 2)
			return 1;
		if (y != 3)
			return 2;

		Console.WriteLine ("Test ok");
		return 0;
	}
}
