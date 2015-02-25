// Compiler options: -unsafe

unsafe class C
{
	int* X;

	static int Main()
	{
		var ptrs = new[] { 0 };

		fixed (int* p = ptrs) {
			new C (p) {
				X = {
					[0] = 1
				}
			};
		}

		if (ptrs [0] != 1)
			return 1;

		return 0;
	}

	C (int* x)
	{
		X = x;
	}
}