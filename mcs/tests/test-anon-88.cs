using System;

class C
{
	public delegate void D ();

	public static int Main ()
	{
		new C ().Test ();
		return 0;
	}

	void Test ()
	{
		int l1 = 0;

		if (l1 == 0) {
			int l2 = 1;
			if (l2 == 1) {
				D dd = delegate {
					int l3 = 2;
					D d2 = delegate { int x = l1; int z = l3; };
					D d22 = delegate { int x = 1; };
				};
			}

			D d3 = delegate { int y = l2; };
		}

		D d1 = delegate { int x = l1; };
	}
}
