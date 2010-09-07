using System;
using System.Threading;
using System.Runtime.InteropServices;

class Test
{
	delegate int SimpleDelegate (int a);

	static int cb_state = 0;

	static int F (int a)
	{
		Console.WriteLine ("Test.F from delegate: " + a);
		throw new NotImplementedException ("F");
	}

	static void async_callback (IAsyncResult ar)
	{
		Console.WriteLine ("Async Callback " + ar.AsyncState);
		cb_state = 1;
	}

	static int Main ()
	{
		SimpleDelegate d = new SimpleDelegate (F);
		AsyncCallback ac = new AsyncCallback (async_callback);
		string state1 = "STATE1";
		int res = 0;

		// Call delegate via ThreadPool and check that the exception is rethrown correctly
		IAsyncResult ar1 = d.BeginInvoke (1, ac, state1);

		while (cb_state == 0)
			Thread.Sleep (0);

		try {
			res = d.EndInvoke (ar1);
			Console.WriteLine ("NO EXCEPTION");
			return 1;
		} catch (NotImplementedException) {
			Console.WriteLine ("received exception ... OK");
		}

		return 0;
	}
}
