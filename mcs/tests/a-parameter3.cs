// Compiler options: -unsafe
using System;

delegate int D ();

unsafe class X {

	static int Main ()
	{
		D x = T (1);

		int v = x ();
		Console.WriteLine ("Should be 2={0}", v);
		return v == 2 ? 0 : 1;
	}

	static D T (int a)
	{
		D d = delegate {
			int *x = &a;

			*x = *x + 1;
			return *x;
		};

		return d;
	}
}
