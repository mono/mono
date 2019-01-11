// CS8385: The given expression cannot be used in a fixed statement
// Line: 9
// Compiler options: -unsafe

unsafe class C
{
	public static void Main ()
	{
		fixed (void* f = Main)
		{
		}
	}
}
