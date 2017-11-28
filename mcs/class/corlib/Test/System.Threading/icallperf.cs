// This is a benchmark to be run w/ and w/o the change
// to compute PerformanceCounter ("Mono Threadpool", "Work Items Added").

using System;
using System.Threading;
using System.Diagnostics;

class Program
{
	static void Main ()
	{
		var workItems = new PerformanceCounter ("Mono Threadpool", "Work Items Added");
		var t1 = DateTime.Now;

		int N = 100 * 100 * 100;

		for (var i = 0; i < N; i++)
			ThreadPool.QueueUserWorkItem (_ => {});

		var t2 = DateTime.Now;
		var d0 = t2 - t1;
		var d1 = d0.TotalMilliseconds;

		Console.WriteLine("{0} items in {1}ms, {2}ms per queue", N, d1, d1 / (double)N);
		var workItems0 = workItems.NextValue();
		Console.WriteLine("workItems0:{0}", workItems0);
	}
}
