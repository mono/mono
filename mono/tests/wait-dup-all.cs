/*
https://github.com/mono/mono/issues/9089
Duplication is ok for WaitAny, exception for WaitAll.
System.DuplicateWaitObjectException: Duplicate objects in argument.
*/
using System;
using System.Threading;

class Program
{
	public static void Main()
	{
		var wh = new ManualResetEvent [2];
		wh [0] = new ManualResetEvent (true);
		wh [1] = wh [0];
		try {
			var result = WaitHandle.WaitAll (wh);
			// Should not get here.
			Console.WriteLine ("failed");
			Environment.Exit (1);
		} catch (System.DuplicateWaitObjectException) {
			Console.WriteLine ("success");
			Environment.Exit (0);
		}
	}
}
