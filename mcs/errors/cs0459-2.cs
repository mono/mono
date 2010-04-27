// CS0459: Cannot take the address of fixed variable `a'
// Line: 10
// Compiler options: -unsafe

class C
{
	static int i;
	
	public static unsafe void Test ()
	{
		fixed (int* a = &i) {
			int** x = &a;
		}
    }
}
