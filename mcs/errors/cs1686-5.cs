// CS1686: Local variable or parameter `a' cannot have their address taken and be used inside an anonymous method, lambda expression or query expression
// Line: 11
// Compiler options: -unsafe

delegate void D ();

unsafe class X {
	public D T (int a)
	{
		int *y = &a;
		
		return delegate {
			int x = a;
		};
	}
}
