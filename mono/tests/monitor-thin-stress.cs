using System;
using System.Threading;

public class Container
{
	public static int Main (string[] args) 
	{
		const int numRuns = 100000;
		const int numIncrements = 20000;
		const int numThreads = 10;

		var theLock = new object ();
		var threads = new Thread[numThreads];
		long count = 0;

		for (int i = 0; i < threads.Length; ++i) {
			int x = i;
			(threads [i] = new Thread (() =>
			{
				int runs = numRuns;
				do {
					if (Monitor.TryEnter (theLock, x)) {
						for (int j = 0; j < numIncrements; ++j) {
							count += 1;
						}
						Monitor.Exit (theLock);
						--runs;
					}
				} while (runs > 0);
			})).Start ();
		}

		for (int i = 0; i < threads.Length; ++i) {
			threads [i].Join ();
		}

		Console.WriteLine ("{0}", count);
		Console.WriteLine("{0}", (long)numRuns * numThreads * numIncrements);
		 
		return (int) (count - (long)numRuns * numThreads * numIncrements);
	}
}