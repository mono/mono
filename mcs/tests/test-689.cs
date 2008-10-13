// Compiler options: -unsafe

public struct Sample
{
	public static unsafe void Foo (byte* a, byte* b)
	{
		int* p = (int*)0;
		long* s = (long*) ++p;
		s = (long*) --p;
		
		int v = 0;
		s = (long*) -v;
		s = (long*) +v;

		byte c = (byte)(*a++ * *b++);
	}
	
	public static void Main ()
	{
		int i = (global::System.Int32)1;
	}
}

