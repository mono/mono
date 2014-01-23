using System;
using System.Threading;
using System.Threading.Tasks;

class MyContext : SynchronizationContext
{
	public override void Post (SendOrPostCallback d, object state)
	{
		base.Post (d, state);
	}

	public override void Send (SendOrPostCallback d, object state)
	{
		base.Send (d, state);
	}
}

class X
{
	static TaskCompletionSource<bool> tcs;
	static ManualResetEvent mre, mre2;
	static int main_thread_id;

	public static int Main ()
	{
		main_thread_id = Thread.CurrentThread.ManagedThreadId;
		Console.WriteLine ("{0}:Main start", main_thread_id);

		mre = new ManualResetEvent (false);
		mre2 = new ManualResetEvent (false);
		tcs = new TaskCompletionSource<bool> ();

		Task.Factory.StartNew (new Func<Task> (ExecuteAsync), new CancellationToken (), TaskCreationOptions.LongRunning, TaskScheduler.Default);

		if (!mre.WaitOne (1000))
			return 1;

		// Have to wait little bit longer for await not to take quick path
		Thread.Sleep (10);

		Console.WriteLine ("{0}:Main Set Result", Thread.CurrentThread.ManagedThreadId);

		SynchronizationContext.SetSynchronizationContext (new MyContext ());

		tcs.SetResult (true);

		if (!mre2.WaitOne (1000))
			return 2;

		Console.WriteLine ("ok");
		return 0;
	}

	static async Task ExecuteAsync ()
	{
		var t = Thread.CurrentThread;
		Console.WriteLine ("{0} - started ", t.ManagedThreadId);

		mre.Set ();

		await tcs.Task;
		t = Thread.CurrentThread;
		Console.WriteLine ("{0} - resumed ", t.ManagedThreadId);

		//
		// Continuation cannot resume on main thread because it has synchronization context set
		//
		if (main_thread_id != Thread.CurrentThread.ManagedThreadId)
			mre2.Set ();
	}
}