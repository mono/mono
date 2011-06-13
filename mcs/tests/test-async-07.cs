// Compiler options: -langversion:future

using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static int Main ()
	{
		var mre_l = new ManualResetEvent (false);
		var mre = new ManualResetEvent (false);

		Func<string, Task<string>> f = async l =>
			await Task.Factory.StartNew (() => {
				if (!mre_l.WaitOne (3000))
					throw new ApplicationException ("3");

				return l;
			});

		var r = f ("a");
		mre_l.Set ();
		if (!r.Wait (3000))
			return 1;

		if (r.Result != "a")
			return 11;

		mre_l.Reset ();

		Func<Task> ff = async () =>
			await Task.Factory.StartNew (() => {
				if (!mre_l.WaitOne (3000))
					throw new ApplicationException ("3");
			});

		var rr = ff ();
		mre_l.Set ();
		if (!rr.Wait (3000))
			return 2;

		Func<short, Task<short>> f2 = async l => l;

		var r2 = f2 (88);
		if (r2.Result != 88)
			return 3;
		
		mre.Reset ();
		mre_l.Reset ();
		Action a = async () => await Task.Factory.StartNew (() => {
				if (!mre_l.WaitOne (3000))
					throw new ApplicationException ("4");
				mre.Set ();
			}, CancellationToken.None);
		
		a ();
		mre_l.Set ();
		if (!mre.WaitOne (3000))
			return 4;

		Console.WriteLine ("ok");
		return 0;
	}
}
