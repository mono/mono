//
// StdlibTest.cs:
// 	NUnit Test Cases for Mono.Unix.Native.Stdlib
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004-2005 Jonathan Pryor
// 

using System;
using System.Text;

using NUnit.Framework;

using Mono.Unix.Native;

namespace MonoTests.Mono.Unix.Native {

	[TestFixture, Category ("NotOnWindows")]
	public class StdlibTest
	{
		private class SignalTest {
			public int signalReceived;

			public void Handler (int sn)
			{
				signalReceived = sn;
			}
		}


		[Test]
		public void GetPid ()
		{
			var currentPID = Syscall.getpid();
			Assert.AreNotEqual (0, currentPID);
		}

		// [Test]
		public void Signal ()
		{
			SignalTest st = new SignalTest ();

			// Insert handler
			SignalHandler oh = Stdlib.signal (Signum.SIGURG, 
					new SignalHandler (st.Handler));

			st.signalReceived = ~NativeConvert.FromSignum (Signum.SIGURG);

			// Send signal
			Stdlib.raise (Signum.SIGURG);

			Assert.IsTrue (
				NativeConvert.ToSignum (st.signalReceived) == Signum.SIGURG,
					"#IH: Signal handler not invoked for SIGURG");

			// Reset old signal
			Stdlib.signal (Signum.SIGURG, oh);

			st.signalReceived = NativeConvert.FromSignum (Signum.SIGUSR1);
			Stdlib.raise (Signum.SIGURG);

			Assert.IsFalse (NativeConvert.ToSignum (st.signalReceived) == Signum.SIGURG,
					"#IH: Signal Handler invoked when it should have been removed!");
		}
#if !NETCOREAPP2_0
		[Test]
		// MSVCRT.DLL doesn't export snprintf(3).
		[Category ("NotDotNet")]
		[Category ("NotWorking")]
		public void Snprintf ()
		{
			StringBuilder s = new StringBuilder (1000);
			Stdlib.snprintf (s, "hello, %s world!\n");
			Assert.AreEqual (s.ToString(), "hello, %s world!\n", 
					"#SNPF: string not echoed");
			s = new StringBuilder (1000);
			Stdlib.snprintf (s, "yet another %s test", "simple");
			Assert.AreEqual (s.ToString(), "yet another simple test",
					"#SNPF: string argument not printed");
			s = new StringBuilder (1000);
			string fmt = 
@"this is another test:
	  char: '%c'
	 short: %i
	   int: %i
	  long: %li
	 float: %g
	double: %g" + "\n";
		Stdlib.snprintf (s, fmt, 'a', (short) 16, 32, (long) 64, (double) 32.23, 64.46);
			string expected = 
@"this is another test:
	  char: 'a'
	 short: 16
	   int: 32
	  long: 64
	 float: 32.23
	double: 64.46" + "\n";
			Assert.AreEqual (s.ToString(), expected,
					"#SNPF: printf of many builtin types failed");
		}
#endif
	}
}

