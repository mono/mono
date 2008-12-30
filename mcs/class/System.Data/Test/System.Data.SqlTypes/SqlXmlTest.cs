//
// SqlXmlTest.cs - NUnit Test Cases for System.Data.SqlTypes.SqlXml
//
// Authors:
//   Veerapuram Varadhan (vvaradhan@novell.com)
//

//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
	public class SqlXmlTest
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

		[Test] // .ctor (Stream)
		[Category ("NotWorking")]
		public void Constructor2_Stream_ASCII ()
		{
			string xmlStr = "<Employee><FirstName>Varadhan</FirstName><LastName>Veerapuram</LastName></Employee>";
			MemoryStream stream = new MemoryStream (Encoding.ASCII.GetBytes (xmlStr));
			SqlXml xmlSql = new SqlXml (stream);
			Assert.IsFalse (xmlSql.IsNull, "#1");
			Assert.AreEqual (xmlStr, xmlSql.Value, "#2");
		}

		// Test constructor
		[Test] // .ctor (Stream)
		[Category ("NotDotNet")] // Name cannot begin with the '.' character, hexadecimal value 0x00. Line 1, position 2
		public void Constructor2_Stream_Unicode ()
		{
			string xmlStr = "<Employee><FirstName>Varadhan</FirstName><LastName>Veerapuram</LastName></Employee>";
			MemoryStream stream = new MemoryStream (Encoding.Unicode.GetBytes (xmlStr));
			SqlXml xmlSql = new SqlXml (stream);
			Assert.IsFalse (xmlSql.IsNull, "#1");
			Assert.AreEqual (xmlStr, xmlSql.Value, "#2");
		}

		[Test] // .ctor (Stream)
		[Category ("NotWorking")]
		public void Constructor2_Stream_UTF8 ()
		{
			string xmlStr = "<Employee><FirstName>Varadhan</FirstName><LastName>Veerapuram</LastName></Employee>";
			MemoryStream stream = new MemoryStream (Encoding.UTF8.GetBytes (xmlStr));
			SqlXml xmlSql = new SqlXml (stream);
			Assert.IsFalse (xmlSql.IsNull, "#1");
			Assert.AreEqual (xmlStr, xmlSql.Value, "#2");
		}

		[Test] // .ctor (Stream)
		public void Constructor2_Stream_Empty ()
		{
			MemoryStream ms = new MemoryStream ();
			SqlXml xmlSql = new SqlXml (ms);
			Assert.IsFalse (xmlSql.IsNull, "#1");
			Assert.AreEqual (string.Empty, xmlSql.Value, "#2");
		}

		[Test]
		public void Constructor2_Stream_Null ()
		{
			SqlXml xmlSql = new SqlXml ((Stream) null);
			Assert.IsTrue (xmlSql.IsNull, "#1");

			try {
				string value = xmlSql.Value;
				Assert.Fail ("#2:" + value);
			} catch (SqlNullValueException) {
			}
		}

		[Test] // .ctor (XmlReader)
		public void Constructor3 ()
		{
			string xmlStr = "<Employee><FirstName>Varadhan</FirstName><LastName>Veerapuram</LastName></Employee>";
			XmlReader xrdr = new XmlTextReader (new StringReader (xmlStr));
			SqlXml xmlSql = new SqlXml (xrdr);
			Assert.IsFalse (xmlSql.IsNull, "#1");
			Assert.AreEqual (xmlStr, xmlSql.Value, "#2");
		}

		[Test] // .ctor (XmlReader)
		public void Constructor3_XmlReader_Empty ()
		{
			XmlReaderSettings xs = new XmlReaderSettings ();
			xs.ConformanceLevel = ConformanceLevel.Fragment;
			XmlReader xrdr = XmlReader.Create (new StringReader (String.Empty), xs);
			SqlXml xmlSql = new SqlXml (xrdr);
			Assert.IsFalse (xmlSql.IsNull, "#1");
			Assert.AreEqual (string.Empty, xmlSql.Value, "#2");
		}

		[Test]
		public void Constructor3_XmlReader_Null ()
		{
			SqlXml xmlSql = new SqlXml ((XmlReader) null);
			Assert.IsTrue (xmlSql.IsNull, "#1");

			try {
				string value = xmlSql.Value;
				Assert.Fail ("#2:" + value);
			} catch (SqlNullValueException) {
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void CreateReader_Stream_ASCII ()
		{
			string xmlStr = "<Employee><FirstName>Varadhan</FirstName><LastName>Veerapuram</LastName></Employee>";
			MemoryStream stream = new MemoryStream (Encoding.ASCII.GetBytes (xmlStr));
			SqlXml xmlSql = new SqlXml (stream);

			XmlReader xrdr = xmlSql.CreateReader ();
			xrdr.MoveToContent ();
			
			Assert.AreEqual (xmlStr, xrdr.ReadOuterXml(), "#1");
		}

		[Test]
		[Category ("NotDotNet")] // Name cannot begin with the '.' character, hexadecimal value 0x00. Line 1, position 2
		public void CreateReader_Stream_Unicode ()
		{
			string xmlStr = "<Employee><FirstName>Varadhan</FirstName><LastName>Veerapuram</LastName></Employee>";
			MemoryStream stream = new MemoryStream (Encoding.Unicode.GetBytes (xmlStr));
			SqlXml xmlSql = new SqlXml (stream);

			XmlReader xrdr = xmlSql.CreateReader ();
			xrdr.MoveToContent ();
			
			Assert.AreEqual (xmlStr, xrdr.ReadOuterXml(), "#A05");
		}

		[Test]
		[Category ("NotWorking")]
		public void CreateReader_Stream_UTF8 ()
		{
			string xmlStr = "<Employee><FirstName>Varadhan</FirstName><LastName>Veerapuram</LastName></Employee>";
			MemoryStream stream = new MemoryStream (Encoding.UTF8.GetBytes (xmlStr));
			SqlXml xmlSql = new SqlXml (stream);

			XmlReader xrdr = xmlSql.CreateReader ();
			xrdr.MoveToContent ();
			
			Assert.AreEqual (xmlStr, xrdr.ReadOuterXml(), "#1");
		}

		[Test]
		public void SqlXml_fromXmlReader_CreateReaderTest()
		{
			string xmlStr = "<Employee><FirstName>Varadhan</FirstName><LastName>Veerapuram</LastName></Employee>";
			XmlReader rdr = new XmlTextReader (new StringReader (xmlStr));
			SqlXml xmlSql = new SqlXml (rdr);

			XmlReader xrdr = xmlSql.CreateReader ();
			xrdr.MoveToContent ();
			
			Assert.AreEqual (xmlStr, xrdr.ReadOuterXml(), "#A06");
		}

		[Test]
		public void SqlXml_fromZeroLengthStream_CreateReaderTest()
		{
			MemoryStream stream = new MemoryStream ();
			SqlXml xmlSql = new SqlXml (stream);

			XmlReader xrdr = xmlSql.CreateReader ();

			Assert.AreEqual (false, xrdr.Read(), "#A07");
		}

		[Test]
		public void SqlXml_fromZeroLengthXmlReader_CreateReaderTest_withFragment()
		{
			XmlReaderSettings xs = new XmlReaderSettings ();
			xs.ConformanceLevel = ConformanceLevel.Fragment;

			XmlReader rdr = XmlReader.Create (new StringReader (String.Empty), xs);
			SqlXml xmlSql = new SqlXml (rdr);

			XmlReader xrdr = xmlSql.CreateReader ();

			Assert.AreEqual (false, xrdr.Read(), "#A07");
		}

		[Test]
		public void SqlXml_fromZeroLengthXmlReader_CreateReaderTest()
		{
			XmlReader rdr = new XmlTextReader (new StringReader (String.Empty));
			try {
				new SqlXml (rdr);
				Assert.Fail ("#1");
			} catch (XmlException) {
			}
		}

		[Test]
		public void CreateReader_Stream_Null ()
		{
			SqlXml xmlSql = new SqlXml ((Stream) null);
			try {
				xmlSql.CreateReader ();
				Assert.Fail ("#1");
			} catch (SqlNullValueException) {
			}
		}

		[Test]
		public void CreateReader_XmlReader_Null ()
		{
			SqlXml xmlSql = new SqlXml ((XmlReader) null);
			try {
				xmlSql.CreateReader ();
				Assert.Fail ("#1");
			} catch (SqlNullValueException) {
			}
		}
	}
}
#endif
