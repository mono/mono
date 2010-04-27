// CS0242: The operation in question is undefined on void pointers
// Line: 10
// Compiler options: -unsafe

unsafe class C
{
	public static void Main ()
    {
		void* v = null;
		int b = *v is string;
    }
}
