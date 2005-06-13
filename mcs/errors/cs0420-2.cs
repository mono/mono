// cs0420.cs: 'XX.j': a reference to a volatile field will not be treated as volatile
// Line: 14
// Compiler options: -unsafe /warnaserror

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
