// CS1686: Local variable or parameter `i' cannot have their address taken and be used inside an anonymous method, lambda expression or query expression
// Line: 16
// Compiler options: -unsafe

class X {
	delegate void S ();

	unsafe void M ()
	{
		int i;
		int * j ;

		S s = delegate {
			i = 1;
		};
		j = &i;
	}

	static void Main () {}
}
