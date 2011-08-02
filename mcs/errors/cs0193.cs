// CS0193: The * or -> operator must be applied to a pointer
// Line: 9
// Compiler options: -unsafe

unsafe class X {
	static void Main ()
	{
		int a = 0;
		if (*a == 0)
			return 1;
		
		return 0;
	}
}
