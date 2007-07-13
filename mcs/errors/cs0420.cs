// CS0420: `X.j': A volatile field references will not be treated as volatile
// Line: 10
// Compiler options: -unsafe /warnaserror /warn:1

unsafe class X {
	static volatile int j;
	
	static void Main ()
	{
		fixed (int *p = &j){
			
		}
	}
}
