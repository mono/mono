//
// Tests whether we implement the correct methods from interfaces
//

using System;

interface IA {
	void Draw ();
}

interface IB {
	void Draw ();
}

class X : IA, IB {
	public bool ia_called;
	public bool ib_called;
	
	void IA.Draw ()
	{
		ia_called = true;
	}

	void IB.Draw ()
	{
		ib_called = true;
	}
}

class test {

	public static int Main ()
	{
		X x = new X ();

		((IA) x).Draw ();
		Console.WriteLine ("IA: " + x.ia_called);
		Console.WriteLine ("IB: " + x.ib_called);

		if (x.ib_called)
			return 1;
		if (!x.ia_called)
			return 2;

		X y = new X ();
		((IB) y).Draw ();
		Console.WriteLine ("IA: " + x.ia_called);
		Console.WriteLine ("IB: " + x.ib_called);

		if (!y.ib_called)
			return 3;
		if (y.ia_called)
			return 4;

		Console.WriteLine ("All tests pass");
		return 0;
	}
}

	
