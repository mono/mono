// cs1666.cs: You cannot use fixed size buffers contained in unfixed expressions. Try using the fixed statement
// Line: 11
// Compiler options: -unsafe

public unsafe struct S
{
    fixed char test_1 [128];
	
    public void Test ()
    {
	test_1 [55] = 'g';
    }
}
