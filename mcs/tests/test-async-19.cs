// Compiler options: -langversion:future

using System;
using System.Threading;
using System.Threading.Tasks;

class C
{
	static ManualResetEvent caught = new ManualResetEvent (false);

	static async void Test (ManualResetEvent mre)
	{
		var a = Task.Factory.StartNew (() => {
			if (mre.WaitOne (1000))
				throw new ApplicationException ();
		});

		await a;
	}

	public static int Main ()
	{
		ManualResetEvent mre = new ManualResetEvent (false);
		Test (mre);

		var handler = new UnhandledExceptionEventHandler (CurrentDomain_UnhandledException);
		AppDomain.CurrentDomain.UnhandledException += handler;
		try {
			mre.Set ();

			if (!caught.WaitOne (1000))
				return 1;

			return 0;
		} finally {
			AppDomain.CurrentDomain.UnhandledException -= handler;
		}
	}

	static void CurrentDomain_UnhandledException (object sender, UnhandledExceptionEventArgs e)
	{
		if (e.ExceptionObject is ApplicationException)
			caught.Set ();
	}
}
