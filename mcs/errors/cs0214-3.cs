// Compiler options: -unsafe

struct X {
	static unsafe void *a ()
		{
			return null;
		}

	static void Main ()
		{
			a ();
		}
	
}
