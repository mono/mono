// cs1686.cs: Can not take the address of a local variable
// Line:
// Compiler options: -unsafe

unsafe class X {
	delegate void T ();

	static void Main ()
	{
		int i;

		unsafe {
			T t = delegate {
				int *j = &i;
			};
		}
	}
}




		
