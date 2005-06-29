// cs0255.cs: Cannot use stackalloc in finally or catch
// Line: 12
// Compiler options: -unsafe

unsafe class X {

	static void Main ()
	{
		try {
		} catch {
			char *ptr = stackalloc char [10];
		}
	}
}	
