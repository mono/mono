using System;

delegate int D (int arg);

class X {

	public static int Main ()
	{
		D x = T (1);

		int v = x (10);
		Console.WriteLine ("Should be 11={0}", v);
		return v == 11 ? 0 : 1;
	}

	static D T (int a)
	{
		D d = delegate (int arg) {
			return arg + a;
		};

		return d;
	}
}
