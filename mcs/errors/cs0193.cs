// cs0193.cs: * or -> operator can only be applied to pointer types.
// Line: 8

unsafe class X {
	static void Main ()
	{
		int a = 0;
		if (*a == 0)
			return 1;
		
		return 0;
	}
}
