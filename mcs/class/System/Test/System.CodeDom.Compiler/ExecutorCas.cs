//
// ExecutorCas.cs 
//	- CAS unit tests for System.CodeDom.Compiler.Executor
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
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

using MonoTests.System.CodeDom.Compiler;

namespace MonoCasTests.System.CodeDom.Compiler {

	[TestFixture]
	[Category ("CAS")]
	public class ExecutorCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		private string cmd;
		private TempFileCollection tfc;
		private ExecutorTest unit;
		private MethodInfo execWaitWithCapture;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// at fulltrust
			tfc = new TempFileCollection ();
			cmd = "ping"; // available everywhere

			unit = new ExecutorTest ();
			unit.FixtureSetUp ();

			// for linkdemands tests
			MethodInfo[] methods = typeof (Executor).GetMethods ();
			foreach (MethodInfo mi in methods) {
				if ((mi.Name == "ExecWaitWithCapture") && (mi.GetParameters ().Length == 4)) {
					execWaitWithCapture = mi;
					break;
				}
			}
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void ReuseUnitTests_PermitOnly ()
		{
			unit.ExecWaitWithCapture ();
			unit.ExecWaitWithCapture_CurrentDir ();
			unit.ExecWaitWithCapture_Token ();
			unit.ExecWaitWithCapture_Token_CurrentDir ();
		}
		
		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ExecWaitWithCapture_Deny_FileIO ()
		{
			unit.ExecWaitWithCapture ();
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ExecWaitWithCapture_Deny_UnmanagedCode ()
		{
			unit.ExecWaitWithCapture ();
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ExecWaitWithCapture_Deny_Environment ()
		{
			unit.ExecWaitWithCapture ();
		}

		[Test]
		public void LinkDemand_No_Restriction ()
		{
			Assert.IsNotNull (execWaitWithCapture, "ExecWaitWithCapture");
			string output = null;
			string error = null;
			Assert.IsNotNull (execWaitWithCapture.Invoke (null, new object[4] { cmd, tfc, output, error }), "invoke");
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[EnvironmentPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_PermitOnly ()
		{
			Assert.IsNotNull (execWaitWithCapture, "ExecWaitWithCapture");
			string output = null;
			string error = null;
			Assert.IsNotNull (execWaitWithCapture.Invoke (null, new object[4] { cmd, tfc, output, error }), "invoke");
			// it's not enough, so there's a LinkDemand on the class
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_Deny_UnmanagedCode ()
		{
			// denying anything results in a non unrestricted permission set
			Assert.IsNotNull (execWaitWithCapture, "ExecWaitWithCapture");
			string output = null;
			string error = null;
			Assert.IsNotNull (execWaitWithCapture.Invoke (null, new object[4] { cmd, tfc, output, error }), "invoke");
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_Deny_FileIO ()
		{
			Assert.IsNotNull (execWaitWithCapture, "ExecWaitWithCapture");
			string output = null;
			string error = null;
			Assert.IsNotNull (execWaitWithCapture.Invoke (null, new object[4] { cmd, tfc, output, error }), "invoke");
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "Mono")]
		[ExpectedException (typeof (SecurityException))]
		public void LinkDemand_Deny_Environment ()
		{
			// denying anything results in a non unrestricted permission set
			Assert.IsNotNull (execWaitWithCapture, "ExecWaitWithCapture");
			string output = null;
			string error = null;
			Assert.IsNotNull (execWaitWithCapture.Invoke (null, new object[4] { cmd, tfc, output, error }), "invoke");
		}
	}
}
