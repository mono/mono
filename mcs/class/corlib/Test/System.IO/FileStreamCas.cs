//
// FileStreamCas.cs -CAS unit tests for System.IO.FileStream
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

namespace MonoCasTests.System.IO {

	[TestFixture]
	[Category ("CAS")]
	public class FileStreamCas {

		private MonoTests.System.IO.FileStreamTest fst;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// this occurs with a "clean" stack (full trust)
			fst  = new MonoTests.System.IO.FileStreamTest ();
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		// Partial Trust Tests - i.e. call "normal" unit with reduced privileges

		[Test]
		[ExpectedException (typeof (SecurityException))]
		public void PartialTrust_DenyUnrestricted_Failure ()
		{
			try {
				// SetUp/TearDown requires FileIOPermission
				fst.SetUp ();
				// so does the call but that's the test ;-)
				CallRestricted (fst);
			}
			finally {
				fst.TearDown ();
			}
		}

		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		private void CallRestricted (MonoTests.System.IO.FileStreamTest fst)
		{
			fst.TestDefaultProperties ();
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void PartialTrust_PermitOnly_FileIOPermission_Success ()
		{
			fst.SetUp ();
			fst.TestCtr ();
			fst.CtorAccess1Read2Read ();
			fst.Write ();
			fst.Length ();
			fst.Flush ();
			fst.TestDefaultProperties ();
			fst.TestLock ();
			fst.Seek ();
			fst.TestSeek ();
			fst.TestClose ();
			fst.PositionAfterSetLength ();
			fst.ReadBytePastEndOfStream ();
			fst.TearDown ();
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[SecurityPermission (SecurityAction.PermitOnly, UnmanagedCode = true)]
		public void PartialTrust_PermitOnly_FileIOPermissionUnmanagedCode_Success ()
		{
			fst.SetUp ();
			fst.TestFlushNotOwningHandle ();
			fst.TearDown ();
		}

		// test Demand by denying the required permissions

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Ctor_IntPtrFileAccess ()
		{
			new FileStream (IntPtr.Zero, FileAccess.Read);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Ctor_IntPtrFileAccessBool ()
		{
			new FileStream (IntPtr.Zero, FileAccess.Read, false);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Ctor_IntPtrFileAccessBoolInt ()
		{
			new FileStream (IntPtr.Zero, FileAccess.Read, false, 0);
		}

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Ctor_IntPtrFileAccessBoolIntBool ()
		{
			new FileStream (IntPtr.Zero, FileAccess.Read, false, 0, false);
		}

		// we use reflection to call FileStream as the Handle property is protected
		// by a LinkDemand (which will be converted into full demand, i.e. a stack 
		// walk) when reflection is used (i.e. it gets testable).

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Handle ()
		{
			FileStream fs = File.OpenWrite (Path.GetTempFileName ());
			try {
				MethodInfo mi = typeof (FileStream).GetProperty ("Handle").GetGetMethod ();
				mi.Invoke (fs, null);
			}
			finally {
				fs.Close ();
			}
		}
	}
}
