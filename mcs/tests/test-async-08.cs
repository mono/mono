// Compiler options: -langversion:future

using System;
using System.Threading.Tasks;

class AsyncTypeInference
{
	public static int Main ()
	{
		Test (async l => await Task.Factory.StartNew (() => 1));
		Test (async l => { return await Task.Factory.StartNew (() => 1); });
		Test2 (async l => { await TT (); } );
		return 0;
	}
	
	static Task TT ()
	{
		return Task.Factory.StartNew (() => 2);
	}

	static void Test<T> (Func<int, Task<T>> arg)
	{
		arg (0);
	}
	
	static void Test2<T> (Func<int, T> arg)
	{
		arg (0);
	}
}
