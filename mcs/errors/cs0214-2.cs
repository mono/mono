// cs0214: Pointer can only be used in unsafe context
// Line: 9
// Compiler options: -unsafe

class X {
	static void Main ()
	{
		int b = 0;
		method ((int *) b);
	}
        
        unsafe static void method (int* i)
        {
        }
}
