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
	public class RegionCas {

		private MethodInfo fromHdcInternal;
		private MethodInfo fromHwndInternal;
		private MethodInfo releaseHdcInternal;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// this executes at fulltrust
			fromHdcInternal = typeof (Graphics).GetMethod ("FromHdcInternal");
			fromHwndInternal = typeof (Graphics).GetMethod ("FromHwndInternal");
			releaseHdcInternal = typeof (Graphics).GetMethod ("ReleaseHdcInternal");
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void FromHrgn_Deny_UnmanagedCode ()
		{
			Region.FromHrgn (IntPtr.Zero);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[ExpectedException (typeof (ArgumentException))]
		public void FromHrgn_PermitOnly_UnmanagedCode ()
		{
			Region.FromHrgn (IntPtr.Zero);
		}
#if NET_2_0
		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void ReleaseHrgn_Deny_UnmanagedCode ()
		{
			new Region ().ReleaseHrgn (IntPtr.Zero);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ReleaseHrgn_PermitOnly_UnmanagedCode ()
		{
			new Region ().ReleaseHrgn (IntPtr.Zero);
		}
#endif
	}
}
