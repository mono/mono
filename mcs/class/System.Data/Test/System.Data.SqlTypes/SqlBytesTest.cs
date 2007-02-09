//
// SqlBytesTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlBytes
//
// Authors:
//   Nagappan A (anagappan@novell.com)
//
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

#if NET_2_0

using NUnit.Framework;
using System;
using System.IO;
using System.Data.SqlTypes;
using System.Threading;
using System.Globalization;

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
        public class SqlBytesTest
	{
		[SetUp]
		public void SetUp ()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
		}

		// Test constructor
		[Test]
		public void SqlBytesItem ()
		{
			SqlBytes bytes = new SqlBytes ();
			try {
				Assert.AreEqual (bytes [0], 0, "#1 Should throw SqlNullValueException");
				Assert.Fail ("Should throw SqlNullValueException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "Should throw SqlNullValueException");
			}
			byte [] b = null;
			bytes = new SqlBytes (b);
			try {
				Assert.AreEqual (bytes [0], 0, "#2 Should throw SqlNullValueException");
				Assert.Fail ("Should throw SqlNullValueException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "Should throw SqlNullValueException");
			}
			b = new byte [10];
			bytes = new SqlBytes (b);
			Assert.AreEqual (bytes [0], 0, "");
			try {
				Assert.AreEqual (bytes [-1], 0, "");
				Assert.Fail ("Should throw ArgumentOutOfRangeException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "Should throw ArgumentOutOfRangeException");
			}
			try {
				Assert.AreEqual (bytes [10], 0, "");
				Assert.Fail ("Should throw ArgumentOutOfRangeException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "Should throw ArgumentOutOfRangeException");
			}
		}
		[Test]
		public void SqlBytesLength ()
		{
			byte [] b = null;
			SqlBytes bytes = new SqlBytes ();
			try {
				Assert.AreEqual (bytes.Length, 0, "#1 Should throw SqlNullValueException");
				Assert.Fail ("Should throw SqlNullValueException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "Should throw SqlNullValueException");
			}
			bytes = new SqlBytes (b);
			try {
				Assert.AreEqual (bytes.Length, 0, "#2 Should throw SqlNullValueException");
				Assert.Fail ("Should throw SqlNullValueException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "Should throw SqlNullValueException");
			}
			b = new byte [10];
			bytes = new SqlBytes (b);
			Assert.AreEqual (bytes.Length, 10, "#3 Should be 10");
		}
		[Test]
		public void SqlBytesMaxLength ()
		{
			byte [] b = null;
			SqlBytes bytes = new SqlBytes ();
			Assert.AreEqual (bytes.MaxLength, -1, "#1 Should return -1");
			bytes = new SqlBytes (b);
			Assert.AreEqual (bytes.MaxLength, -1, "#2 Should return -1");
			b = new byte [10];
			bytes = new SqlBytes (b);
			Assert.AreEqual (bytes.MaxLength, 10, "#3 Should return 10");
		}
		[Test]
		public void SqlBytesNull ()
		{
			byte [] b = null;
			SqlBytes bytes = SqlBytes.Null;
			Assert.AreEqual (bytes.IsNull, true, "#1 Should return true");
		}
		[Test]
		public void SqlBytesStorage ()
		{
			byte [] b = null;
			SqlBytes bytes = new SqlBytes ();
			try {
				Assert.AreEqual (bytes.Storage, StorageState.Buffer, "#1 Should throw SqlNullValueException");
				Assert.Fail ("Should throw SqlNullValueException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "Should throw SqlNullValueException");
			}
			bytes = new SqlBytes (b);
			try {
				Assert.AreEqual (bytes.Storage, StorageState.Buffer, "#2 Should throw SqlNullValueException");
				Assert.Fail ("Should throw SqlNullValueException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "Should throw SqlNullValueException");
			}
			b = new byte [10];
			bytes = new SqlBytes (b);
			Assert.AreEqual (bytes.Storage, StorageState.Buffer, "#3 Should be StorageState.Buffer");
			FileStream fs = null;
			bytes = new SqlBytes (fs);
			try {
				Assert.AreEqual (bytes.Storage, StorageState.Buffer, "#4 Should throw SqlNullValueException");
				Assert.Fail ("Should throw SqlNullValueException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "Should throw SqlNullValueException");
			}
		}
		[Test]
		public void SqlBytesValue ()
		{
			byte [] b1 = new byte [10];
			SqlBytes bytes = new SqlBytes (b1);
			byte [] b2 = bytes.Value;
			Assert.AreEqual (b1 [0], b2 [0], "#1 Should be same");
			b2 [0] = 10;
			Assert.AreEqual (b1 [0], 0, "#2 Should be same");
			Assert.AreEqual (b2 [0], 10, "#3 Should be same");
		}
		[Test]
		public void SqlBytesSetLength ()
		{
			byte [] b1 = new byte [10];
			SqlBytes bytes = new SqlBytes ();
			try {
				bytes.SetLength (20);
				Assert.Fail ("Should throw SqlTypeException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (SqlTypeException), ex.GetType (), "Should throw SqlTypeException");
			}
			bytes = new SqlBytes (b1);
			Assert.AreEqual (bytes.Length, 10, "#1 Should be same");
			try {
				bytes.SetLength (-1);
				Assert.Fail ("Should throw ArgumentOutOfRangeException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "Should throw ArgumentOutOfRangeException");
			}
			try {
				bytes.SetLength (11);
				Assert.Fail ("Should throw ArgumentOutOfRangeException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "Should throw ArgumentOutOfRangeException");
			}
			bytes.SetLength (2);
			Assert.AreEqual (bytes.Length, 2, "#2 Should be same");
		}
		[Test]
		public void SqlBytesSetNull ()
		{
			byte [] b1 = new byte [10];
			SqlBytes bytes = new SqlBytes (b1);
			Assert.AreEqual (bytes.Length, 10, "#1 Should be same");
			bytes.SetNull ();
			try {
				Assert.AreEqual (bytes.Length, 10, "#1 Should not be same");
				Assert.Fail ("Should throw SqlNullValueException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "Should throw SqlNullValueException");
			}
			Assert.AreEqual (true, bytes.IsNull, "#2 Should be same");
		}
	}
}
#endif
