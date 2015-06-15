//
// SecurityContextCas.cs - CAS Unit Tests for SecurityContext
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
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
#if FEATURE_COMPRESSEDSTACK

using System;
using System.Security;
using System.Security.Permissions;
using System.Threading;

using NUnit.Framework;

namespace MonoCasTests.System.Security {

	[TestFixture]
	[Category ("CAS")]
	public class SecurityContextCas {

		static bool success;

		static void Callback (object o)
		{
			new SecurityPermission (SecurityPermissionFlag.UnmanagedCode).Demand ();
			success = (bool)o;
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager isn't enabled");

			success = false;
		}

		[TearDown]
		public void TearDown ()
		{
			if (SecurityContext.IsFlowSuppressed () || SecurityContext.IsWindowsIdentityFlowSuppressed ())
				SecurityContext.RestoreFlow ();
		}

		private void Thread_Run_Empty ()
		{
			Assert.IsFalse (success, "pre-check");
			SecurityContext.Run (SecurityContext.Capture (), new ContextCallback (Callback), true);
			Assert.IsTrue (success, "post-check");
		}

		[Test]
		public void Run_Empty ()
		{
			Thread t = new Thread (new ThreadStart (Thread_Run_Empty));
			t.Start ();
			t.Join ();
		}

		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		private SecurityContext GetSecurityContextUnmanaged ()
		{
			return SecurityContext.Capture ();
			// the Deny disappears with this stack frame but we got a capture of it
		}

		private void Thread_Run_UnmanagedCode ()
		{
			bool result = false;
			Assert.IsFalse (success, "pre-check");
			try {
				SecurityContext sc = GetSecurityContextUnmanaged ();
				// run with the captured security stack (i.e. deny unmanaged)
				SecurityContext.Run (sc, new ContextCallback (Callback), true);
			}
			catch (SecurityException) {
				result = true;
			}
			finally {
				Assert.IsFalse (success, "post-check");
				Assert.IsTrue (result, "Result");
			}
		}

		[Test]
		public void Run_UnmanagedCode ()
		{
			Thread t = new Thread (new ThreadStart (Thread_Run_UnmanagedCode));
			t.Start ();
			t.Join ();
		}

		private void Thread_Run_UnmanagedCode_SuppressFlow_BeforeCapture ()
		{
			bool result = false;
			Assert.IsFalse (success, "pre-check");
			AsyncFlowControl afc = SecurityContext.SuppressFlow ();
			try {
				SecurityContext sc = GetSecurityContextUnmanaged ();
				SecurityContext.Run (sc, new ContextCallback (Callback), true);
			}
			catch (InvalidOperationException) {
				result = true;
			}
			finally {
				Assert.IsFalse (success, "post-check");
				afc.Undo ();
				Assert.IsTrue (result, "Result");
			}
		}

		[Test]
		public void Run_UnmanagedCode_SuppressFlow_BeforeCapture ()
		{
			Thread t = new Thread (new ThreadStart (Thread_Run_UnmanagedCode_SuppressFlow_BeforeCapture));
			t.Start ();
			t.Join ();
		}

		private void Thread_Run_UnmanagedCode_SuppressFlow_AfterCapture ()
		{
			bool result = false;
			Assert.IsFalse (success, "pre-check");
			SecurityContext sc = GetSecurityContextUnmanaged ();
			AsyncFlowControl afc = SecurityContext.SuppressFlow ();
			try {
				SecurityContext.Run (sc, new ContextCallback (Callback), true);
			}
			catch (SecurityException) {
				result = true;
			}
			finally	{
				Assert.IsFalse (success, "post-check");
				afc.Undo ();
				Assert.IsTrue (result, "Result");
			}
		}

		[Test]
		public void Run_UnmanagedCode_SuppressFlow_AfterCapture ()
		{
			Thread t = new Thread (new ThreadStart (Thread_Run_UnmanagedCode_SuppressFlow_AfterCapture));
			t.Start ();
			t.Join ();
		}
	}
}

#endif