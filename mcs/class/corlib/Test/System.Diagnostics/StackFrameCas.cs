//
// StackFrameCas.cs - CAS unit tests for System.Diagnostics.StackFrame
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
using System.Diagnostics;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace MonoCasTests.System.Diagnostics {

	[TestFixture]
	[Category ("CAS")]
	public class StackFrameCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		// avoid replication of tests on all constructors (this is no 
		// problem because the stack is already set correctly). The 
		// goal is to call every property and methods to see if they
		// have any* security requirements (*except for LinkDemand and
		// InheritanceDemand).
		private void Check (StackFrame sf, bool checkFile)
		{
			int cn = sf.GetFileColumnNumber ();
			int ln = sf.GetFileLineNumber ();
			int il = sf.GetILOffset ();
			int no = sf.GetNativeOffset ();

			Assert.IsNotNull (sf.GetMethod (), "GetMethod");

			if (checkFile) {
				string fn = sf.GetFileName ();
			}
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void StackFrame_DefaultConstructor ()
		{
			StackFrame sf = new StackFrame ();
			Check (sf, true);
		}
		
#if !RUN_ONDOTNET || NET_4_0 // Disabled because .net 2 fails to load dll with "Failure decoding embedded permission set object" due to "/" path
		[Test]
		[FileIOPermission (SecurityAction.Deny, PathDiscovery = "/")]
		[ExpectedException (typeof (SecurityException))]
		public void StackFrame_TrueConstructor_Fail ()
		{
			StackFrame sf = null;
			try {
				// ask for file informations
				sf = new StackFrame (true);
				Check (sf, false);
			}
			catch {
				Assert.Fail ("Didn't ask for file information");
			}
			// now look at the file informations...
			// note: only fails under 2.0
			Check (sf, true);
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, PathDiscovery = "/")]
		public void StackFrame_TrueConstructor_Pass ()
		{
			// ask file info
			StackFrame sf = new StackFrame (true);
			Check (sf, true);
		}
		
		[Test]
		[FileIOPermission (SecurityAction.Deny, PathDiscovery = "/")]
		[ExpectedException (typeof (SecurityException))]
		public void StackFrame_IntTrueConstructor_Fail ()
		{
			StackFrame sf = null;
			try {
				// ask for file informations
				sf = new StackFrame (0, true);
				Check (sf, false);
			}
			catch {
				Assert.Fail ("Didn't ask for file information");
			}
			// now look at the file informations...
			// note: only fails under 2.0
			Check (sf, true);
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, PathDiscovery = "/")]
		public void StackFrame_IntTrueConstructor_Pass ()
		{
			// ask file info
			StackFrame sf = new StackFrame (0, true);
			Check (sf, true);
		}
		
		[Test]
		[FileIOPermission (SecurityAction.Deny, PathDiscovery = "/")]
		[ExpectedException (typeof (SecurityException))]
		public void StackFrame_StringIntConstructor_Fail ()
		{
			StackFrame sf = null;
			try {
				// ask for file informations
				sf = new StackFrame ("mono.cs", 1);
				Check (sf, false);
			}
			catch {
				Assert.Fail ("Didn't ask for file information");
			}
			// now look at the file informations...
			// note: only fails under 2.0
			Check (sf, true);
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, PathDiscovery = "/")]
		public void StackFrame_StringIntConstructor_Pass ()
		{
			// supply file info
			StackFrame sf = new StackFrame ("mono.cs", 1);
			Check (sf, true);
		}

		[Test]
		[FileIOPermission (SecurityAction.Deny, PathDiscovery = "/")]
		[ExpectedException (typeof (SecurityException))]
		public void StackFrame_StringIntIntConstructor_Fail ()
		{
			StackFrame sf = null;
			try {
				// supply file info
				sf = new StackFrame ("mono.cs", 1, 1);
				Check (sf, false);
			}
			catch {
				Assert.Fail ("Didn't ask for file information");
			}
			// now look at the file informations...
			// note: only fails under 2.0
			Check (sf, true);
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, PathDiscovery = "/")]
		public void StackFrame_StringIntIntConstructor_Pass ()
		{
			// supply file info
			StackFrame sf = new StackFrame ("mono.cs", 1, 1);
			Check (sf, true);
		}		
#endif		

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void StackFrame_FalseConstructor ()
		{
			// DO NOT ask for file informations
			StackFrame sf = new StackFrame (false);
			Check (sf, true);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void StackFrame_IntConstructor ()
		{
			StackFrame sf = new StackFrame (1);
			Check (sf, true);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void StackFrame_IntFalseConstructor ()
		{
			// DO NOT ask for file informations
			StackFrame sf = new StackFrame (1, false);
			Check (sf, true);
		}
	}
}
