//
// SecureStringTest.cs - Unit tests for System.Security.SecureString
//
// Author:
//      Sebastien Pouliot  <sebastien@ximian.com>
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

#if NET_2_0

using System;
using System.Security;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace MonoTests.System.Security {

	[TestFixture]
	public class SecureStringTest {

		private const string NotSupported = "Not supported before Windows 2000 Service Pack 3";

		[Test]
		public void DefaultConstructor ()
		{
			try {
				SecureString ss = new SecureString ();
				Assert.IsFalse (ss.IsReadOnly (), "IsReadOnly");
				Assert.AreEqual (0, ss.Length, "0");
				ss.AppendChar ('a');
				Assert.AreEqual (1, ss.Length, "1");
				ss.Clear ();
				Assert.AreEqual (0, ss.Length, "0b");
				ss.InsertAt (0, 'b');
				Assert.AreEqual (1, ss.Length, "1b");
				ss.SetAt (0, 'c');
				Assert.AreEqual (1, ss.Length, "1c");
				Assert.AreEqual ("System.Security.SecureString", ss.ToString (), "ToString");
				ss.RemoveAt (0);
				Assert.AreEqual (0, ss.Length, "0c");
				ss.Dispose ();
			}
			catch (NotSupportedException) {
				Assert.Ignore (NotSupported);
			}
		}
#if !TARGET_JVM
		[Test]
		public unsafe void UnsafeConstructor ()
		{
			try {
				SecureString ss = null;
				char[] data = new char[] { 'a', 'b', 'c' };
				fixed (char* p = &data[0]) {
					ss = new SecureString (p, data.Length);
				}
				Assert.IsFalse (ss.IsReadOnly (), "IsReadOnly");
				Assert.AreEqual (3, ss.Length, "3");
				ss.AppendChar ('a');
				Assert.AreEqual (4, ss.Length, "4");
				ss.Clear ();
				Assert.AreEqual (0, ss.Length, "0b");
				ss.InsertAt (0, 'b');
				Assert.AreEqual (1, ss.Length, "1b");
				ss.SetAt (0, 'c');
				Assert.AreEqual (1, ss.Length, "1c");
				ss.RemoveAt (0);
				Assert.AreEqual (0, ss.Length, "0c");
				ss.Dispose ();
			}
			catch (NotSupportedException) {
				Assert.Ignore (NotSupported);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public unsafe void UnsafeConstructor_Null ()
		{
			new SecureString (null, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public unsafe void UnsafeConstructor_Negative ()
		{
			char[] data = new char[] { 'a', 'b', 'c' };
			fixed (char* p = &data[0]) {
				new SecureString (p, -1);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public unsafe void UnsafeConstructor_BiggerThanMax ()
		{
			char[] data = new char[] { 'a', 'b', 'c' };
			fixed (char* p = &data[0]) {
				new SecureString (p, UInt16.MaxValue + 2);
			}
		}

		private SecureString max;

		private unsafe SecureString GetMaxLength ()
		{
			if (max == null) {
				int maxlength =  UInt16.MaxValue + 1;
				char[] data = new char[maxlength];
				fixed (char* p = &data[0]) {
					max = new SecureString (p, maxlength);
				}
				// note: don't try a loop of AppendChar with that size ;-)
			}
			return max;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void AppendChar_BiggerThanMax ()
		{
			SecureString ss = GetMaxLength ();
			ss.AppendChar ('a');
		}
#endif
		[Test]
		public void Copy_Empty ()
		{
			SecureString empty = new SecureString ();
			Assert.AreEqual (0, empty.Length, "Empty.Length");
			SecureString empty_copy = empty.Copy ();
			Assert.AreEqual (0, empty_copy.Length, "EmptyCopy.Length");
		}

		[Test]
		public void Copy ()
		{
	                SecureString ss = new SecureString ();
        	        ss.AppendChar ('a');
			Assert.AreEqual (1, ss.Length, "Length");

	                SecureString ss2 = ss.Copy();
			Assert.AreEqual (1, ss2.Length, "Copy.Length");
			Assert.IsFalse (ss2.IsReadOnly (), "Copy.IsReadOnly");
			ss2.MakeReadOnly ();
			Assert.IsTrue (ss2.IsReadOnly (), "Copy.IsReadOnly-2");

			SecureString ss3 = ss2.Copy ();
			Assert.IsFalse (ss3.IsReadOnly (), "Copy.IsReadOnly-3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void InsertAt_Negative ()
		{
			SecureString ss = new SecureString ();
			ss.InsertAt (-1, 'a');
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void InsertAt_BiggerThanLength ()
		{
			SecureString ss = new SecureString ();
			ss.InsertAt (1, 'a');
		}

		[Test]
		public void InsertAt_UsedLikeAppendChar () // #350820
		{
			SecureString ss = new SecureString ();
			ss.AppendChar ('T');
			Assert.AreEqual (1, ss.Length, "AppendChar");
			ss.InsertAt (1, 'e');
			Assert.AreEqual (2, ss.Length, "InsertAt");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SetAt_Negative ()
		{
			SecureString ss = new SecureString ();
			ss.SetAt (-1, 'a');
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SetAt_BiggerThanLength ()
		{
			SecureString ss = new SecureString ();
			ss.SetAt (1, 'a');
		}

		[Test]
		public void RemoveAt ()
		{
			string test_string = "test string";
			string expected, actual;
			SecureString ss = new SecureString ();
			foreach (char c in test_string) {
				ss.AppendChar (c);
			}

			ss.RemoveAt (0);
			expected = "est string";
			actual = ReadSecureString (ss);
			Assert.AreEqual (expected, actual, "RemoveAt begining");

			ss.RemoveAt (4);
			expected = "est tring";
			actual = ReadSecureString (ss);
			Assert.AreEqual (expected, actual, "RemoveAt middle");

			ss.RemoveAt (8);
			expected = "est trin";
			actual = ReadSecureString (ss);
			Assert.AreEqual (expected, actual, "RemoveAt end");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RemoveAt_Negative ()
		{
			SecureString ss = new SecureString ();
			ss.RemoveAt (-1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RemoveAt_BiggerThanLength ()
		{
			SecureString ss = new SecureString ();
			ss.RemoveAt (1);
		}
#if !TARGET_JVM
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void InsertAt_BiggerThanMax ()
		{
			SecureString ss = GetMaxLength ();
			ss.InsertAt (ss.Length, 'a');
		}
#endif
		private SecureString GetReadOnly ()
		{
			SecureString ss = new SecureString ();
			ss.MakeReadOnly ();
			return ss;
		}

		[Test]
		public void ReadOnly ()
		{
			try {
				SecureString ss = GetReadOnly ();
				Assert.IsTrue (ss.IsReadOnly (), "IsReadOnly");
				Assert.AreEqual (0, ss.Length, "0");
				ss.Dispose ();
			}
			catch (NotSupportedException) {
				Assert.Ignore (NotSupported);
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReadOnly_AppendChar ()
		{
			SecureString ss = GetReadOnly ();
			ss.AppendChar ('a');
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReadOnly_Clear ()
		{
			SecureString ss = GetReadOnly ();
			ss.Clear ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReadOnly_InsertAt ()
		{
			SecureString ss = GetReadOnly ();
			ss.InsertAt (0, 'a');
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReadOnly_SetAt ()
		{
			SecureString ss = GetReadOnly ();
			ss.SetAt (0, 'a');
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ReadOnly_RemoveAt ()
		{
			SecureString ss = GetReadOnly ();
			ss.RemoveAt (0);
		}

		private SecureString GetDisposed ()
		{
			SecureString ss = new SecureString ();
			ss.Dispose ();
			return ss;
		}

		[Test]
		public void Disposed ()
		{
			try {
				SecureString ss = GetDisposed ();
				ss.Dispose ();
			}
			catch (NotSupportedException) {
				Assert.Ignore (NotSupported);
			}
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed_AppendChar ()
		{
			SecureString ss = GetDisposed ();
			ss.AppendChar ('a');
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed_Clear ()
		{
			SecureString ss = GetDisposed ();
			ss.Clear ();
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed_InsertAt ()
		{
			SecureString ss = GetDisposed ();
			ss.InsertAt (0, 'a');
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed_IsReadOnly ()
		{
			SecureString ss = GetDisposed ();
			Assert.IsFalse (ss.IsReadOnly (), "IsReadOnly");
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed_Length ()
		{
			SecureString ss = GetDisposed ();
			Assert.AreEqual (0, ss.Length, "Length");
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed_SetAt ()
		{
			SecureString ss = GetDisposed ();
			ss.SetAt (0, 'a');
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Disposed_RemoveAt ()
		{
			SecureString ss = GetDisposed ();
			ss.RemoveAt (0);
		}

		// helper function
		private static string ReadSecureString(SecureString aSecureString)
		{
			var strPtr = Marshal.SecureStringToGlobalAllocUnicode (aSecureString);
			var str = Marshal.PtrToStringUni(strPtr);
			Marshal.ZeroFreeGlobalAllocUnicode (strPtr);
			return str;
		}
	}
}

#endif
