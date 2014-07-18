using System;
using System.Threading.Tasks;

class C
{
	static int counter;

	public static async Task TestRethrow (Exception e)
	{
		try {
			throw e;
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
			Console.WriteLine ("ga");
			throw;
		}
	}

	public static int Main ()
	{
		var ex = new ApplicationException ();
		try {
			TestRethrow (ex).Wait ();
		} catch (AggregateException e) {
			if (e.InnerException != ex)
				return 1;
		}

		if (counter != 3)
			return 2;

		var ex2 = new NotSupportedException ();
		try {
			TestRethrow (ex2).Wait ();
		} catch (AggregateException e) {
			if (e.InnerException != ex2)
				return 3;
		}

		if (counter != 9)
			return 4;

		Console.WriteLine ("ok");
		return 0;
	}
}
