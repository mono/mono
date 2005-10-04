//
// BitmapCas.cs - CAS unit tests for System.Drawing.Bitmap
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
	public class BitmapCas {

		private MethodInfo getHbitmap1;
		private MethodInfo getHbitmap2;
		private MethodInfo getHicon;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// this executes at fulltrust
			getHbitmap1 = typeof (Bitmap).GetMethod ("GetHbitmap", new Type[0]);
			getHbitmap2 = typeof (Bitmap).GetMethod ("GetHbitmap", new Type[1] { typeof (Color) });
			getHicon = typeof (Bitmap).GetMethod ("GetHicon");
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		public void GetHbitmap ()
		{
			Bitmap b = new Bitmap (10, 10);
			try {
				Assert.IsTrue (b.GetHbitmap () != IntPtr.Zero, "GetHbitmap");
			}
			catch (NotImplementedException) {
				// not available on Mono
			}
			try {
				Assert.IsTrue (b.GetHbitmap (Color.Aqua) != IntPtr.Zero, "GetHbitmap(Color)");
			}
			catch (NotImplementedException) {
				// not available on Mono
			}
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		public void GetHicon ()
		{
			Bitmap b = new Bitmap (10, 10);
			try {
				Assert.IsTrue (b.GetHicon () != IntPtr.Zero, "GetHicon");
			}
			catch (NotImplementedException) {
				// not available on Mono
			}
		}

		// we use reflection to call Bitmap as it's GetHbitmap and GetHicon methods
		// are protected by a LinkDemand (which will be converted into full demand, 
		// i.e. a stack walk) when reflection is used (i.e. it gets testable).

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetHbitmap_Empty_LinkDemand ()
		{
			// requires FullTrust, so denying anything break the requirements
			Assert.IsNotNull (getHbitmap1, "GetHbitmap");
			Bitmap b = new Bitmap (10, 10);
			getHbitmap1.Invoke (b, null);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetHbitmap_Color_LinkDemand ()
		{
			// requires FullTrust, so denying anything break the requirements
			Assert.IsNotNull (getHbitmap2, "GetHbitmap(Color)");
			Bitmap b = new Bitmap (10, 10);
			getHbitmap2.Invoke (b, new object[1] { Color.Aqua });
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void GetHicon_LinkDemand ()
		{
			// requires FullTrust, so denying anything break the requirements
			Assert.IsNotNull (getHicon, "GetHicon");
			Bitmap b = new Bitmap (10, 10);
			getHicon.Invoke (b, null);
		}
	}
}
