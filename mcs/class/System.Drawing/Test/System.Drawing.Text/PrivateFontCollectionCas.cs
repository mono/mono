//
// System.Drawing.Text.PrivateFontCollection CAS unit tests
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoCasTests.System.Drawing.Text {

	[TestFixture]
	[Category ("CAS")]
	public class PrivateFontCollectionCas {

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor ()
		{
			PrivateFontCollection pfc = new PrivateFontCollection ();
			Assert.IsNotNull (pfc.Families);
		}

		// TODO - tests for AddFontFile

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		public void AddMemoryFont_Deny_UnmanagedCode () 
		{
			Assert.Throws<SecurityException> (() => new PrivateFontCollection ().AddMemoryFont (IntPtr.Zero, 1024));
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void AddMemoryFont_PermitOnly_UnmanagedCode ()
		{
			Assert.Throws<ArgumentException> (() => new PrivateFontCollection ().AddMemoryFont (IntPtr.Zero, 1024));
		}

		// yes, that fails with FileNotFoundException ;-)

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void AddMemoryFont_NegativeLength ()
		{
			IntPtr ptr = Marshal.AllocHGlobal (1024);
			try {
				Assert.Throws<FileNotFoundException> (() => new PrivateFontCollection ().AddMemoryFont (ptr, -1024));
			}
			finally {
				Marshal.FreeHGlobal (ptr);
			}
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void AddMemoryFont_InvalidData ()
		{
			IntPtr ptr = Marshal.AllocHGlobal (1024);
			try {
				Assert.Throws<FileNotFoundException> (() => new PrivateFontCollection ().AddMemoryFont (ptr, 1024));
			}
			finally {
				Marshal.FreeHGlobal (ptr);
			}
		}
	}
}
