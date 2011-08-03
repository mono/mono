// Compiler options: -langversion:future

using System;
using System.Threading.Tasks;
using System.Threading;

class C
{
	static async Task<int> TestNested_1 ()
	{
		return Call (
			await Task.Factory.StartNew (() => { Thread.Sleep (10); return 5; }),
			await Task.Factory.StartNew (() => -3),
			await Task.Factory.StartNew (() => 6));
	}

	static int Call (int arg1, int arg2, int arg3)
	{
		if (arg1 != 5)
			return 1;

		if (arg2 != -3)
			return 2;

		if (arg3 != 6)
			return 3;

		return 0;
	}

	public static int Main ()
	{
		var t1 = TestNested_1 ();
		if (!Task.WaitAll (new[] { t1 }, 1000))
			return 1;

		if (t1.Result != 0)
			return 2;

		Console.WriteLine ("ok");
		return 0;
	}
}
