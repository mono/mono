using System;

delegate int D ();

class X {

	public static void Main ()
	{
		D x = T (1);

		Console.WriteLine ("Should be 2={0}", x ());
	}

	static D T (int a)
	{
		D d = delegate {
			a = a + 1;
			return a;
		};

		return d;
	}
}
