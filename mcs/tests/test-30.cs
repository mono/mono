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

	static int Main ()
	{
		X x = new X ();

		((IA) x).Draw ();
		Console.WriteLine ("IA: " + x.ia_called);
		Console.WriteLine ("IB: " + x.ib_called);

		if (x.ib_called)
			return 1;
		if (x.ia_called)
			return 0;

		return 1;
	}
}

	
