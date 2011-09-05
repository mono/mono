using System;

public delegate void Bla ();

public class Driver
{
	static int Main ()
	{
		new Driver().Repro ();
		return 0;
	}

	void P (int a, int b) {}

	void Repro ()
	{ 
		int a = -1;
		int b = 10;

		P (b, a++);

		Bla c = () => P(b, ++a); 

		P (b, a++);
	}

}
