// Compiler options: -langversion:future

using System;
using System.Threading.Tasks;
using System.Threading;

class C
{
	ManualResetEvent mre = new ManualResetEvent (false);

	public async Task TestTask ()
	{
		await Call ().ConfigureAwait (false);
	}

	public async Task TestTask2 ()
	{
		await Call ().ConfigureAwait (false);
		return;
	}

	Task Call ()
	{
		return Task.Factory.StartNew (() => {
			mre.WaitOne (3000);
			Console.WriteLine ("a");
		});
	}

	public async Task<int> TestTaskGeneric ()
	{
		return await CallGeneric ().ConfigureAwait (false);
	}

	Task<int> CallGeneric ()
	{
		return Task.Factory.StartNew (() => {
			mre.WaitOne (3000);
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
		if (!Task.WaitAll (new[] { t }, 3000))
			return 2;

		if (t.Status != TaskStatus.RanToCompletion)
			return 3;

		c = new C ();
		t = c.TestTask2 ();
		if (t.Status != TaskStatus.WaitingForActivation)
			return 4;

		c.mre.Set ();
		if (!Task.WaitAll (new[] { t }, 3000))
			return 5;

		if (t.Status != TaskStatus.RanToCompletion)
			return 6;

		c = new C ();
		var t2 = c.TestTaskGeneric ();
		if (t2.Status != TaskStatus.WaitingForActivation)
			return 7;

		c.mre.Set ();
		if (!Task.WaitAll (new[] { t2 }, 3000))
			return 8;

		if (t2.Result != 5)
			return 9;

		if (t2.Status != TaskStatus.RanToCompletion)
			return 10;

		return 0;
	}
}
