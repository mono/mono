// CS8189: By reference return delegate does not match `C.D()' return type
// Line: 15

class C
{
	delegate ref int D ();

	static int M ()
	{
		return 1;
	}

	static void Main ()
	{
		D d = new D (M);
	}
}