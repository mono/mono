// CS0411: The type arguments for method `AsyncTypeInference.Test2<T>(System.Func<int,T>)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 11

using System;
using System.Threading.Tasks;

class AsyncTypeInference
{
	public static int Main ()
	{
		Test2 (async l => { await TT (); return null; } );
		return 0;
	}
	
	static Task TT ()
	{
		return Task.Factory.StartNew (() => 2);
	}

	static void Test2<T> (Func<int, T> arg)
	{
		arg (0);
	}
}
