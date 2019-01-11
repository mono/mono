// CS8385: The given expression cannot be used in a fixed statement
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
