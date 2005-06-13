// cs0420.cs: 'X.j': a reference to a volatile field will not be treated as volatile
// Line: 10
// Compiler options: -unsafe /warnaserror

unsafe class X {
	static volatile int j;
	
	static void Main ()
	{
		fixed (int *p = &j){
			
		}
	}
}
