//
// Tests whether we implement the correct methods from interfaces
//
interface IA {
	void Draw ();
}

interface IB {
	void Draw ();
}

class X : IA, IB {
	bool ia_called;
	bool ib_called;
	
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
	}
}

	
