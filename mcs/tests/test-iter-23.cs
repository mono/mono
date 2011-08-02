using System;
using System.Collections;
using System.Threading;

class X
{
	static ManualResetEvent dispose = new ManualResetEvent (false);

	static IEnumerable GetIt2 ()
	{
		yield return Delay ();
		yield return 2;
	}

	static int Delay ()
	{
		dispose.Set ();
		Thread.Sleep (10);
		return 1;
	}

	static int Main ()
	{
		var e = GetIt2 ().GetEnumerator ();
		ThreadPool.QueueUserWorkItem (l => {
			dispose.WaitOne ();
			((IDisposable) e).Dispose ();
		});

		if (!e.MoveNext ())
			return 1;

		if (e.MoveNext ())
			return 2;

		Console.WriteLine (e.Current);
		if ((int) e.Current != 1)
			return 3;

		return 0;
	}
}
