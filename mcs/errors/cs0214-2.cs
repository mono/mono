// Compiler options: -unsafe

class X {
	static void Main ()
	{
		int b = 0;
		int a = (int *) b;
	}
}
