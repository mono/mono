using System;
using System.Threading.Tasks;
using System.Threading;

class C
{
	ManualResetEvent mre = new ManualResetEvent (false);
	ManualResetEvent mre_task = new ManualResetEvent (false);

	public async Task<int> TestTaskGeneric ()
	{
		await Task.Factory.StartNew (() => {
			mre_task.Set ();
			mre.WaitOne (3000);
			return 5;
		}).ConfigureAwait (false);;

		return 1;
	}

	public static int Main ()
	{
		var c = new C ();
		
		var t2 = c.TestTaskGeneric ();
		if (t2.Status != TaskStatus.WaitingForActivation)
			return 1;
		
		c.mre_task.WaitOne (3000);
		c.mre.Set ();
		
		if (!Task.WaitAll (new[] { t2 }, 3000))
			return 2;

		if (t2.Result != 1)
			return 3;

		if (t2.Status != TaskStatus.RanToCompletion)
			return 4;

		return 0;
	}
}
