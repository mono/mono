//
// ThreadCas.cs - CAS unit tests for System.Threading.Thread
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
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace MonoCasTests.System.Threading {

	[TestFixture]
	[Category ("CAS")]
	public class ThreadCas {

		private Type ThreadType;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			ThreadType = typeof (Thread);
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		// Partial Trust Tests - i.e. call "normal" unit with reduced privileges

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void PartialTrust_DenyUnrestricted_Success ()
		{
			MonoTests.System.Threading.ThreadTest tt = new MonoTests.System.Threading.ThreadTest ();
			tt.TestCtor1 ();
			// most tests use Abort so there's not much to call
		}		

		// test Demand by denying the caller of the required privileges

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlThread = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Abort ()
		{
			Thread.CurrentThread.Abort ();
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlThread = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Abort_Object ()
		{
			Thread.CurrentThread.Abort (new object [0]);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlThread = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CurrentCulture ()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlThread = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Interrupt ()
		{
			Thread.CurrentThread.Interrupt ();
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlThread = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ResetAbort ()
		{
			Thread.ResetAbort ();
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlThread = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Resume ()
		{
			Thread.CurrentThread.Resume ();
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, ControlThread = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Suspend ()
		{
			Thread.CurrentThread.Suspend ();
		}

		// we use reflection to call Mutex as it's named constructors are protected by
		// a LinkDemand (which will be converted into full demand, i.e. a stack walk) 
		// when reflection is used (i.e. it gets testable).

		[Test]
		[SecurityPermission (SecurityAction.Deny, Infrastructure = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CurrentContext ()
		{
			MethodInfo mi = ThreadType.GetProperty ("CurrentContext").GetGetMethod ();
			Assert.IsNotNull (mi, "get_CurrentContext");
			mi.Invoke (null, null);
		}
	}
}
