using System;
using System.Threading;
using System.Threading.Tasks;

class Test
{
	public static int Main ()
	{
		Task<int> t = TestMethod ();

		try {
			t.Start ();
			return 1;
		} catch (InvalidOperationException) {
		}

		try {
			t.RunSynchronously ();
			return 2;
		} catch (InvalidOperationException) {
		}

		Console.WriteLine ("ok");
		return 0;
	}

	async static Task<int> TestMethod ()
	{
		await Task.Delay (100000);
		return 1;
	}
}