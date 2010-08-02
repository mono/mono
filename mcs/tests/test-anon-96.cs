using System;

class P
{
	public int A;
}

static class Program
{
	static int Extra () { return 36; }

	delegate int D ();

	static D Get (int dummy)
	{
		var p = new P { A = 6 };
		switch (dummy) {
		case 0:
			int extra = Extra ();
			return () => p.A + extra;
		case 1:
			extra = 9;
			return () => p.A * extra;
		case 2:
			return () => p.A * 2;
		}
		throw new NotSupportedException ();
	}

	static int Run (int i)
	{
		return Get (i) ();
	}

	static int Main ()
	{
		if (Run (0) != 42)
			return 1;

		if (Run (1) != 54)
			return 2;

		if (Run (2) != 12)
			return 3;

		if (Run (1) != 54)
			return 4;

		if (Run (0) != 42)
			return 5;

		return 0;
	}
}
