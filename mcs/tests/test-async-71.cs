// Compiler options: -unsafe

using System;
using System.Threading.Tasks;

class UnsafeContext
{
	static int Main()
	{
		if (TestUnsafe (1).Result != 0)
			return 1;

		return 0;
	}

	static async Task<int> TestUnsafe (int g)
	{
		unsafe {
			int* ga = &g;
		}

		await Task.Yield ();
		return 0;
	}
}