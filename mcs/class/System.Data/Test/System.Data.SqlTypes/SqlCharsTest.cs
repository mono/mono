//
// SqlCharsTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlChars
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
        public class SqlCharsTest
	{
		[SetUp]
		public void SetUp ()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
		}

		// Test constructor
		[Test]
		public void SqlCharsItem ()
		{
			SqlChars chars = new SqlChars ();
			try {
				Assert.AreEqual (chars [0], 0, "#1 Should throw SqlNullValueException");
				Assert.Fail ("Should throw SqlNullValueException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "Should throw SqlNullValueException");
			}
			char [] b = null;
			chars = new SqlChars (b);
			try {
				Assert.AreEqual (chars [0], 0, "#2 Should throw SqlNullValueException");
				Assert.Fail ("Should throw SqlNullValueException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "Should throw SqlNullValueException");
			}
			b = new char [10];
			chars = new SqlChars (b);
			Assert.AreEqual (chars [0], 0, "");
			try {
				Assert.AreEqual (chars [-1], 0, "");
				Assert.Fail ("Should throw ArgumentOutOfRangeException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "Should throw ArgumentOutOfRangeException");
			}
			try {
				Assert.AreEqual (chars [10], 0, "");
				Assert.Fail ("Should throw ArgumentOutOfRangeException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "Should throw ArgumentOutOfRangeException");
			}
		}
		[Test]
		public void SqlCharsLength ()
		{
			char [] b = null;
			SqlChars chars = new SqlChars ();
			try {
				Assert.AreEqual (chars.Length, 0, "#1 Should throw SqlNullValueException");
				Assert.Fail ("Should throw SqlNullValueException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "Should throw SqlNullValueException");
			}
			chars = new SqlChars (b);
			try {
				Assert.AreEqual (chars.Length, 0, "#2 Should throw SqlNullValueException");
				Assert.Fail ("Should throw SqlNullValueException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "Should throw SqlNullValueException");
			}
			b = new char [10];
			chars = new SqlChars (b);
			Assert.AreEqual (chars.Length, 10, "#3 Should be 10");
		}
		[Test]
		public void SqlCharsMaxLength ()
		{
			char [] b = null;
			SqlChars chars = new SqlChars ();
			Assert.AreEqual (chars.MaxLength, -1, "#1 Should return -1");
			chars = new SqlChars (b);
			Assert.AreEqual (chars.MaxLength, -1, "#2 Should return -1");
			b = new char [10];
			chars = new SqlChars (b);
			Assert.AreEqual (chars.MaxLength, 10, "#3 Should return 10");
		}
		[Test]
		public void SqlCharsNull ()
		{
			char [] b = null;
			SqlChars chars = SqlChars.Null;
			Assert.AreEqual (chars.IsNull, true, "#1 Should return true");
		}
		[Test]
		public void SqlCharsStorage ()
		{
			char [] b = null;
			SqlChars chars = new SqlChars ();
			try {
				Assert.AreEqual (chars.Storage, StorageState.Buffer, "#1 Should throw SqlNullValueException");
				Assert.Fail ("Should throw SqlNullValueException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "Should throw SqlNullValueException");
			}
			chars = new SqlChars (b);
			try {
				Assert.AreEqual (chars.Storage, StorageState.Buffer, "#2 Should throw SqlNullValueException");
				Assert.Fail ("Should throw SqlNullValueException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "Should throw SqlNullValueException");
			}
			b = new char [10];
			chars = new SqlChars (b);
			Assert.AreEqual (chars.Storage, StorageState.Buffer, "#3 Should be StorageState.Buffer");
		}
		[Test]
		public void SqlCharsValue ()
		{
			char [] b1 = new char [10];
			SqlChars chars = new SqlChars (b1);
			char [] b2 = chars.Value;
			Assert.AreEqual (b1 [0], b2 [0], "#1 Should be same");
			b2 [0] = '1';
			Assert.AreEqual (b1 [0], 0, "#2 Should be same");
			Assert.AreEqual (b2 [0], '1', "#3 Should be same");
		}
		[Test]
#if TARGET_JVM
		[Ignore ("Array.Resize(null) is not supported")]
#endif
		public void SqlCharsSetLength ()
		{
			char [] b1 = new char [10];
			SqlChars chars = new SqlChars ();
			try {
				chars.SetLength (20);
				Assert.Fail ("Should throw SqlTypeException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (SqlTypeException), ex.GetType (), "Should throw SqlTypeException");
			}
			chars = new SqlChars (b1);
			Assert.AreEqual (chars.Length, 10, "#1 Should be same");
			try {
				chars.SetLength (-1);
				Assert.Fail ("Should throw ArgumentOutOfRangeException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "Should throw ArgumentOutOfRangeException");
			}
			try {
				chars.SetLength (11);
				Assert.Fail ("Should throw ArgumentOutOfRangeException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "Should throw ArgumentOutOfRangeException");
			}
			chars.SetLength (2);
			Assert.AreEqual (chars.Length, 2, "#2 Should be same");
		}
		[Test]
		public void SqlCharsSetNull ()
		{
			char [] b1 = new char [10];
			SqlChars chars = new SqlChars (b1);
			Assert.AreEqual (chars.Length, 10, "#1 Should be same");
			chars.SetNull ();
			try {
				Assert.AreEqual (chars.Length, 10, "#1 Should not be same");
				Assert.Fail ("Should throw SqlNullValueException");
			} catch (Exception ex) {
				Assert.AreEqual (typeof (SqlNullValueException), ex.GetType (), "Should throw SqlNullValueException");
			}
			Assert.AreEqual (true, chars.IsNull, "#2 Should be same");
		}
	}
}

#endif
