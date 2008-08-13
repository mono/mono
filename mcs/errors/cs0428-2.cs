// CS0428: Cannot convert method group `Main' to non-delegate type `void*'. Consider using parentheses to invoke the method
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
