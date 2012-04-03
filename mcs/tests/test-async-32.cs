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

	static int Main()
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
		t = TestException().ContinueWith(l =>
		{
			faulted = l.IsFaulted;
		}, TaskContinuationOptions.ExecuteSynchronously);

		if (!faulted)
			return 21;

		if (t.Exception != null)
			return 22;

		Console.WriteLine("ok");
		return 0;
	}
}
