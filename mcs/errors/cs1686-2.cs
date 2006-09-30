// cs1686.cs: Local variable `a' or its members cannot have their address taken and be used inside an anonymous method block
// Line: 11
// Compiler options: -unsafe

delegate void D ();

unsafe class X {
	public D T (int a)
	{
		return delegate {
			int *x = &a;
		};
	}

	static void Main ()
	{ }
}
