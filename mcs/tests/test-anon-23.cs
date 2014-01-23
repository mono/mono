using System;

delegate void D ();

class X {

	public static int Main ()
	{
		X x = new X();
		x.M ();
		e ();
                Console.WriteLine ("J should be 101= {0}", j);
		if (j != 101)
			return 3;
		Console.WriteLine ("OK");
		return 0;
	}

	static int j;
	static D e;
	
	void M ()
	{
		int l = 100;

		D d = delegate {
			int b;
			b = 1;
			e = delegate {
					j = l + b;
				};
			};
		d ();
	}
	
}
