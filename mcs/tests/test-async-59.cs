using System;
using System.Threading;
using System.Threading.Tasks;

class X
{
	static bool unobserved;

	public static int Main ()
	{
		TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
		try {
			Test ().Wait ();

			GC.Collect ();
			GC.WaitForPendingFinalizers ();
			if (unobserved)
				return 1;

			return 0;
		} finally {
			TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
		}
	}

	static void TaskScheduler_UnobservedTaskException (object sender, UnobservedTaskExceptionEventArgs e)
	{
		unobserved = true;
		Console.WriteLine ("unobserved");
	}

	static async Task Test ()
	{
		try {
			await ThrowAsync ();
		} catch {			
		}
	}

	static async Task ThrowAsync()
	{
		await Task.Delay (5);

		throw new Exception ("boom");
	}
}