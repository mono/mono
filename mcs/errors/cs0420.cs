// cs0420.cs: `X.j': A volatile fields passed using a ref or out parameter might be problematic: routine does not know about volatile
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
