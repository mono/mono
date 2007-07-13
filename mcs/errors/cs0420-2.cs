// cs0420-2.cs: `XX.j': A volatile fields passed using a ref or out parameter might be problematic: routine does not know about volatile
// Line: 14
// Compiler options: -unsafe /warnaserror /warn:1

unsafe class XX {
	static volatile int j;

	static void X (ref int a)
	{
	}
	
	static void Main ()
	{
		X (ref j);
	}
}
