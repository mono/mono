// CS0029: Cannot implicitly convert type `T' to `int*'
// Line: 8
// Compiler options: -unsafe

class T
{
	static unsafe void Main ()
	{
		int* a = default (T);
	}
}
