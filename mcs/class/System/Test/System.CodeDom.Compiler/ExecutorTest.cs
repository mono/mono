//
// ExecutorTest.cs 
//	- Unit tests for System.CodeDom.Compiler.Executor
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;

using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace MonoTests.System.CodeDom.Compiler
{
	[TestFixture]
	public class ExecutorTest
	{
		private bool posix;
		private bool cmdNotFound;
		private int errcode;
		private string cmd;
		private string cd;
		private string temp;
		private TempFileCollection tfc;
		private WindowsIdentity winid;
		private IntPtr token;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			cmd = "ping"; // available everywhere
			cd = Environment.CurrentDirectory;
			temp = Path.GetTempPath ();
			tfc = new TempFileCollection ();
			winid = WindowsIdentity.GetCurrent ();
			token = winid.Token;

			try {
				string output = null;
				string error = null;
				errcode = Executor.ExecWaitWithCapture (cmd, tfc, ref output, ref error);
			}
			catch (Exception) {
				// cmd might not be in the PATH
				cmdNotFound = true;
			}
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
#if NET_2_0
			winid.Dispose ();
#else
			GC.KeepAlive (winid);
#endif
		}

		[Test]
		[ExpectedException (typeof (ExternalException))]
		public void ExecWait_NullCmd ()
		{
			Executor.ExecWait (null, new TempFileCollection ());
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void ExecWait_NullTempFileCollection ()
		{
			if (cmdNotFound)
				Assert.Ignore ("ping command not found.");

			Executor.ExecWait (cmd, null);
		}

		[Test]
		[Category ("NotWorking")] // https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=341293
		public void ExecWait ()
		{
			if (cmdNotFound)
				Assert.Ignore ("ping command not found.");

			try {
				Executor.ExecWait (cmd, new TempFileCollection ());
				Assert.Fail ("#1");
			} catch (ExternalException ex) {
				// Cannot execute a program. The command being executed was .
				Assert.AreEqual (typeof (ExternalException), ex.GetType (), "#2");
				Assert.AreEqual (2, ex.ErrorCode, "#3");
				Assert.IsNull (ex.InnerException, "#4");
				Assert.IsNotNull (ex.Message, "#5");
			}
		}

		[Test]
		public void ExecWaitWithCapture ()
		{
			if (cmdNotFound)
				Assert.Ignore ("ping command not found.");

			string output = null;
			string error = null;
			TempFileCollection tfc = new TempFileCollection ();
			Assert.AreEqual (errcode, Executor.ExecWaitWithCapture (cmd, tfc, ref output, ref error), "ErrorCode");
			Assert.IsTrue (File.Exists (output), "output");
			Assert.IsTrue (output.StartsWith (temp), "output-path");
			Assert.IsTrue (File.Exists (error), "error");
			Assert.IsTrue (error.StartsWith (temp), "error-path");
			Assert.IsTrue (tfc.Count >= 2, "TempFileCollection");
		}

		[Test]
		public void ExecWaitWithCapture_CurrentDir ()
		{
			if (cmdNotFound)
				Assert.Ignore ("ping command not found.");

			string output = null;
			string error = null;
			TempFileCollection tfc = new TempFileCollection ();
			Assert.AreEqual (errcode, Executor.ExecWaitWithCapture (cmd, cd, tfc, ref output, ref error), "ErrorCode");
			Assert.IsTrue (File.Exists (output), "output");
			Assert.IsTrue (output.StartsWith (temp), "output-path");
			Assert.IsTrue (File.Exists (error), "error");
			Assert.IsTrue (error.StartsWith (temp), "error-path");
			// output/error file are relative to temp path
			Assert.IsTrue (tfc.Count >= 2, "TempFileCollection");
		}

		[Test]
		public void ExecWaitWithCapture_Token ()
		{
			if (cmdNotFound)
				Assert.Ignore ("ping command not found.");

			string output = null;
			string error = null;
			TempFileCollection tfc = new TempFileCollection ();
			Assert.AreEqual (errcode, Executor.ExecWaitWithCapture (token, cmd, tfc, ref output, ref error), "ErrorCode");
			Assert.IsTrue (File.Exists (output), "output");
			Assert.IsTrue (output.StartsWith (temp), "output-path");
			Assert.IsTrue (File.Exists (error), "error");
			Assert.IsTrue (error.StartsWith (temp), "error-path");
			Assert.IsTrue (tfc.Count >= 2, "TempFileCollection");
		}

		[Test]
		public void ExecWaitWithCapture_Token_CurrentDir ()
		{
			if (cmdNotFound)
				Assert.Ignore ("ping command not found.");

			string output = null;
			string error = null;
			TempFileCollection tfc = new TempFileCollection ();
			Assert.AreEqual (errcode, Executor.ExecWaitWithCapture (token, cmd, cd, tfc, ref output, ref error), "ErrorCode");
			Assert.IsTrue (File.Exists (output), "output");
			Assert.IsTrue (output.StartsWith (temp), "output-path");
			Assert.IsTrue (File.Exists (error), "error");
			Assert.IsTrue (error.StartsWith (temp), "error-path");
			// output/error file are relative to temp path
			Assert.IsTrue (tfc.Count >= 2, "TempFileCollection");
		}
	}
}
