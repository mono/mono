// Compiler options: -langversion:future

using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	public static int Main ()
	{
		var mre = new ManualResetEvent (false);
		var mre_l = new ManualResetEvent (false);

		Action a = async () => {
			await Task.Factory.StartNew (() => {
				if (!mre_l.WaitOne (3000))
					throw new ApplicationException ("1");
			}).ConfigureAwait (false);

			if (mre_l.WaitOne ())
				mre.Set ();
		};

		a ();
		mre_l.Set ();
		if (!mre.WaitOne (3000))
			return 1;

		mre.Reset ();
		mre_l.Reset ();

		Action a2 = async delegate {
			await Task.Factory.StartNew (() => {
				if (!mre_l.WaitOne (3000))
					throw new ApplicationException ("2");
			}).ConfigureAwait (false);

			if (mre_l.WaitOne ())
				mre.Set ();
		};

		a2 ();
		mre_l.Set ();
		if (!mre.WaitOne (3000))
			return 2;

		mre_l.Reset ();

		Func<string, Task<string>> f = async l => {
			var t = await Task.Factory.StartNew (() => {
				if (!mre_l.WaitOne (3000))
					throw new ApplicationException ("3");

				return l;
			}).ConfigureAwait (false);

			return t;
		};

		var r = f ("a");
		mre_l.Set ();
		if (!r.Wait (3000))
			return 3;

		if (r.Result != "a")
			return 31;

		mre_l.Reset ();

		Func<decimal, Task<decimal>> f2 = async delegate (decimal l) {
			var t = await Task.Factory.StartNew (() => {
				if (!mre_l.WaitOne (3000))
					throw new ApplicationException ("4");

				return l;
			}).ConfigureAwait (false);

			return t;
		};

		var r2 = f2 (decimal.MaxValue);
		mre_l.Set ();
		if (!r2.Wait (3000))
			return 4;

		if (r2.Result != decimal.MaxValue)
			return 41;

		f2 = async delegate (decimal l) {
			return l;
		};

		r2 = f2 (88);
		if (r2.Result != 88)
			return 5;

		return 0;
	}
}
