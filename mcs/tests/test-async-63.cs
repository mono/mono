using System;
using System.Threading.Tasks;

class C
{
	static int counter;
	public static async Task TestSingleAwait (bool throwException)
	{
		try {
			if (throwException)
				throw new ApplicationException ();
		} catch (ApplicationException ex) {
			Console.WriteLine ("x1a");
			++counter;
			await Call ();
			Console.WriteLine ("x2a");
			++counter;
		} catch {
			throw;
		}

		Console.WriteLine ("end");
	}
	
	public static async Task TestDoubleAwait (bool throwException)
	{
		try {
			if (throwException)
				throw new ApplicationException ();
		} catch (ApplicationException ex) {
			Console.WriteLine ("x1a");
			++counter;
			await Call ();
			Console.WriteLine ("x2a");
			++counter;
		} catch {
			Console.WriteLine ("x1b");
			counter += 4;
			await Call ();
			Console.WriteLine ("x2b");
			counter += 7;
		}

		Console.WriteLine ("end");
	}

	static Task Call ()
	{
		return Task.Factory.StartNew (() => false);
	}

	void HH ()
	{
		try {
				throw new ApplicationException ();
		} catch {
			throw;
		}
	}

	public static int Main ()
	{
		TestSingleAwait (true).Wait ();
		Console.WriteLine (counter);
		if (counter != 2)
			return 1;

		TestSingleAwait (false).Wait ();
		if (counter != 2)
			return 1;

		counter = 0;

		TestDoubleAwait (true).Wait ();
		Console.WriteLine (counter);
		if (counter != 2)
			return 3;

		TestDoubleAwait (false).Wait ();
		if (counter != 2)
			return 4;		

		return 0;
	}
}
