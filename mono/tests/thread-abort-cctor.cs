
using System;
using System.Threading;

class Driver
{
	public static ManualResetEvent mre1 = new ManualResetEvent (false);
	public static ManualResetEvent mre2 = new ManualResetEvent (false);

	class StaticConstructor1
	{
		static StaticConstructor1 ()
		{
			Console.WriteLine ("StaticConstructor1.StaticConstructor1 (1)");
			Driver.mre1.Set ();
			Thread.Sleep (1000);
			Console.WriteLine ("StaticConstructor1.StaticConstructor1 (2)");
		}

		public static void Init ()
		{
			Console.WriteLine ("StaticConstructor1.Init");
		}
	}

	static void Test1 ()
	{
		Console.WriteLine ("Test 1:");

		Driver.mre1.Reset ();
		Driver.mre2.Reset ();

		Thread thread = new Thread (() => {
			try {
				StaticConstructor1.Init ();
			} catch (Exception e) {
				Console.WriteLine (e);

				if (!(e is ThreadAbortException))
					throw;
			}
		});

		thread.Start ();

		Driver.mre1.WaitOne ();

		// The ThreadAbortException should land while in
		// the StaticConstructor1.cctor. The exception should
		// be queued, and be rethrown when exiting the cctor.
		thread.Abort ();

		thread.Join ();
	}

	class StaticConstructor2Exception : Exception {}

	class StaticConstructor2
	{
		static StaticConstructor2 ()
		{
			Console.WriteLine ("StaticConstructor2.StaticConstructor2 (1)");
			Driver.mre1.Set ();
			throw new StaticConstructor2Exception ();
			/* Unreachable */
			Driver.mre2.Set ();
			Console.WriteLine ("StaticConstructor2.StaticConstructor2 (2)");
		}

		public static void Init ()
		{
			Console.WriteLine ("StaticConstructor2.Init");
		}
	}

	static void Test2 ()
	{
		Console.WriteLine ("Test 2:");

		Driver.mre1.Reset ();
		Driver.mre2.Reset ();

		Thread thread = new Thread (() => {
			try {
				StaticConstructor2.Init ();
			} catch (TypeInitializationException e) {
				Console.WriteLine (e);

				if (!(e.InnerException is StaticConstructor2Exception))
					throw;
			}
		});

		thread.Start ();

		Driver.mre1.WaitOne ();

		// A InvalidOperationException should be thrown while in
		// the StaticConstructor2.cctor. The exception should
		// be wrapped in a TypeInitializationException.

		if (Driver.mre2.WaitOne (500)) {
			/* We shouldn't reach Driver.mre.Set () in StaticConstructor2.cctor */
			Environment.Exit (1);
		}

		thread.Join ();
	}

	public static void Main ()
	{
		Test1 ();
		Test2 ();
	}
}
