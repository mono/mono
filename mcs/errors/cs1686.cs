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
