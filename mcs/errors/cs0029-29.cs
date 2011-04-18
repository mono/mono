// CS0019: Cannot implicitly convert type `dynamic' to `int*'
// Line: 10
// Compiler options: -unsafe

public unsafe class C
{
	public static void Main ()
	{
		dynamic d = 1;
		int* i = d;
	}
}
