//
// WaitHandleCas.cs - CAS unit tests for System.Threading.WaitHandle
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
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace MonoCasTests.System.Threading {

	// "test" inheritance - this will (or should) always works as we have
	// UnmanagedCode permission when loading the assembly. So we're proving
	// this works - not proving when it shouldn't works :-/

	public class NonAbstractWaitHandle : WaitHandle {
	}

	public class CasWaitHandle : WaitHandle {
		public override IntPtr Handle {
			get { return base.Handle; }
			set { base.Handle = value; }
		}
	}

	[TestFixture]
	[Category ("CAS")]
	public class WaitHandleCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		// we use reflection to call WaitHandle as the Handle property is protected by
		// a LinkDemand (which will be converted into full demand, i.e. a stack walk) 
		// when reflection is used (i.e. it gets testable).

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void WaitHandle_Handle ()
		{
			MethodInfo mi = typeof (NonAbstractWaitHandle).GetProperty ("Handle").GetSetMethod ();
			NonAbstractWaitHandle wh = new NonAbstractWaitHandle ();
			mi.Invoke (wh, new object [1] { IntPtr.Zero });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		public void CasWaitHandle_Handle ()
		{
			MethodInfo mi = typeof (CasWaitHandle).GetProperty ("Handle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).GetSetMethod ();
			CasWaitHandle cwh = new CasWaitHandle ();
			mi.Invoke (cwh, new object [1] { IntPtr.Zero });
			// note: this works because CasWaitHandle doesn't have a LinkDemand on Handle
			// and yes this is a reason why LinkDemand are dangereous
		}
	}
}
