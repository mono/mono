//
// XmlDataDocumentTest2.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Data;
using System.IO;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlDataDocumentTest2 : Assertion
	{
		string xml = "<NewDataSet><table><row><col1>1</col1><col2>2</col2></row></table></NewDataSet>";

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestCtorNullArgs ()
		{
			new XmlDataDocument (null);
		}

		[Test]
		public void TestDefaultCtor ()
		{
			XmlDataDocument doc = new XmlDataDocument ();
			AssertNotNull (doc.DataSet);
			AssertEquals ("NewDataSet", doc.DataSet.DataSetName);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TestMultipleLoadError ()
		{
			DataSet ds = new DataSet ();
			ds.ReadXml (new XmlTextReader (xml, XmlNodeType.Document, null));
			// If there is already data element, Load() fails.
			XmlDataDocument doc = new XmlDataDocument (ds);
			doc.LoadXml (xml);
		}

		[Test]
		public void TestMultipleLoadNoError ()
		{
			DataSet ds = new DataSet ();
			DataTable dt = new DataTable ();
			dt.Columns.Add ("col1");
			ds.Tables.Add (dt);

			XmlDataDocument doc = new XmlDataDocument (ds);
			doc.LoadXml (xml);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestMultipleDataDocFromDataSet ()
		{
			DataSet ds = new DataSet ();
			XmlDataDocument doc = new XmlDataDocument (ds);
			XmlDataDocument doc2 = new XmlDataDocument (ds);
		}

		[Test]
		public void TestLoadXml ()
		{
			XmlDataDocument doc = new XmlDataDocument ();
			doc.LoadXml ("<NewDataSet><TestTable><TestRow><TestColumn>1</TestColumn></TestRow></TestTable></NewDataSet>");

			doc = new XmlDataDocument ();
			doc.LoadXml ("<test>value</test>");
		}

		[Test]
		public void TestCreateElementAndRow ()
		{
			DataSet ds = new DataSet ("set");
			DataTable dt = new DataTable ("tab1");
			dt.Columns.Add ("col1");
			dt.Columns.Add ("col2");
			ds.Tables.Add (dt);
			DataTable dt2 = new DataTable ("child");
			dt2.Columns.Add ("ref");
			dt2.Columns.Add ("val");
			ds.Tables.Add (dt2);
			DataRelation rel = new DataRelation ("rel",
				dt.Columns [0], dt2.Columns [0]);
			rel.Nested = true;
			ds.Relations.Add (rel);
			XmlDataDocument doc = new XmlDataDocument (ds);
			doc.LoadXml ("<set><tab1><col1>1</col1><col2/><child><ref>1</ref><val>aaa</val></child></tab1></set>");
			AssertEquals (1, ds.Tables [0].Rows.Count);
			AssertEquals (1, ds.Tables [1].Rows.Count);

			// document element - no mapped row
			XmlElement el = doc.DocumentElement;
			AssertNull (doc.GetRowFromElement (el));

			// tab1 element - has mapped row
			el = el.FirstChild as XmlElement;
			DataRow row = doc.GetRowFromElement (el);
			AssertNotNull (row);
			AssertEquals (DataRowState.Added, row.RowState);

			// col1 - it is column. no mapped row
			el = el.FirstChild as XmlElement;
			row = doc.GetRowFromElement (el);
			AssertNull (row);

			// col2 - it is column. np mapped row
			el = el.NextSibling as XmlElement;
			row = doc.GetRowFromElement (el);
			AssertNull (row);

			// child - has mapped row
			el = el.NextSibling as XmlElement;
			row = doc.GetRowFromElement (el);
			AssertNotNull (row);
			AssertEquals (DataRowState.Added, row.RowState);

			// created (detached) table 1 element (used later)
			el = doc.CreateElement ("tab1");
			row = doc.GetRowFromElement (el);
			AssertEquals (DataRowState.Detached, row.RowState);
			AssertEquals (1, dt.Rows.Count); // not added yet

			// adding a node before setting EnforceConstraints
			// raises an error
			try {
				doc.DocumentElement.AppendChild (el);
				Fail ("Invalid Operation should occur; EnforceConstraints prevents addition.");
			} catch (InvalidOperationException) {
			}

			// try again...
			ds.EnforceConstraints = false;
			AssertEquals (1, dt.Rows.Count); // not added yet
			doc.DocumentElement.AppendChild (el);
			AssertEquals (2, dt.Rows.Count); // added
			row = doc.GetRowFromElement (el);
			AssertEquals (DataRowState.Added, row.RowState); // changed

			// Irrelevant element
			XmlElement el2 = doc.CreateElement ("hoge");
			row = doc.GetRowFromElement (el2);
			AssertNull (row);

			// created table 2 element (used later)
			el = doc.CreateElement ("child");
			row = doc.GetRowFromElement (el);
			AssertEquals (DataRowState.Detached, row.RowState);

			// Adding it to irrelevant element performs no row state change.
			AssertEquals (1, dt2.Rows.Count); // not added yet
			el2.AppendChild (el);
			AssertEquals (1, dt2.Rows.Count); // still not added
			row = doc.GetRowFromElement (el);
			AssertEquals (DataRowState.Detached, row.RowState); // still detached here
		}

		// bug #54505
		public void TypedDataDocument ()
		{
			string xml = @"<top xmlns=""urn:test"">
  <foo>
    <s>first</s>
    <d>2004-02-14T10:37:03</d>
  </foo>
  <foo>
    <s>second</s>
    <d>2004-02-17T12:41:49</d>
  </foo>
</top>";
			string xmlschema = @"<xs:schema id=""webstore"" targetNamespace=""urn:test"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:element name=""top"">
    <xs:complexType>
      <xs:sequence maxOccurs=""unbounded"">
        <xs:element name=""foo"">
          <xs:complexType>
            <xs:sequence maxOccurs=""unbounded"">
              <xs:element name=""s"" type=""xs:string""/>
              <xs:element name=""d"" type=""xs:dateTime""/>
            </xs:sequence>
          </xs:complexType>
        </xs:element>
      </xs:sequence>
    </xs:complexType>
  </xs:element>
</xs:schema>";
			XmlDataDocument doc = new XmlDataDocument ();
			doc.DataSet.ReadXmlSchema (new StringReader (xmlschema));
			doc.LoadXml (xml);
			DataTable foo = doc.DataSet.Tables ["foo"];
			DataRow newRow = foo.NewRow ();
			newRow ["s"] = "new";
			newRow ["d"] = DateTime.Now;
			foo.Rows.Add (newRow);
			doc.Save (new StringWriter ());
		}
	}
}
