//
// Parameter and return value compilation tests for anonymous methods
//
delegate void D (int x);
delegate void E (out int x);

delegate void F (params int[] x);

class X {
	public static int Main ()
	{
		// This should create an AnonymousMethod with the implicit argument
		D d1 = delegate {};
		D d2 = delegate (int a) {};

		F f1 = delegate {};
		F f2 = delegate (int[] a) {};

		return 0;
	}
}
