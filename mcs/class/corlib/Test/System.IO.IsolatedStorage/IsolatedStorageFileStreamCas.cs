//
// IsolatedStorageFileStreamCas.cs - CAS unit tests for 
//	System.IO.IsolatedStorage.IsolatedStorageFileStream
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
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Threading;

#if NET_2_0
using Microsoft.Win32.SafeHandles;
#endif

namespace MonoCasTests.System.IO.IsolatedStorageTest {

	[TestFixture]
	[Category ("CAS")]
	public class IsolatedStorageFileStreamCas {

		private const int timeout = 30000;
		private string message;

		static ManualResetEvent reset;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			reset = new ManualResetEvent (false);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			reset.Close ();
		}

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void DenyUnrestricted ()
		{
			IsolatedStorageFileStream isfs = new IsolatedStorageFileStream ("deny", FileMode.Create);
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.Deny, Unrestricted = true)]
		[ExpectedException (typeof (SecurityException))]
		public void DenyIsolatedStorageFilePermission ()
		{
			IsolatedStorageFileStream isfs = new IsolatedStorageFileStream ("deny", FileMode.Create);
		}

		private void Read (string filename)
		{
			byte[] buffer = new byte[8];
			using (IsolatedStorageFileStream read = new IsolatedStorageFileStream (filename, FileMode.Open, FileAccess.Read)) {
				Assert.AreEqual (8, read.Length, "Length");
				Assert.AreEqual (0, read.Position, "Position");
				Assert.IsTrue (read.CanRead, "read.CanRead");
				Assert.IsTrue (read.CanSeek, "read.CanSeek");
				Assert.IsFalse (read.CanWrite, "read.CanWrite");
				Assert.IsFalse (read.IsAsync, "read.IsAync");
				Assert.AreEqual (buffer.Length, read.ReadByte (), "ReadByte");
				read.Seek (0, SeekOrigin.Begin);
				Assert.AreEqual (buffer.Length, read.Read (buffer, 0, buffer.Length), "Read");
				read.Close ();
			}
		}

		private void Write (string filename)
		{
			byte[] buffer = new byte[8];
			using (IsolatedStorageFileStream write = new IsolatedStorageFileStream (filename, FileMode.Create, FileAccess.Write)) {
				Assert.IsFalse (write.CanRead, "write.CanRead");
				Assert.IsTrue (write.CanSeek, "write.CanSeek");
				Assert.IsTrue (write.CanWrite, "write.CanWrite");
				Assert.IsFalse (write.IsAsync, "write.IsAync");
				write.Write (buffer, 0, buffer.Length);
				write.Position = 0;
				write.WriteByte ((byte)buffer.Length);
				write.SetLength (8);
				write.Flush ();
				write.Close ();
			}
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (FileNotFoundException))]
		public void ReadUnexistingFile ()
		{
			string filename = "cas-doesnt-exists";
			try {
				Read (filename);
			}
			catch (FileNotFoundException fnfe) {
				// check that we do not leak the full path to the missing file
				// as we do not have the FileIOPermission's PathDiscovery rights
				Assert.AreEqual (filename, fnfe.FileName, "FileName");
				throw;
			}
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (DirectoryNotFoundException))]
		public void ReadInUnexistingDirectory ()
		{
			string filename = Path.Combine ("unexistingdir", "filename");
			try {
				Read (filename);
			}
			catch (DirectoryNotFoundException dnf) {
				// check that we do not leak the full path to the missing file
				// as we do not have the FileIOPermission's PathDiscovery rights
				Assert.IsTrue (dnf.Message.IndexOf (filename) >= 0, "filename");
				Assert.IsFalse (dnf.Message.IndexOf ("\\" + filename) >= 0, "fullpath");
				throw;
			}
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (UnauthorizedAccessException))]
		public void ReadDirectoryAsFile ()
		{
			string dirname = "this-is-a-dir";
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain ();
			try {
				string[] dirs = isf.GetDirectoryNames (dirname);
				if (dirs.Length == 0) {
					isf.CreateDirectory (dirname);
				}
				Read (dirname);
			}
			catch (UnauthorizedAccessException uae) {
				// check that we do not leak the full path to the missing file
				// as we do not have the FileIOPermission's PathDiscovery rights
				Assert.IsTrue (uae.Message.IndexOf (dirname) >= 0, "dirname");
				Assert.IsFalse (uae.Message.IndexOf ("\\" + dirname) >= 0, "fullpath");
				try {
					isf.DeleteDirectory (dirname);
				}
				catch (IsolatedStorageException) {
					// this isn't where we want ot fail!
					// and 1.x isn't always cooperative
				}
				throw;
			}
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void ReWrite ()
		{
			Write ("cas-rewrite");
			Write ("cas-rewrite");
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void WriteThenRead ()
		{
			Write ("cas-rw");
			Read ("cas-rw");
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (DirectoryNotFoundException))]
		public void WriteInUnexistingDirectory ()
		{
			string filename = Path.Combine ("unexistingdir", "filename");
			try {
				Write (filename);
			}
			catch (DirectoryNotFoundException dnf) {
				// check that we do not leak the full path to the missing file
				// as we do not have the FileIOPermission's PathDiscovery rights
				Assert.IsTrue (dnf.Message.IndexOf (filename) >= 0, "filename");
				Assert.IsFalse (dnf.Message.IndexOf ("\\" + filename) >= 0, "fullpath");
				throw;
			}
		}

		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (IsolatedStorageException))]
		public void Handle ()
		{
			IsolatedStorageFileStream isfs = new IsolatedStorageFileStream ("cas-Handle", FileMode.Create);
			IntPtr p = isfs.Handle;
			// Note: The SecurityException for UnmanagedCode cannot be tested here because it's a LinkDemand
		}
