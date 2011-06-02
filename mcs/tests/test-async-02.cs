// Compiler options: -langversion:future

using System;
using System.Threading.Tasks;
using System.Threading;

class C
{
	ManualResetEvent mre = new ManualResetEvent (false);

	public async Task TestTask ()
	{
		await Call ();
	}

	Task Call ()
	{
		return Task.Factory.StartNew (() => {
			mre.WaitOne ();
			Console.WriteLine ("a");
		});
	}

	public async Task<int> TestTaskGeneric ()
	{
		return await CallGeneric ();
	}

	Task<int> CallGeneric ()
	{
		return Task.Factory.StartNew (() => {
			mre.WaitOne ();
			return 5;
		});
	}

	public static int Main ()
	{
		var c = new C ();
		var t = c.TestTask ();
		if (t.Status != TaskStatus.WaitingForActivation)
			return 1;

		c.mre.Set ();
		Task.WaitAll (t);

		if (t.Status != TaskStatus.RanToCompletion)
			return 2;

		c = new C ();
		var t2 = c.TestTaskGeneric ();
		if (t2.Status != TaskStatus.WaitingForActivation)
			return 3;

		c.mre.Set ();
		Task.WaitAll (t2);

		if (t2.Result != 5)
			return 4;

		if (t.Status != TaskStatus.RanToCompletion)
			return 5;

		return 0;
	}
}
