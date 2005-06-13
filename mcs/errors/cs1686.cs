// cs1686.cs: Variable i is captured in an anonymous method and its address is also being taken: they are exclusive
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