#if NET_2_0
		[Test]
		[IsolatedStorageFilePermission (SecurityAction.PermitOnly, Unrestricted = true)]
		[ExpectedException (typeof (IsolatedStorageException))]
		public void SafeFileHandle ()
		{
			IsolatedStorageFileStream isfs = new IsolatedStorageFileStream ("cas-SafeFileHandle", FileMode.Create);
			SafeFileHandle sfh = isfs.SafeFileHandle;
			// Note: The SecurityException for UnmanagedCode cannot be tested here because it's a LinkDemand
		}
#endif

		// we use reflection to call IsolatedStorageFileStream as the Handle and SafeFileHandle
		// properties are protected by LinkDemand (which will be converted into full demand, 
		// i.e. a stack walk) when reflection is used (i.e. it gets testable).

		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void Handle_UnmanagedCode ()
		{
			IsolatedStorageFileStream isfs = new IsolatedStorageFileStream ("cas-Handle-Unmanaged", FileMode.Create);
			try {
				MethodInfo mi = typeof (IsolatedStorageFileStream).GetProperty ("Handle").GetGetMethod ();
				mi.Invoke (isfs, null);
			}
			finally {
				isfs.Close ();
			}
		}
#if NET_2_0
		[Test]
		[SecurityPermission (SecurityAction.Deny, UnmanagedCode = true)]
		[ExpectedException (typeof (SecurityException))]
		public void SafeFileHandle_UnmanagedCode ()
		{
			IsolatedStorageFileStream isfs = new IsolatedStorageFileStream ("cas-SafeFileHandle-Unmanaged", FileMode.Create);
			try {
				MethodInfo mi = typeof (IsolatedStorageFileStream).GetProperty ("SafeFileHandle").GetGetMethod ();
				mi.Invoke (isfs, null);
			}
			finally {
				isfs.Close ();
			}
		}
#endif

		// async tests (for stack propagation)

		private void ReadCallback (IAsyncResult ar)
		{
			IsolatedStorageFileStream s = (IsolatedStorageFileStream)ar.AsyncState;
			s.EndRead (ar);
			try {
				// can we do something bad here ?
				Assert.IsNotNull (Environment.GetEnvironmentVariable ("USERNAME"));
				message = "Expected a SecurityException";
			}
			catch (SecurityException) {
				message = null;
				reset.Set ();
			}
			catch (Exception e) {
				message = e.ToString ();
			}
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "USERNAME")]
		public void AsyncRead ()
		{
			IsolatedStorageFileStream isfs = new IsolatedStorageFileStream ("cas-AsyncRead", FileMode.Create);
			message = "AsyncRead";
			reset.Reset ();
			IAsyncResult r = isfs.BeginRead (new byte[0], 0, 0, new AsyncCallback (ReadCallback), isfs);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
			isfs.Close ();
		}

		private void WriteCallback (IAsyncResult ar)
		{
			IsolatedStorageFileStream s = (IsolatedStorageFileStream)ar.AsyncState;
			s.EndWrite (ar);
			try {
				// can we do something bad here ?
				Assert.IsNotNull (Environment.GetEnvironmentVariable ("USERNAME"));
				message = "Expected a SecurityException";
			}
			catch (SecurityException) {
				message = null;
				reset.Set ();
			}
			catch (Exception e) {
				message = e.ToString ();
			}
		}

		[Test]
		[EnvironmentPermission (SecurityAction.Deny, Read = "USERNAME")]
		public void AsyncWrite ()
		{
			IsolatedStorageFileStream isfs = new IsolatedStorageFileStream ("cas-AsyncWrite", FileMode.Create);
			message = "AsyncWrite";
			reset.Reset ();
			IAsyncResult r = isfs.BeginWrite (new byte[0], 0, 0, new AsyncCallback (WriteCallback), isfs);
			Assert.IsNotNull (r, "IAsyncResult");
			if (!reset.WaitOne (timeout, true))
				Assert.Ignore ("Timeout");
			Assert.IsNull (message, message);
			isfs.Close ();
		}
	}
}
