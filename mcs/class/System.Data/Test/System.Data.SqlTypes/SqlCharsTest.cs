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
using System.Xml;
using System.Data.SqlTypes;
using System.Threading;
using System.Globalization;

#if NET_2_0
using System.Xml.Serialization;
#endif 

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
	public class SqlCharsTest
	{
		private CultureInfo originalCulture;

		[SetUp]
		public void SetUp ()
		{
			originalCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
		}

		[TearDown]
		public void TearDown ()
		{
			Thread.CurrentThread.CurrentCulture = originalCulture;
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

		[Test]
		public void GetXsdTypeTest ()
		{
			XmlQualifiedName qualifiedName = SqlChars.GetXsdType (null);
			NUnit.Framework.Assert.AreEqual ("string", qualifiedName.Name, "#A01");
		}

		internal void ReadWriteXmlTestInternal (string xml, 
							string testval, 
							string unit_test_id)
		{
			SqlString test;
			SqlString test1;
			XmlSerializer ser;
			StringWriter sw;
			XmlTextWriter xw;
			StringReader sr;
			XmlTextReader xr;

			test = new SqlString (testval);
			ser = new XmlSerializer(typeof(SqlString));
			sw = new StringWriter ();
			xw = new XmlTextWriter (sw);
			
			ser.Serialize (xw, test);

			Assert.AreEqual (xml, sw.ToString (), unit_test_id);

			sr = new StringReader (xml);
			xr = new XmlTextReader (sr);
			test1 = (SqlString)ser.Deserialize (xr);

			Assert.AreEqual (testval, test1.Value, unit_test_id);
		}

		[Test]
		public void ReadWriteXmlTest ()
		{
			string xml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><string>This is a test string</string>";
			string xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><string>a</string>";
			string strtest1 = "This is a test string";
			char strtest2 = 'a';

			ReadWriteXmlTestInternal (xml1, strtest1, "BA01");
			ReadWriteXmlTestInternal (xml2, strtest2.ToString (), "BA02");
		}

		/* Read tests */
		[Test]
		public void Read_SuccessTest1 ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			SqlChars chars = new SqlChars (c1);
			char [] c2 = new char [10];

			chars.Read (0, c2, 0, (int) chars.Length);
			Assert.AreEqual (chars.Value [5], c2 [5], "#1 Should be equal");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Read_NullBufferTest ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			SqlChars chars = new SqlChars (c1);
			char [] c2 = null;
			
			chars.Read (0, c2, 0, 10);
			Assert.Fail ("#2 Should throw ArgumentNullException");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Read_InvalidCountTest1 ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			SqlChars chars = new SqlChars (c1);
			char [] c2 = new char [5]; 

			chars.Read (0, c2, 0, 10);
			Assert.Fail ("#3 Should throw ArgumentOutOfRangeException");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Read_NegativeOffsetTest ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			SqlChars chars = new SqlChars (c1);
			char [] c2 = new char [5];
			
			chars.Read (-1, c2, 0, 4);
			Assert.Fail ("#4 Should throw ArgumentOutOfRangeException");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Read_NegativeOffsetInBufferTest ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			SqlChars chars = new SqlChars (c1);
			char [] c2 = new char [5];
			
			chars.Read (0, c2, -1, 4);
			Assert.Fail ("#5 Should throw ArgumentOutOfRangeException");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Read_InvalidOffsetInBufferTest ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			SqlChars chars = new SqlChars (c1);
			char [] c2 = new char [5];

			chars.Read (0, c2, 8, 4);
			Assert.Fail ("#6 Should throw ArgumentOutOfRangeException");
		}

		[Test]
		[ExpectedException (typeof (SqlNullValueException))]
		public void Read_NullInstanceValueTest ()
		{
			char [] c2 = new char [5];
			SqlChars chars = new SqlChars ();
			
			chars.Read (0, c2, 8, 4);
			Assert.Fail ("#7 Should throw SqlNullValueException");
		}

		[Test]
		public void Read_SuccessTest2 ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			SqlChars chars = new SqlChars (c1);
			char [] c2 = new char [10];
			
			chars.Read (5, c2, 0, 10);
			Assert.AreEqual (chars.Value [5], c2 [0], "#8 Should be same");
			Assert.AreEqual (chars.Value [9], c2 [4], "#9 Should be same");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Read_NullBufferAndInstanceValueTest ()
		{
			char [] c2 = null;
			SqlChars chars = new SqlChars ();
			
			chars.Read (0, c2, 8, 4);
			Assert.Fail ("#10 Should throw ArgumentNullException");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Read_NegativeCountTest ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			SqlChars chars = new SqlChars (c1);
			char [] c2 = new char [5];
			
			chars.Read (0, c2, 0, -1);
			Assert.Fail ("#11 Should throw ArgumentOutOfRangeException");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Read_InvalidCountTest2 ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			SqlChars chars = new SqlChars (c1);
			char [] c2 = new char [5]; 

			chars.Read (0, c2, 3, 4);
			Assert.Fail ("#12 Should throw ArgumentOutOfRangeException");
		}

		[Test]
		public void Write_SuccessTest1 ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			char [] c2 = new char [10];
			SqlChars chars = new SqlChars (c2);

			chars.Write (0, c1, 0, (int) c1.Length);
			Assert.AreEqual (chars.Value [0], c1 [0], "#1 Should be same");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Write_NegativeOffsetTest ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			char [] c2 = new char [10];
			SqlChars chars = new SqlChars (c2);

			chars.Write (-1, c1, 0, (int) c1.Length);
			Assert.Fail ("#2 Should throw ArgumentOutOfRangeException");
		}

		[Test]
		[ExpectedException (typeof (SqlTypeException))]
		public void Write_InvalidOffsetTest ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			char [] c2 = new char [10];
			SqlChars chars = new SqlChars (c2);
			
			chars.Write (chars.Length+5, c1, 0, (int) c1.Length);
			Assert.Fail ("#3 Should throw SqlTypeException");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Write_NegativeOffsetInBufferTest ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			char [] c2 = new char [10];
			SqlChars chars = new SqlChars (c2);

			chars.Write (0, c1, -1, (int) c1.Length);
			Assert.Fail ("#4 Should throw ArgumentOutOfRangeException");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Write_InvalidOffsetInBufferTest ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			char [] c2 = new char [10];
			SqlChars chars = new SqlChars (c2);

			chars.Write (0, c1, c1.Length+5, (int) c1.Length);
			Assert.Fail ("#5 Should throw ArgumentOutOfRangeException");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Write_InvalidCountTest1 ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			char [] c2 = new char [10];
			SqlChars chars = new SqlChars (c2);

			chars.Write (0, c1, 0, (int) c1.Length+5);
			Assert.Fail ("#6 Should throw ArgumentOutOfRangeException");
		}

		[Test]
		[ExpectedException (typeof (SqlTypeException))]
		public void Write_InvalidCountTest2 ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			char [] c2 = new char [10];
			SqlChars chars = new SqlChars (c2);

			chars.Write (8, c1, 0, (int) c1.Length);
			Assert.Fail ("#7 Should throw SqlTypeException");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Write_NullBufferTest ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			char [] c2 = null;
			SqlChars chars = new SqlChars (c1);

			chars.Write (0, c2, 0, 10);
			Assert.Fail ("#8 Should throw ArgumentNullException");
		}

		[Test]
		[ExpectedException (typeof (SqlTypeException))]
		public void Write_NullInstanceValueTest ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			SqlChars chars = new SqlChars();

			chars.Write (0, c1, 0, 10);
			Assert.Fail ("#9 Should throw SqlTypeException");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Write_NullBufferAndInstanceValueTest ()
		{
			char [] c1 = null;
			SqlChars chars = new SqlChars();
			
			chars.Write (0, c1, 0, 10);
			Assert.Fail ("#9 Should throw ArgumentNullException");
		}
		
		[Test]
		public void Write_SuccessTest2 ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			char [] c2 = new char [20];
			SqlChars chars = new SqlChars (c2);

			chars.Write (8, c1, 0, 10);
			Assert.AreEqual (chars.Value [8], c1 [0], "#10 Should be same");
			Assert.AreEqual (chars.Value [17], c1 [9], "#10 Should be same");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Write_NegativeCountTest ()
		{
			char [] c1 = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j' };
			char [] c2 = new char [10];
			SqlChars chars = new SqlChars (c2);

			chars.Write (0, c1, 0, -1);
			Assert.Fail ("#11 Should throw ArgumentOutOfRangeException");
		}
	}
}

#endif
