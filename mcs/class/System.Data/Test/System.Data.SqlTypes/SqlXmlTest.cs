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
using NUnit.Framework;
using System;
using System.Xml;
using System.Data.SqlTypes;
using System.Threading;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace MonoTests.System.Data.SqlTypes
{
	[TestFixture]
    public class SqlXmlTest
	{

		[SetUp]
		public void SetUp ()
		{
	              Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
		}
                // Test constructor
		[Test]
		public void SqlXml_ctor_StreamTest()
		{
			string xmlStr = "<Employee><FirstName>Varadhan</FirstName><LastName>Veerapuram</LastName></Employee>";
			MemoryStream stream = new MemoryStream (Encoding.Unicode.GetBytes (xmlStr));
			SqlXml xmlSql = new SqlXml (stream);
			
			Assert.AreEqual (xmlStr, xmlSql.Value, "#A01");			
		}

		[Test]
		public void SqlXml_ctor_XmlReaderTest()
		{
			string xmlStr = "<Employee><FirstName>Varadhan</FirstName><LastName>Veerapuram</LastName></Employee>";
			XmlReader xrdr = new XmlTextReader (new StringReader (xmlStr));
			SqlXml xmlSql = new SqlXml (xrdr);
			
			Assert.AreEqual (xmlStr, xmlSql.Value, "#A02");			
		}

		[Test]
		public void SqlXml_ctor_ZeroLengthStreamTest()
		{
			MemoryStream ms = new MemoryStream ();
			SqlXml xmlSql = new SqlXml (ms);
			
			Assert.AreEqual (false, xmlSql.IsNull, "#A03");			
		}

		[Test]
		public void SqlXml_ctor_ZeroLengthXmlReaderTest()
		{
			XmlReaderSettings xs = new XmlReaderSettings ();
			xs.ConformanceLevel = ConformanceLevel.Fragment;
			XmlReader xrdr = XmlReader.Create (new StringReader (String.Empty), xs);
			SqlXml xmlSql = new SqlXml (xrdr);
			
			Assert.AreEqual (false, xmlSql.IsNull, "#A04");			
		}

		[Test]
		[ExpectedException (typeof (SqlNullValueException))]
		public void SqlXml_getValue_ZeroLengthStreamTest()
		{
			MemoryStream ms = null;
			SqlXml xmlSql = new SqlXml (ms);
			
			string str = xmlSql.Value;
		}

		[Test]
		[ExpectedException (typeof (SqlNullValueException))]
		public void SqlXml_getValue_ZeroLengthXmlReaderTest()
		{
			XmlReader xrdr = null;
			SqlXml xmlSql = new SqlXml (xrdr);
			
			string str = xmlSql.Value;
		}

		[Test]
		public void SqlXml_fromStream_CreateReaderTest()
		{
			string xmlStr = "<Employee><FirstName>Varadhan</FirstName><LastName>Veerapuram</LastName></Employee>";
			MemoryStream stream = new MemoryStream (Encoding.Unicode.GetBytes (xmlStr));
			SqlXml xmlSql = new SqlXml (stream);

			XmlReader xrdr = xmlSql.CreateReader ();
			xrdr.MoveToContent ();
			
			Assert.AreEqual (xmlStr, xrdr.ReadOuterXml(), "#A05");			
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
		[ExpectedException (typeof (XmlException))]
		public void SqlXml_fromZeroLengthXmlReader_CreateReaderTest()
		{
			XmlReader rdr = new XmlTextReader (new StringReader (String.Empty));
			SqlXml xmlSql = new SqlXml (rdr);

			XmlReader xrdr = xmlSql.CreateReader ();

			Assert.AreEqual (false, xrdr.Read(), "#A07");			
		}

		[Test]
		[ExpectedException (typeof (SqlNullValueException))]
		public void SqlXml_fromNullStream_CreateReaderTest()
		{
			MemoryStream stream = null;
			SqlXml xmlSql = new SqlXml (stream);

			XmlReader xrdr = xmlSql.CreateReader ();
		}

		[Test]
		[ExpectedException (typeof (SqlNullValueException))]
		public void SqlXml_fromNullXmlReader_CreateReaderTest()
		{
			XmlReader rdr = null;
			SqlXml xmlSql = new SqlXml (rdr);

			XmlReader xrdr = xmlSql.CreateReader ();
		}
	}
}
#endif 
