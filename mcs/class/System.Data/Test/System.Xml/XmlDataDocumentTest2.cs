//
// XmlDataDocumentTest2.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
using System;
using System.Data;
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
		}
	}
}
