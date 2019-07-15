using System;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class AbortBlockingConsoleIOTest
	{
		void StartBlockingConsoleReadCall ()
		{
			Console.ReadLine ();
		}

		[Test]
		public void AbortBlockingConsoleIOReadCall ()
		{
			Thread readThread = new Thread (StartBlockingConsoleReadCall);
			readThread.Start ();
			Thread.Sleep (2000);

			readThread.Abort ();
			readThread.Join ();
		}

	}
}
