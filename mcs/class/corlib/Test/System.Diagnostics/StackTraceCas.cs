//
// StackTraceCas.cs - CAS unit tests for System.Diagnostics.StackTrace
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
	public class StackTraceCas {

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
		private void Check (StackTrace st)
		{
			if (st.FrameCount > 0)
				Assert.IsNotNull (st.GetFrame (0), "GetFrame");
			else
				Assert.IsNull (st.GetFrame (0), "GetFrame");
#if NET_2_0
			if (st.FrameCount > 0)
				Assert.IsNotNull (st.GetFrames (), "GetFrames");
			else
				Assert.IsNull (st.GetFrames (), "GetFrames");
#endif
			Assert.IsNotNull (st.ToString (), "ToString");
		}

		[Test]
#if NET_2_0
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
#else
		[ReflectionPermission (SecurityAction.Deny, TypeInformation = true)]
		[ExpectedException (typeof (SecurityException))]
#endif
		public void StackTrace_DefaultConstructor ()
		{
			StackTrace st = new StackTrace ();
			Check (st);
		}

		[Test]
#if NET_2_0
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
#else
		[ReflectionPermission (SecurityAction.Deny, TypeInformation = true)]
		[ExpectedException (typeof (SecurityException))]
#endif
		public void StackTrace_BoolConstructor ()
		{
			StackTrace st = new StackTrace (true);
			Check (st);
		}

		[Test]
#if NET_2_0
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
#else
		[ReflectionPermission (SecurityAction.Deny, TypeInformation = true)]
		[ExpectedException (typeof (SecurityException))]
#endif
		public void StackTrace_IntConstructor ()
		{
			StackTrace st = new StackTrace (1);
			Check (st);
		}

		[Test]
#if NET_2_0
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
#else
		[ReflectionPermission (SecurityAction.Deny, TypeInformation = true)]
		[ExpectedException (typeof (SecurityException))]
#endif
		public void StackTrace_IntBoolConstructor ()
		{
			StackTrace st = new StackTrace (1, true);
			Check (st);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void StackTrace_ExceptionConstructor ()
		{
			StackTrace st = new StackTrace (new Exception ());
			Check (st);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void StackTrace_ExceptionBoolConstructor ()
		{
			StackTrace st = new StackTrace (new Exception (), true);
			Check (st);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void StackTrace_ExceptionIntConstructor ()
		{
			StackTrace st = new StackTrace (new Exception (), 1);
			Check (st);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void StackTrace_ExceptionIntBoolConstructor ()
		{
			StackTrace st = new StackTrace (new Exception (), 1, true);
			Check (st);
		}

		[Test]
#if NET_2_0
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
#else
		[ReflectionPermission (SecurityAction.Deny, TypeInformation = true)]
		[ExpectedException (typeof (SecurityException))]
#endif
		public void StackTrace_StackFrameConstructor ()
		{
			StackTrace st = new StackTrace (new StackFrame ());
			Check (st);
		}

		[Test]
#if NET_2_0
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
#else
		[ReflectionPermission (SecurityAction.Deny, TypeInformation = true)]
		[ExpectedException (typeof (SecurityException))]
#endif
		[Category ("NotWorking")]
		public void StackTrace_ThreadBoolConstructor ()
		{
			StackTrace st = new StackTrace (Thread.CurrentThread, true);
			Check (st);
		}
	}
}
