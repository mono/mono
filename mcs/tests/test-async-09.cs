// Compiler options: -langversion:future

using System;
using System.Threading.Tasks;

class Test
{
	static int Foo<T> (Func<T, Task<int>> f)
	{
		return 1;
	}
	
	static int Foo<T> (Func<T, Task<short>> f)
	{
		return 2;
	}

	static int Main ()
	{
		if (Foo (async (string str) => (short) 1) != 2)
			return 1;
		
		return 0;
	}
}
