//
// DataSetInferXmlSchemaTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.

using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataSetInferXmlSchemaTest : DataSetAssertion
	{
		string xml1 = "<root/>";
		string xml2 = "<root attr='value' />";
		string xml3 = "<root attr='value' attr2='2' />";
		string xml4 = "<root>simple.txt</root>";
		string xml5 = "<root><child/></root>";

		private DataSet GetDataSet (string xml, string [] nss)
		{
			DataSet ds = new DataSet ();
			ds.InferXmlSchema (new XmlTextReader (xml, XmlNodeType.Document, null), nss);
			return ds;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NullFileName ()
		{
			DataSet ds = GetDataSet (null, null);
		}

		[Test]
		public void SingleElement ()
		{
			DataSet ds = GetDataSet (xml1, null);
			AssertDataSet ("ds", ds, "root", 0, 0);

			ds = GetDataSet (xml4, null);
			AssertDataSet ("ds", ds, "root", 0, 0);
		}

		[Test]
		public void SingleElementWithAttribute ()
		{
			DataSet ds = GetDataSet (xml2, null);
			AssertDataSet ("ds", ds, "NewDataSet", 1, 0);
			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "root", 1, 0);
			AssertDataColumn ("col", dt.Columns [0], "attr", true, false, 0, 1, "attr", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
		}

		[Test]
		public void SingleElementWithTwoAttribute ()
		{
			DataSet ds = GetDataSet (xml3, null);
			AssertDataSet ("ds", ds, "NewDataSet", 1, 0);
			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "root", 2, 0);
			AssertDataColumn ("col", dt.Columns [0], "attr", true, false, 0, 1, "attr", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("col", dt.Columns [1], "attr2", true, false, 0, 1, "attr2", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);
		}

		[Test]
		public void SingleEmptyChild ()
		{
			DataSet ds = GetDataSet (xml5, null);
			AssertDataSet ("ds", ds, "NewDataSet", 1, 0);
			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "root", 1, 0);
			AssertDataColumn ("col", dt.Columns [0], "child", true, false, 0, 1, "child", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
		}
	}
}
