// Compiler options: -langversion:future

using System;
using System.Threading.Tasks;
using System.Threading;

class C
{
	public async Task<int> TestResult ()
	{
		if (await Task.Factory.StartNew (() => 8) != 9) {
			return 2;
		}
		
		return 0;
	}

	public static int Main ()
	{
		var c = new C ();
		var t = c.TestResult ();

		if (!Task.WaitAll (new[] { t }, 3000))
			return 1;

		if (t.Status != TaskStatus.RanToCompletion)
			return 2;
		
		if (t.Result != 2)
			return 3;
		
		Func<Task<int>> f = async () => {
			var tr = await Task.Factory.StartNew (() => 1);
			if (tr == 1)
				return 3;

			return 1;
		};
		
		var t2 = f ();

		if (!Task.WaitAll (new[] { t2 }, 3000))
			return 4;

		if (t2.Status != TaskStatus.RanToCompletion)
			return 5;
		
		if (t2.Result != 3)
			return 6;

		Console.WriteLine ("ok");
		return 0;
	}
}
