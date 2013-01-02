// Compiler options: -langversion:future
using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static ManualResetEvent master_mre = new ManualResetEvent (false);
	static ManualResetEvent async_mre = new ManualResetEvent (false);

	static int pos;

	public static int Main ()
	{
		pos = 0;
		TestAsync ();

		if (pos != 1)
			throw new ApplicationException (pos.ToString ());

		pos = 2;

		master_mre.Set ();

		if (!async_mre.WaitOne (3000))
			return 1;

		if (pos != 4)
			throw new ApplicationException (pos.ToString ());

		return 0;
	}

	static async void TestAsync ()
	{
		pos = 1;

		await RunAsync ().ConfigureAwait (false);

		if (pos != 3)
			throw new ApplicationException (pos.ToString ());

		pos = 4;
		async_mre.Set ();
	}

	static Task RunAsync ()
	{
		return Task.Factory.StartNew (() => {
			master_mre.WaitOne ();
			Console.WriteLine ("Hello async");
			if (pos != 2)
				throw new ApplicationException (pos.ToString ());

			pos = 3;
		});
	}
}
