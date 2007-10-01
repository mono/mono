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
using System.Xml;
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
		[Test]
		public void GetXsdTypeTest ()
		{
			XmlQualifiedName qualifiedName = SqlBytes.GetXsdType (null);
			NUnit.Framework.Assert.AreEqual ("base64Binary", qualifiedName.Name, "#A01");
		}

		/* Read tests */
		[Test]
		public void Read_SuccessTest1 ()
		{
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			SqlBytes bytes = new SqlBytes (b1);
			byte [] b2 = new byte [10];

			bytes.Read (0, b2, 0, (int) bytes.Length);
			Assert.AreEqual (bytes.Value [5], b2 [5], "#1 Should be equal");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Read_NullBufferTest ()
		{			
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			SqlBytes bytes = new SqlBytes (b1);			
			byte [] b2 = null;
			
			bytes.Read (0, b2, 0, 10);
			Assert.Fail ("#2 Should throw ArgumentNullException");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Read_InvalidCountTest1 ()
		{			
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			SqlBytes bytes = new SqlBytes (b1);			
			byte [] b2 = new byte [5]; 
			
			bytes.Read (0, b2, 0, 10);
			Assert.Fail ("#3 Should throw ArgumentOutOfRangeException");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Read_NegativeOffsetTest ()
		{			
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			SqlBytes bytes = new SqlBytes (b1);			
			byte [] b2 = new byte [5];
			
			bytes.Read (-1, b2, 0, 4);
			Assert.Fail ("#4 Should throw ArgumentOutOfRangeException");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Read_NegativeOffsetInBufferTest ()
		{			
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			SqlBytes bytes = new SqlBytes (b1);			
			byte [] b2 = new byte [5];
			
			bytes.Read (0, b2, -1, 4);
			Assert.Fail ("#5 Should throw ArgumentOutOfRangeException");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Read_InvalidOffsetInBufferTest ()
		{			
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			SqlBytes bytes = new SqlBytes (b1);			
			byte [] b2 = new byte [5];
			
			bytes.Read (0, b2, 8, 4);
			Assert.Fail ("#6 Should throw ArgumentOutOfRangeException");
		}
		
		[Test]
		[ExpectedException (typeof (SqlNullValueException))]
		public void Read_NullInstanceValueTest ()
		{			
			byte [] b2 = new byte [5];
			SqlBytes bytes = new SqlBytes ();
			
			bytes.Read (0, b2, 8, 4);
			Assert.Fail ("#7 Should throw SqlNullValueException");
		}
		
		[Test]
		public void Read_SuccessTest2 ()
		{
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };			
			SqlBytes bytes = new SqlBytes (b1);
			byte [] b2 = new byte [10];
			
			bytes.Read (5, b2, 0, 10);
			Assert.AreEqual (bytes.Value [5], b2 [0], "#8 Should be same");
			Assert.AreEqual (bytes.Value [9], b2 [4], "#9 Should be same");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Read_NullBufferAndInstanceValueTest ()
		{			
			byte [] b2 = null;
			SqlBytes bytes = new SqlBytes ();
			
			bytes.Read (0, b2, 8, 4);
			Assert.Fail ("#10 Should throw ArgumentNullException");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Read_NegativeCountTest ()
		{			
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			SqlBytes bytes = new SqlBytes (b1);			
			byte [] b2 = new byte [5];
			
			bytes.Read (0, b2, 0, -1);
			Assert.Fail ("#11 Should throw ArgumentOutOfRangeException");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Read_InvalidCountTest2 ()
		{			
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			SqlBytes bytes = new SqlBytes (b1);			
			byte [] b2 = new byte [5]; 
			
			bytes.Read (0, b2, 3, 4);
			Assert.Fail ("#12 Should throw ArgumentOutOfRangeException");
		}
		
		/* Write Tests */
		[Test]
		public void Write_SuccessTest1 ()
		{
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			byte [] b2 = new byte[10];
			SqlBytes bytes = new SqlBytes (b2);
			
			bytes.Write (0, b1, 0, (int) b1.Length);
			Assert.AreEqual (bytes.Value [0], b1 [0], "#1 Should be same");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Write_NegativeOffsetTest ()
		{
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			byte [] b2 = new byte [10];
			SqlBytes bytes = new SqlBytes (b2);
			
			bytes.Write (-1, b1, 0, (int) b1.Length);
			Assert.Fail ("#2 Should throw ArgumentOutOfRangeException");
		}
		
		[Test]
		[ExpectedException (typeof (SqlTypeException))]
		public void Write_InvalidOffsetTest ()
		{
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			byte [] b2 = new byte [10];
			SqlBytes bytes = new SqlBytes (b2);
			
			bytes.Write (bytes.Length+5, b1, 0, (int) b1.Length);
			Assert.Fail ("#3 Should throw SqlTypeException");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Write_NegativeOffsetInBufferTest ()
		{
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			byte [] b2 = new byte [10];
			SqlBytes bytes = new SqlBytes (b2);
			
			bytes.Write (0, b1, -1, (int) b1.Length);
			Assert.Fail ("#4 Should throw ArgumentOutOfRangeException");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Write_InvalidOffsetInBufferTest ()
		{
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			byte [] b2 = new byte [10];
			SqlBytes bytes = new SqlBytes (b2);
			
			bytes.Write (0, b1, b1.Length+5, (int) b1.Length);
			Assert.Fail ("#5 Should throw ArgumentOutOfRangeException");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Write_InvalidCountTest1 ()
		{
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			byte [] b2 = new byte [10];
			SqlBytes bytes = new SqlBytes (b2);
			
			bytes.Write (0, b1, 0, (int) b1.Length+5);
			Assert.Fail ("#6 Should throw ArgumentOutOfRangeException");
		}
		
		[Test]
		[ExpectedException (typeof (SqlTypeException))]
		public void Write_InvalidCountTest2 ()
		{
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			byte [] b2 = new byte [10];
			SqlBytes bytes = new SqlBytes (b2);
			
			bytes.Write (8, b1, 0, (int) b1.Length);
			Assert.Fail ("#7 Should throw SqlTypeException");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Write_NullBufferTest ()
		{
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			byte [] b2 = null;
			SqlBytes bytes = new SqlBytes (b1);
			
			bytes.Write (0, b2, 0, 10);
			Assert.Fail ("#8 Should throw ArgumentNullException");
		}
		
		[Test]
		[ExpectedException (typeof (SqlTypeException))]
		public void Write_NullInstanceValueTest ()
		{
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			SqlBytes bytes = new SqlBytes();
			
			bytes.Write (0, b1, 0, 10);
			Assert.Fail ("#9 Should throw SqlTypeException");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Write_NullBufferAndInstanceValueTest ()
		{
			byte [] b1 = null;
			SqlBytes bytes = new SqlBytes();
			
			bytes.Write (0, b1, 0, 10);
			Assert.Fail ("#9 Should throw ArgumentNullException");
		}
		
		[Test]
		public void Write_SuccessTest2 ()
		{
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			byte [] b2 = new byte [20];
			SqlBytes bytes = new SqlBytes (b2);
			
			bytes.Write (8, b1, 0, 10);
			Assert.AreEqual (bytes.Value [8], b1 [0], "#10 Should be same");
			Assert.AreEqual (bytes.Value [17], b1 [9], "#10 Should be same");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Write_NegativeCountTest ()
		{
			byte [] b1 = { 33, 34, 35, 36, 37, 38, 39, 40, 41, 42 };
			byte [] b2 = new byte [10];
			SqlBytes bytes = new SqlBytes (b2);
			
			bytes.Write (0, b1, 0, -1);
			Assert.Fail ("#11 Should throw ArgumentOutOfRangeException");
		}
	}
}
#endif
