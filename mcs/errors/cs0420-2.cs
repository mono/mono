// CS0420: `XX.j': A volatile field references will not be treated as volatile
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
