using System;
using System.Collections;
using System.Threading;

class X
{
	static ManualResetEvent dispose = new ManualResetEvent (false);
	static ManualResetEvent wait = new ManualResetEvent (false);

	static IEnumerable GetIt2 ()
	{
		yield return Delay ();
		yield return 2;
	}

	static int Delay ()
	{
		dispose.Set ();
		wait.WaitOne ();
		return 1;
	}

	public static int Main ()
	{
		var e = GetIt2 ().GetEnumerator ();
		ThreadPool.QueueUserWorkItem (l => {
			dispose.WaitOne ();
			((IDisposable) e).Dispose ();
			wait.Set ();
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
