using System;
using System.Threading;
using System.Threading.Tasks;

class MyContext : SynchronizationContext
{
	public int Started;
	public int Completed;
	public int PostCounter;
	public int SendCounter;
	ManualResetEvent mre;
	
	public MyContext (ManualResetEvent mre)
	{
		this.mre = mre;
	}

	public override void OperationStarted ()
	{
		++Started;
		base.OperationStarted ();
	}

	public override void OperationCompleted ()
	{
		++Completed;
		base.OperationCompleted ();
	}

	public override void Post (SendOrPostCallback d, object state)
	{
		++PostCounter;
		mre.Set ();
		base.Post (d, state);
	}

	public override void Send (SendOrPostCallback d, object state)
	{
		++SendCounter;
		base.Send (d, state);
	}
}


public class TestPostContext
{
	static ManualResetEvent await_mre;
	
	static async Task<int> Test ()
	{
		return await Task.Factory.StartNew (() => { await_mre.WaitOne(); return 1; });
	}

	public static int Main ()
	{
		var mre = new ManualResetEvent (false);
		await_mre = new ManualResetEvent (false);
		var context = new MyContext (mre);
		try {
			SynchronizationContext.SetSynchronizationContext (context);
			var t = Test ();
			await_mre.Set ();
			if (!t.Wait (3000))
				return 3;
				
			// Wait is needed because synchronization is executed as continuation (once task finished)
			if (!mre.WaitOne (3000))
				return 2;
		} finally {
			SynchronizationContext.SetSynchronizationContext (null);
		}

		if (context.Started != 0 || context.Completed != 0 || context.SendCounter != 0)
			return 1;

		Console.WriteLine ("ok");
		return 0;
	}
}
