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
