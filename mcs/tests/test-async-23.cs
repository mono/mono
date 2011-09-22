// Compiler options: -langversion:future

using System;
using System.Threading;
using System.Threading.Tasks;

class MyContext : SynchronizationContext
{
	public int Started;
	public int Completed;
	public int PostCounter;
	public int SendCounter;

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
	static async Task<int> Test ()
	{
		return await Task.Factory.StartNew (() => 1);
	}

	public static int Main ()
	{
		var context = new MyContext ();
		try {
			SynchronizationContext.SetSynchronizationContext (context);
			var t = Test ();
			if (!t.Wait (1000))
				return 3;
		} finally {
			SynchronizationContext.SetSynchronizationContext (null);
		}

		if (context.Started != 0 || context.Completed != 0 || context.SendCounter != 0)
			return 1;

		if (context.PostCounter != 1)
			return 2;

		Console.WriteLine ("ok");
		return 0;
	}
}
