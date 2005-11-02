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

namespace MonoTests.System.CodeDom.Compiler {

	[TestFixture]
	public class ExecutorTest {

		private bool posix;
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
			int platform = (int) Environment.OSVersion.Platform;
			posix = (platform == 4) || (platform == 128);
			errcode = (posix ? 2 : 1);
			cmd = "ping"; // available everywhere
			cd = Environment.CurrentDirectory;
			temp = Path.GetTempPath ();
			tfc = new TempFileCollection ();
			winid = WindowsIdentity.GetCurrent ();
			token = winid.Token;
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
			Executor.ExecWait (cmd, null);
		}

		[Test]
		[ExpectedException (typeof (ExternalException))]
		[Category ("NotWorking")]
		public void ExecWait ()
		{
			// why does it fail ? any case that works ?
			Executor.ExecWait (cmd, tfc);
		}

		[Test]
		public void ExecWaitWithCapture ()
		{
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
