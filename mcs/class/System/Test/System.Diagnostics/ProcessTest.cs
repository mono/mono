//
// ProcessTest.cs - NUnit Test Cases for System.Diagnostics.Process
//
// Authors:
//   Gert Driesen (drieseng@users.sourceforge.net)
//   Robert Jordan <robertj@gmx.net>
//
// (C) 2007 Gert Driesen
// 

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Diagnostics
{
	[TestFixture]
	public class ProcessTest
	{
		[Test]
		public void GetProcessById_MachineName_Null ()
		{
			try {
				Process.GetProcessById (1, (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNotNull (ex.ParamName, "#4");
				Assert.AreEqual ("machineName", ex.ParamName, "#5");
				Assert.IsNull (ex.InnerException, "#6");
			}
		}

		[Test]
		public void GetProcesses_MachineName_Null ()
		{
			try {
				Process.GetProcesses ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNotNull (ex.ParamName, "#4");
				Assert.AreEqual ("machineName", ex.ParamName, "#5");
				Assert.IsNull (ex.InnerException, "#6");
			}
		}

		[Test]
		[Category ("NotDotNet")]
		public void TestRedirectedOutputIsAsync ()
		{
			// Test requires cygwin, so we just bail out for now.
			if (Path.DirectorySeparatorChar == '\\')
				return;
			
			Process p = new Process ();
			p.StartInfo = new ProcessStartInfo ("/bin/sh", "-c \"sleep 2; echo hello\"");
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.UseShellExecute = false;
			p.Start ();

			Stream stdout = p.StandardOutput.BaseStream;

			byte [] buffer = new byte [200];

			// start async Read operation
			DateTime start = DateTime.Now;
			IAsyncResult ar = stdout.BeginRead (buffer, 0, buffer.Length,
							    new AsyncCallback (Read), stdout);

			Assert.IsTrue ((DateTime.Now - start).TotalMilliseconds < 1000, "#01 BeginRead was not async");
			p.WaitForExit ();
			Assert.AreEqual (0, p.ExitCode, "#02 script failure");
			ar.AsyncWaitHandle.WaitOne (2000, false);
			if (bytesRead < "hello".Length)
				Assert.Fail ("#03 got {0} bytes", bytesRead);
			Assert.AreEqual ("hello", Encoding.Default.GetString (buffer, 0, 5), "#04");
		}

		void Read (IAsyncResult ar)
		{
			Stream stm = (Stream) ar.AsyncState;
			bytesRead = stm.EndRead (ar);
		}

		int bytesRead = -1;
	}
}
