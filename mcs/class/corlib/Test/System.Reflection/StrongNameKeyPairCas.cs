//
// StrongNameKeyPairCas.cs - CAS unit tests for System.Reflection.StrongNameKeyPair
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
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

using MonoTests.System.Reflection;

namespace MonoCasTests.System.Reflection {

	[TestFixture]
	[Category ("CAS")]
	public class StrongNameKeyPairCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		// Partial Trust Tests

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CtorByteArray_Deny_Unrestricted ()
		{
			new StrongNameKeyPair ((byte[])null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CtorFileStream_Deny_Unrestricted ()
		{
			new StrongNameKeyPair ((FileStream)null);
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void CtorKeyContainer_Deny_Unrestricted ()
		{
			new StrongNameKeyPair ((string)null);
		}

		[Test]
		public void PublicKey_Deny_Unrestricted ()
		{
			PublicKey (new StrongNameKeyPair (StrongNameKeyPairTest.GetKey ()));
		}

		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		private void PublicKey (StrongNameKeyPair snkp)
		{
			Assert.IsNotNull (snkp.PublicKey, "PublicKey");
		}

		// Partial Trust - Working (minimal permissions)

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void CtorByteArray_PermitOnly_UnmanagedCode ()
		{
			StrongNameKeyPairTest snkpt = new StrongNameKeyPairTest ();
			snkpt.ConstructorByteArray ();
			snkpt.ConstructorECMAByteArray ();
		}

		[Test]
		public void CtorFileStream_PermitOnly_UnmanagedCodeFileIOPermission ()
		{
			StrongNameKeyPairTest snkpt = new StrongNameKeyPairTest ();
			FileStream fs = null;
			try {
				snkpt.SetUp ();
				string filename = snkpt.CreateSnkFile ();
				fs = new FileStream (filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				// we needed too much permissions before calling the 
				// interesting part, so the test is splitted in two
				CtorFileStream (fs);
			}
			finally {
				if (fs != null)
					fs.Close ();
				snkpt.TearDown ();
			}
		}

		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		private void CtorFileStream (FileStream fs)
		{
			new StrongNameKeyPair (fs);
		}

		[Test]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorKeyContainer_PermitOnly_UnmanagedCode () 
		{
			StrongNameKeyPairTest snkpt = new StrongNameKeyPairTest ();
			snkpt.ConstructorNullString ();
		}
	}
}
