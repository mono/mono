// CS8030: Anonymous function or lambda expression converted to a void returning delegate cannot return a value
// Line: 9

delegate void D (int x);

class X {
	static void Main ()
	{
		D d6 = delegate (int x) { return x; }; // Return type mismatch.
	}
}
