//
// Tests unary mutator operators on captured variables
//
using System;

class X {
	delegate void D ();

	static int gt, gj;
    
	public static int Main ()
	{
		int times = 0;
		
		D d = delegate {
		    int t = times++;
		    int j = ++times;

		    gt = t;
		    gj = j;
		};
		d ();

		if (gt != 0)
			return 1;
		if (gj != 2)
			return 2;

		return 0;
	}
}
