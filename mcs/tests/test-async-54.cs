using System;
using System.Threading;
using System.Threading.Tasks;

class Test
{
	public static int Main ()
	{
		int res;
		res = TestMethod (new TaskCanceledException ()).Result;
		if (res != 0)
			return 10 * res;

		res = TestMethod (new OperationCanceledException ("my message")).Result;
		if (res != 0)
			return 20 * res;

		return 0;
	}

	async static Task<int> TestMethod (Exception ex)
	{
		try {
			await Foo (ex);
		} catch (OperationCanceledException e) {
			if (e == ex)
				return 0;

			return 1;
		}

		return 2;
	}


	async static Task Foo (Exception e)
	{
		await Task.Delay (1);
		throw e;
	}
}