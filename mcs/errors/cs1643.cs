// CS1643: Not all code paths return a value in anonymous method of type `X.T'
// Line: 12
using System;

class X {
	delegate int T ();

	static void Main ()
	{
		int a = 1;
		
		T t = delegate {
			if (a == 1)
				return 1;
		};
	}
}
