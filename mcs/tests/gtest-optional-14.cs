using System;
using System.Threading.Tasks;

class C
{
	static void M (int x, int y = 1)
	{
	}

	static void M<T> (T x, int y = 2)
	{
		throw new ApplicationException ();
	}

	static void M2<T, U> (T u, Func<T, U> c, int y = 1)
	{
		throw new ApplicationException ();
	}

	static void M2<T, U> (T u, Func<T, Task<U>> c, int y = 2)
	{
	}

	static void Main ()
	{ 
		M (1);
		M2 (1, s => Task.FromResult (s));
	}
}