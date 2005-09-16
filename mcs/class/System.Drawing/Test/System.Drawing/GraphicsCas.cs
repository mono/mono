//
// GraphicsCas.cs - CAS unit tests for System.Drawing.Graphics
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
using System.Drawing;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace MonoCasTests.System.Drawing {

	[TestFixture]
	[Category ("CAS")]
	public class GraphicsCas {

		private MethodInfo fromHdcInternal;
		private MethodInfo fromHwndInternal;
		private MethodInfo releaseHdcInternal;
		private Bitmap bitmap;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// this executes at fulltrust
			fromHdcInternal = typeof (Graphics).GetMethod ("FromHdcInternal");
			fromHwndInternal = typeof (Graphics).GetMethod ("FromHwndInternal");
			releaseHdcInternal = typeof (Graphics).GetMethod ("ReleaseHdcInternal");
			bitmap = new Bitmap (10, 10);
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		private Graphics GetGraphics ()
		{
			return Graphics.FromImage (bitmap);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		public void FromHdcInternal ()
		{
			try {
				Graphics.FromHdcInternal (IntPtr.Zero);
			}
			catch (SecurityException) {
				Assert.Fail ("SecurityException");
			}
			catch (Exception) {
			}
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[Category ("NotWorking")]
		public void FromHwndInternal ()
		{
			try {
				Graphics.FromHwndInternal (IntPtr.Zero);
			}
			catch (SecurityException) {
				Assert.Fail ("SecurityException");
			}
			catch (Exception) {
			}
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		public void ReleaseHdcInternal ()
		{
			try {
				Graphics g = GetGraphics ();
				g.ReleaseHdcInternal (IntPtr.Zero);
			}
			catch (SecurityException) {
				Assert.Fail ("SecurityException");
			}
			catch (Exception) {
			}
		}

		// we use reflection to call Graphics as it's *Internal methods are 
		// protected by a LinkDemand (which will be converted into full demand, 
		// i.e. a stack walk) when reflection is used (i.e. it gets testable).

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void FromHdcInternal_LinkDemand ()
		{
			// requires FullTrust, so denying anything break the requirements
			Assert.IsNotNull (fromHdcInternal, "FromHdcInternal");
			fromHdcInternal.Invoke (null, new object[1] { IntPtr.Zero });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void FromHwndInternal_LinkDemand ()
		{
			// requires FullTrust, so denying anything break the requirements
			Assert.IsNotNull (fromHwndInternal, "FromHwndInternal");
			fromHwndInternal.Invoke (null, new object[1] { IntPtr.Zero });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ReleaseHdcInternal_LinkDemand ()
		{
			// requires FullTrust, so denying anything break the requirements
			Assert.IsNotNull (releaseHdcInternal, "ReleaseHdcInternal");
			Graphics g = GetGraphics ();
			releaseHdcInternal.Invoke (g, new object[1] { IntPtr.Zero });
		}
	}
}
