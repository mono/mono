using System;

class Test226
{
	static bool ok;

	public static void Test ()
	{
		int n=0;
		while (true) {
			if (++n==5)
				break;
			switch (0) {
			case 0: break;
			}
		}
		ok = true;
	}

	public static int Main ()
	{
		Test ();
		return ok ? 0 : 1;
	}
}
