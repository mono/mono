using System;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static async Task<int> TestCanceled()
	{
		await Task.FromResult(1);
		throw new OperationCanceledException();
	}

	static async Task TestCanceled_2()
	{
		await Task.FromResult(1);
		throw new OperationCanceledException();
	}

	static async Task<int> TestException()
	{
		await Task.FromResult(1);
		throw new ApplicationException();
	}

	public static int Main()
	{
		bool canceled = false;
		var t = TestCanceled().ContinueWith(l =>
		{
			canceled = l.IsCanceled;
		}, TaskContinuationOptions.ExecuteSynchronously);

		t.Wait();

		if (!canceled)
			return 1;

		if (t.Exception != null)
			return 2;

		t = TestCanceled_2().ContinueWith(l =>
		{
			canceled = l.IsCanceled;
		}, TaskContinuationOptions.ExecuteSynchronously);

		t.Wait();

		if (!canceled)
			return 11;

		if (t.Exception != null)
			return 12;

		bool faulted = false;
		bool has_exception = false;
		t = TestException().ContinueWith(l =>
		{
			faulted = l.IsFaulted;
			has_exception = l.Exception != null; // Has to observe it or will throw on shutdown
		}, TaskContinuationOptions.ExecuteSynchronously);

		if (!faulted)
			return 21;
			
		if (!has_exception)
			return 22;

		if (t.Exception != null)
			return 23;

		Console.WriteLine("ok");
		return 0;
	}
}
