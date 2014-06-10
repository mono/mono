using System;
using System.Threading.Tasks;

class C
{
	static int counter;

	public static async Task TestRethrow ()
	{
		try {
			throw new ApplicationException ();
		} catch (ApplicationException) {
			Console.WriteLine ("x1a");
			counter = 1;
			await Task.Delay (1);
			Console.WriteLine ("x2a");
			counter = 3;
			throw;
		} catch {
			counter = 9;
			await Task.Delay (1);
			throw;
		}
	}

	public static int Main ()
	{
		try {
			TestRethrow ().Wait ();
		} catch (AggregateException e) {
			if (!(e.InnerException is ApplicationException))
				return 1;
		}

		if (counter != 3)
			return 2;

		Console.WriteLine ("ok");
		return 0;
	}
}
