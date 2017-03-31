//
// IsolatedStorageFileStreamTest.cs 
//	- Unit Tests for abstract IsolatedStorageFileStream class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell Inc. (http://www.novell.com)
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
using System.IO.IsolatedStorage;
using Microsoft.Win32.SafeHandles;

using NUnit.Framework;

namespace MonoTests.System.IO.IsolatedStorageTest {

	[TestFixture]
	public class IsolatedStorageFileStreamTest {

		private void CheckCommonDetails (string prefix, IsolatedStorageFileStream isfs, bool read, bool write)
		{
			Assert.AreEqual (read, isfs.CanRead, prefix + ".CanRead");
			Assert.IsTrue (isfs.CanSeek, prefix + ".CanSeek");
			Assert.AreEqual (write, isfs.CanWrite, prefix + ".CanWrite");
			Assert.IsFalse (isfs.IsAsync, prefix + ".IsAsync");
			Assert.AreEqual (0, isfs.Length, prefix + ".Length");
			Assert.AreEqual ("[Unknown]", isfs.Name, prefix + ".Name");
			Assert.AreEqual (0, isfs.Position, prefix + ".Position");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_StringNullMode ()
		{
			IsolatedStorageFileStream isfs = new IsolatedStorageFileStream (null, FileMode.CreateNew);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))] // Mono's FileStream throw an ArgumentOutOfRangeException
		public void Constructor_StringModeBad ()
		{
			IsolatedStorageFileStream isfs = new IsolatedStorageFileStream ("badmode", (FileMode)Int32.MinValue);
		}

		[Test]
		public void Constructor_StringMode ()
		{
			string test = "string-filemode";
			using (var isfs = new IsolatedStorageFileStream (test, FileMode.Create)) {
				CheckCommonDetails (test, isfs, true, true);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Constructor_StringModeAccessBad ()
		{
			IsolatedStorageFileStream isfs = new IsolatedStorageFileStream ("badaccess", FileMode.Create, (FileAccess)Int32.MinValue);
		}

		[Test]
		public void Constructor_StringModeAccess ()
		{
			string test = "string-filemode-fileaccess";
			using (var isfs = new IsolatedStorageFileStream (test, FileMode.Create, FileAccess.ReadWrite)) {
				CheckCommonDetails (test, isfs, true, true);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Constructor_StringModeAccessShareBad ()
		{
			IsolatedStorageFileStream isfs = new IsolatedStorageFileStream ("badshare", FileMode.Create, FileAccess.Read, (FileShare)Int32.MinValue);
		}

		[Test]
		public void Constructor_StringModeAccessShare ()
		{
			string test = "string-filemode-fileaccess-fileshare";
			using (var isfs = new IsolatedStorageFileStream (test, FileMode.Create, FileAccess.Write, FileShare.Read)) {
				CheckCommonDetails (test, isfs, false, true);
			}
		}

		[Test]
		[ExpectedException (typeof (IsolatedStorageException))]
		public void Handle ()
		{
			using (var isfs = new IsolatedStorageFileStream ("handle", FileMode.Create)) {
				IntPtr p = isfs.Handle;
			}
		}

		[Test]
		public void RootPath ()
		{
			new IsolatedStorageFileStream ("/rootpath", FileMode.Create).Close ();
		}

		[Test]
		public void Constructor_StorageInvalid ()
		{
			IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly ();

			isf.Close ();
			try {
				new IsolatedStorageFileStream ("file", FileMode.Create, isf);
			} catch (InvalidOperationException) {
			}

			isf.Dispose ();
			try {
				new IsolatedStorageFileStream ("file", FileMode.Create, isf);
			} catch (InvalidOperationException) {
			}

			// Re-open and then remove the storage
			isf = IsolatedStorageFile.GetUserStoreForAssembly ();
			isf.Remove ();

			try {
				new IsolatedStorageFileStream ("file", FileMode.Create, isf);
			} catch (InvalidOperationException) {
			}
		}
	}
}
