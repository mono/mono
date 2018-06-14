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
		int index = WaitHandle.WaitAny (wh);
		bool success = index == 0;
		Console.WriteLine (success ? "success" : "failed");
		Environment.Exit (success ? 0 : 1);
	}
}
