//
// SecurityContextTest.cs - NUnit tests for SecurityContext
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

#if !MOBILE

using System;
using System.Security;
using System.Security.Principal;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Security {

	[TestFixture]
	public class SecurityContextTest {

		static bool success;

		static void Callback (object o)
		{
			success = (bool) o;
		}

		[SetUp]
		public void SetUp ()
		{
			success = false;
		}

		[TearDown]
		public void TearDown ()
		{
			if (SecurityContext.IsFlowSuppressed () || SecurityContext.IsWindowsIdentityFlowSuppressed ())
				SecurityContext.RestoreFlow ();
		}

		[Test]
		public void Capture ()
		{
			SecurityContext sc = SecurityContext.Capture ();
			Assert.IsNotNull (sc, "Capture");

			AsyncFlowControl afc = SecurityContext.SuppressFlow ();
			Assert.IsTrue (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-1");
			try {
				sc = SecurityContext.Capture ();
				Assert.IsNull (sc, "Capture with SuppressFlow");
			}
			finally {
				afc.Undo ();
			}

			afc = SecurityContext.SuppressFlowWindowsIdentity ();
			Assert.IsTrue (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-1");
			try {
				sc = SecurityContext.Capture ();
				Assert.IsNotNull (sc, "Capture with SuppressFlowWindowsIdentity");
			}
			finally {
				afc.Undo ();
			}
		}

		[Test]
		public void Copy ()
		{
			SecurityContext sc = SecurityContext.Capture ();
			Assert.IsNotNull (sc, "Capture");

			SecurityContext copy = sc.CreateCopy ();
			Assert.IsNotNull (copy, "Copy of Capture");

			Assert.IsFalse (sc.Equals (copy));
			Assert.IsFalse (copy.Equals (sc));
			Assert.IsFalse (Object.ReferenceEquals (sc, copy));

			SecurityContext copy2nd = copy.CreateCopy ();
			Assert.IsNotNull (copy2nd, "2nd level copy of Capture");
		}

		[Test]
		public void IsFlowSuppressed ()
		{
			Assert.IsFalse (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-1");

			AsyncFlowControl afc = SecurityContext.SuppressFlowWindowsIdentity ();
			Assert.IsFalse (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-2");
			afc.Undo ();

			afc = SecurityContext.SuppressFlow ();
			Assert.IsTrue (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-3");
			afc.Undo ();

			Assert.IsFalse (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-4");
		}

		[Test]
		public void IsWindowsIdentityFlowSuppressed ()
		{
			Assert.IsFalse (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-1");

			AsyncFlowControl afc = SecurityContext.SuppressFlow ();
			Assert.IsTrue (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-2");
			afc.Undo ();

			afc = SecurityContext.SuppressFlowWindowsIdentity ();
			Assert.IsTrue (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-3");
			afc.Undo ();

			Assert.IsFalse (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-4");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void RestoreFlow_None ()
		{
			SecurityContext.RestoreFlow ();
		}

		[Test]
		public void RestoreFlow_SuppressFlow ()
		{
			Assert.IsFalse (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-1");
			SecurityContext.SuppressFlow ();
			Assert.IsTrue (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-2");
			SecurityContext.RestoreFlow ();
			Assert.IsFalse (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-3");
		}

		[Test]
		public void RestoreFlow_SuppressFlowWindowsIdentity ()
		{
			Assert.IsFalse (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-1");
			SecurityContext.SuppressFlowWindowsIdentity ();
			Assert.IsTrue (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-2");
			SecurityContext.RestoreFlow ();
			Assert.IsFalse (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-3");
		}

		[Test]
		public void Run ()
		{
			Assert.IsFalse (success, "pre-check");
			SecurityContext.Run (SecurityContext.Capture (), new ContextCallback (Callback), true);
			Assert.IsTrue (success, "post-check");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Run_SuppressFlow ()
		{
			Assert.IsFalse (SecurityContext.IsFlowSuppressed ());
			AsyncFlowControl afc = SecurityContext.SuppressFlow ();
			Assert.IsTrue (SecurityContext.IsFlowSuppressed ());
			try {
				SecurityContext.Run (SecurityContext.Capture (), new ContextCallback (Callback), "Hello world.");
			}
			finally {
				afc.Undo ();
			}
		}

		[Test]
		public void SuppressFlow ()
		{
			Assert.IsFalse (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-1");
			Assert.IsFalse (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-1");

			AsyncFlowControl afc = SecurityContext.SuppressFlow ();
			Assert.IsTrue (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-3");
			Assert.IsTrue (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-3");
			afc.Undo ();

			Assert.IsFalse (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-4");
			Assert.IsFalse (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-4");
		}

		[Test]
		public void SuppressFlowWindowsIdentity ()
		{
			Assert.IsFalse (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-1");
			Assert.IsFalse (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-1");

			AsyncFlowControl afc = SecurityContext.SuppressFlowWindowsIdentity ();
			Assert.IsFalse (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-2");
			Assert.IsTrue (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-2");
			afc.Undo ();

			Assert.IsFalse (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-4");
			Assert.IsFalse (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-4");
		}

		[Test]
		public void SuppressFlow_Both ()
		{
			Assert.IsFalse (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-1");
			Assert.IsFalse (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-1");

			AsyncFlowControl afc = SecurityContext.SuppressFlowWindowsIdentity ();
			Assert.IsFalse (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-2");
			Assert.IsTrue (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-2");

			AsyncFlowControl afc2 = SecurityContext.SuppressFlow ();
			Assert.IsTrue (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-2");
			Assert.IsTrue (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-2");
			afc2.Undo ();

			// note: afc2 Undo return to the original (not the previous) state
			Assert.IsFalse (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-2");
			Assert.IsFalse (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void SuppressFlow_Both_Undo ()
		{
			Assert.IsFalse (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-1");
			Assert.IsFalse (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-1");

			AsyncFlowControl afc = SecurityContext.SuppressFlowWindowsIdentity ();
			Assert.IsFalse (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-2");
			Assert.IsTrue (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-2");

			AsyncFlowControl afc2 = SecurityContext.SuppressFlow ();
			Assert.IsTrue (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-2");
			Assert.IsTrue (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-2");
			afc2.Undo ();

			// note: afc2 Undo return to the original (not the previous) state
			Assert.IsFalse (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-2");
			Assert.IsFalse (SecurityContext.IsWindowsIdentityFlowSuppressed (), "IsWindowsIdentityFlowSuppressed-2");

			// we can't use the first AsyncFlowControl
			afc.Undo ();
		}

		// effects of ExecutionContext on SecurityContext

		[Test]
		public void ExecutionContext_SuppressFlow ()
		{
			Assert.IsFalse (ExecutionContext.IsFlowSuppressed (), "IsFlowSuppressed-1");
			AsyncFlowControl afc = ExecutionContext.SuppressFlow ();
			try {
				Assert.IsTrue (ExecutionContext.IsFlowSuppressed (), "IsFlowSuppressed-2");
				Assert.IsFalse (SecurityContext.IsFlowSuppressed (), "IsFlowSuppressed-3");
				Assert.IsNotNull (SecurityContext.Capture (), "Capture");
			}
			finally {
				afc.Undo ();
			}
		}

		[Test]
		public void WindowsIdentity_ ()
		{
			Thread.CurrentPrincipal = new WindowsPrincipal (WindowsIdentity.GetCurrent ());
			SecurityContext sc = SecurityContext.Capture ();
			Assert.IsNotNull (sc, "Capture");
		}
	}
}

#endif