// CS0459: Cannot take the address of foreach iteration variable `c'
// Line: 10
// Compiler options: -unsafe

class C
{
	public static unsafe void Main ()
	{
		foreach (char c in "test") {
			char* ch = &c;
		}
    }
}
