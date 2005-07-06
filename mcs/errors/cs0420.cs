// cs0420.cs: `X.j': A volatile fields cannot be passed using a ref or out parameter
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
