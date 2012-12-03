// Compiler options: -unsafe

unsafe class T {
	public static int Main () {
		int len = 10;
		int* x = stackalloc int [len];
		for (int i = 0; i < len; i++)
		{
			if (x [i] != 0)
				return i + 1;
		}
		return 0;
	}
}
