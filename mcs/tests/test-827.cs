// Compiler options: -r:test-827-lib.dll

// Only to check we are compatible with broken csc definite assignment implementation

class Program
{
	public static void Main ()
	{
		S s;
		s.Test ();
	}
}
