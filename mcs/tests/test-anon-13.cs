using System;

delegate void D ();

class X {
	public static void Main ()
	{
		X x = new X (1);
		X y = new X (100);
		D a = x.T ();
		D b = y.T ();

		a ();
		b ();
	}

	X (int start)
	{
		ins = start;
	}

	int ins;

	D T ()
	{
		D d = delegate () {
			Console.WriteLine ("My state is: " + CALL ());
		};

		return d;
	}
	string CALL ()
	{
		return "GOOD";
	}

}
