// CS0213: You cannot use the fixed statement to take the address of an already fixed expression
// Line: 9
// Compiler options: -unsafe

unsafe struct S
{
	public void Test ()
	{
		fixed (S* i = null) {
		}
	}
}
