//
// DataTableReadXmlSchemaTest.cs
//
// Author:
//	Hagit Yidov <hagity@mainsoft.com>
//	[DataSetReadXmlSchemaTest.cs: Atsushi Enomoto <atsushi@ximian.com>]
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
using System.IO;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Xml;
using NUnit.Framework;

using MonoTests.Helpers;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataTableReadXmlSchemaTest : DataSetAssertion
	{
		private DataSet CreateTestSet ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add ("Table1");
			ds.Tables.Add ("Table2");
			ds.Tables [0].Columns.Add ("Column1_1");
			ds.Tables [0].Columns.Add ("Column1_2");
			ds.Tables [0].Columns.Add ("Column1_3");
			ds.Tables [1].Columns.Add ("Column2_1");
			ds.Tables [1].Columns.Add ("Column2_2");
			ds.Tables [1].Columns.Add ("Column2_3");
			ds.Tables [0].Rows.Add (new object [] {"ppp", "www", "xxx"});
			ds.Relations.Add ("Rel1", ds.Tables [0].Columns [2], ds.Tables [1].Columns [0]);
			return ds;
		}

		CultureInfo currentCultureBackup;

		[SetUp]
		public void Setup ()
		{
			currentCultureBackup = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("fi-FI");
		}

		[TearDown]
		public void Teardown ()
		{
			Thread.CurrentThread.CurrentCulture = currentCultureBackup;
		}

		[Test]
		[Category ("NotWorking")]
		public void SingleElementTreatmentDifference ()
		{
			// This is one of the most complicated case. When the content
			// type particle of 'Root' element is a complex element, it
			// is DataSet element. Otherwise, it is just a data table.
			//
			// But also note that there is another test named
			// LocaleOnRootWithoutIsDataSet(), that tests if locale on
			// the (mere) data table modifies *DataSet's* locale.

			// Moreover, when the schema contains another element
			// (regardless of its schema type), the elements will
			// never be treated as a DataSet.
			string xsbase = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' id='hoge'>
	<xs:element name='Root'> <!-- When simple, it becomes table. When complex, it becomes DataSet -->
		<xs:complexType>
			<xs:choice>
				{0}
			</xs:choice>
		</xs:complexType>
	</xs:element>
</xs:schema>";

			string xsbase2 = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' id='hoge'>
	<xs:element name='Root'> <!-- When simple, it becomes table. When complex, it becomes DataSet -->
		<xs:complexType>
			<xs:choice>
				{0}
			</xs:choice>
		</xs:complexType>
	</xs:element>
	<xs:element name='more' type='xs:string' />
</xs:schema>";

			string simple = "<xs:element name='Child' type='xs:string' />";
			string complex = @"<xs:element name='Child'>
	<xs:complexType>
		<xs:attribute name='a1' />
		<xs:attribute name='a2' type='xs:integer' />
	</xs:complexType>
</xs:element>";
			string elref = "<xs:element ref='more' />";

			string xs2 = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' id='hoge'>
	<xs:element name='Root' type='RootType' />
	<xs:complexType name='RootType'>
		<xs:choice>
			<xs:element name='Child'>
				<xs:complexType>
					<xs:attribute name='a1' />
					<xs:attribute name='a2' type='xs:integer' />
				</xs:complexType>
			</xs:element>
		</xs:choice>
	</xs:complexType>
</xs:schema>";

			DataSet ds = new DataSet ();
			ds.Tables.Add (new DataTable ("Root"));
			string xs = String.Format (xsbase, simple);
			ds.Tables[0].ReadXmlSchema (new StringReader (xs));
			AssertDataTable ("simple", ds.Tables [0], "Root", 1, 0, 0, 0, 0, 0);

			// reference to global complex type
			ds = new DataSet ();
			ds.Tables.Add (new DataTable ("Root"));
			ds.Tables.Add (new DataTable ("Child"));
			ds.Tables[0].ReadXmlSchema (new StringReader (xs2));
			ds.Tables[1].ReadXmlSchema (new StringReader (xs2));
			AssertDataTable ("external Tab1", ds.Tables [0], "Root", 1, 0, 0, 1, 1, 1);
			AssertDataTable ("external Tab2", ds.Tables [1], "Child", 3, 0, 1, 0, 1, 0);

			// xsbase2 + complex -> datatable
			ds = new DataSet ();
			ds.Tables.Add (new DataTable ("Root"));
			ds.Tables.Add (new DataTable ("Child"));
			xs = String.Format (xsbase2, complex);
			ds.Tables[0].ReadXmlSchema (new StringReader (xs));
			ds.Tables[1].ReadXmlSchema (new StringReader (xs));
			AssertDataTable ("complex", ds.Tables [0], "Root", 1, 0, 0, 1, 1, 1);
			DataTable dt = ds.Tables [1];
			AssertDataTable ("complex", dt, "Child", 3, 0, 1, 0, 1, 0);
			AssertDataColumn ("a1", dt.Columns ["a1"], "a1", true, false, 0, 1, "a1", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, /*0*/-1, String.Empty, false, false);
			AssertDataColumn ("a2", dt.Columns ["a2"], "a2", true, false, 0, 1, "a2", MappingType.Attribute, typeof (long), DBNull.Value, String.Empty, -1, String.Empty, /*1*/-1, String.Empty, false, false);
			AssertDataColumn ("Root_Id", dt.Columns [2], "Root_Id", true, false, 0, 1, "Root_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 2, String.Empty, false, false);

			ds = new DataSet ();
			ds.Tables.Add (new DataTable ("Root"));
			xs = String.Format (xsbase2, elref);
			ds.Tables[0].ReadXmlSchema (new StringReader (xs));
			AssertDataTable ("complex", ds.Tables [0], "Root", 1, 0, 0, 0, 0, 0);
		}

		[Test]
		public void SuspiciousDataSetElement ()
		{
			string schema = @"<?xml version='1.0'?>
<xsd:schema xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
	<xsd:attribute name='foo' type='xsd:string'/>
	<xsd:attribute name='bar' type='xsd:string'/>
	<xsd:complexType name='attRef'>
		<xsd:attribute name='att1' type='xsd:int'/>
		<xsd:attribute name='att2' type='xsd:string'/>
	</xsd:complexType>
	<xsd:element name='doc'>
		<xsd:complexType>
			<xsd:choice>
				<xsd:element name='elem' type='attRef'/>
			</xsd:choice>
		</xsd:complexType>
	</xsd:element>
</xsd:schema>";
			DataSet ds = new DataSet ();
			ds.Tables.Add (new DataTable ("elem"));
			ds.Tables[0].ReadXmlSchema (new StringReader (schema));
			AssertDataTable ("table", ds.Tables [0], "elem", 2, 0, 0, 0, 0, 0);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void UnusedComplexTypesIgnored ()
		{
			string xs = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' id='hoge'>
	<xs:element name='Root'>
		<xs:complexType>
			<xs:sequence>
				<xs:element name='Child' type='xs:string' />
			</xs:sequence>
		</xs:complexType>
	</xs:element>
	<xs:complexType name='unusedType'>
		<xs:sequence>
			<xs:element name='Orphan' type='xs:string' />
		</xs:sequence>
	</xs:complexType>
</xs:schema>";

			DataSet ds = new DataSet ();
			ds.Tables.Add (new DataTable ("Root"));
			ds.Tables.Add (new DataTable ("unusedType"));
			ds.Tables[0].ReadXmlSchema (new StringReader (xs));
			AssertDataTable ("dt", ds.Tables[0], "Root", 1, 0, 0, 0, 0, 0);
			// Here "unusedType" table is never imported.
			ds.Tables[1].ReadXmlSchema (new StringReader (xs));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsDataSetAndTypeIgnored ()
		{
			string xsbase = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata'>
	<xs:element name='Root' type='unusedType' msdata:IsDataSet='{0}'>
	</xs:element>
	<xs:complexType name='unusedType'>
		<xs:sequence>
			<xs:element name='Child' type='xs:string' />
		</xs:sequence>
	</xs:complexType>
</xs:schema>";
			// When explicit msdata:IsDataSet value is "false", then
			// treat as usual.
			string xs = String.Format (xsbase, "false");
			DataSet ds = new DataSet ();
			ds.Tables.Add (new DataTable ("Root"));
			ds.Tables[0].ReadXmlSchema (new StringReader (xs));
			AssertDataTable ("dt", ds.Tables[0], "Root", 1, 0, 0, 0, 0, 0);

			// Even if a global element uses a complexType, it will be
			// ignored if the element has msdata:IsDataSet='true'
			xs = String.Format (xsbase, "true");
			ds = new DataSet ();
			ds.Tables.Add (new DataTable ("Root"));
			ds.Tables[0].ReadXmlSchema (new StringReader (xs));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NestedReferenceNotAllowed ()
		{
			string xs = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata'>
	<xs:element name='Root' type='unusedType' msdata:IsDataSet='true'>
	</xs:element>
	<xs:complexType name='unusedType'>
		<xs:sequence>
			<xs:element name='Child' type='xs:string' />
		</xs:sequence>
	</xs:complexType>
	<xs:element name='Foo'>
		<xs:complexType>
			<xs:sequence>
				<xs:element ref='Root' />
			</xs:sequence>
		</xs:complexType>
	</xs:element>
</xs:schema>";

			// DataSet element cannot be converted into a DataTable.
			// (i.e. cannot be referenced in any other elements)
			DataSet ds = new DataSet ();
			ds.Tables.Add (new DataTable ());
			ds.Tables[0].ReadXmlSchema (new StringReader (xs));
		}

		[Test]
		public void LocaleOnRootWithoutIsDataSet ()
		{
			string xs = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata'>
	<xs:element name='Root' msdata:Locale='ja-JP'>
		<xs:complexType>
			<xs:sequence>
				<xs:element name='Child' type='xs:string' />
			</xs:sequence>
			<xs:attribute name='Attr' type='xs:integer' />
		</xs:complexType>
	</xs:element>
</xs:schema>";

			DataSet ds = new DataSet ();
			ds.Tables.Add ("Root");
			ds.Tables[0].ReadXmlSchema (new StringReader (xs));
			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "Root", 2, 0, 0, 0, 0, 0);
			Assert.AreEqual ("ja-JP", dt.Locale.Name); // DataTable's Locale comes from msdata:Locale
			AssertDataColumn ("col1", dt.Columns [0], "Attr", true, false, 0, 1, "Attr", MappingType.Attribute, typeof (Int64), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("col2", dt.Columns [1], "Child", false, false, 0, 1, "Child", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);
		}


		[Test]
		public void PrefixedTargetNS ()
		{
			string xs = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata' xmlns:x='urn:foo' targetNamespace='urn:foo' elementFormDefault='qualified'>
	<xs:element name='DS' msdata:IsDataSet='true'>
		<xs:complexType>
			<xs:choice>
				<xs:element ref='x:R1' />
				<xs:element ref='x:R2' />
			</xs:choice>
		</xs:complexType>
		<xs:key name='key'>
			<xs:selector xpath='./any/string_is_OK/x:R1'/>
			<xs:field xpath='x:Child2'/>
		</xs:key>
		<xs:keyref name='kref' refer='x:key'>
			<xs:selector xpath='.//x:R2'/>
			<xs:field xpath='x:Child2'/>
		</xs:keyref>
	</xs:element>
	<xs:element name='R3' type='x:RootType' />
	<xs:complexType name='extracted'>
		<xs:choice>
			<xs:element ref='x:R1' />
			<xs:element ref='x:R2' />
		</xs:choice>
	</xs:complexType>
	<xs:element name='R1' type='x:RootType'>
		<xs:unique name='Rkey'>
			<xs:selector xpath='.//x:Child1'/>
			<xs:field xpath='.'/>
		</xs:unique>
		<xs:keyref name='Rkref' refer='x:Rkey'>
			<xs:selector xpath='.//x:Child2'/>
			<xs:field xpath='.'/>
		</xs:keyref>
	</xs:element>
	<xs:element name='R2' type='x:RootType'>
	</xs:element>
	<xs:complexType name='RootType'>
		<xs:choice>
			<xs:element name='Child1' type='xs:string'>
			</xs:element>
			<xs:element name='Child2' type='xs:string' />
		</xs:choice>
		<xs:attribute name='Attr' type='xs:integer' />
	</xs:complexType>
</xs:schema>";
			// No prefixes on tables and columns
			DataSet ds = new DataSet ();
			ds.Tables.Add(new DataTable("R3"));
			ds.Tables[0].ReadXmlSchema (new StringReader (xs));
			DataTable dt = ds.Tables [0];
			AssertDataTable ("R3", dt, "R3", 3, 0, 0, 0, 0, 0);
			AssertDataColumn ("col1", dt.Columns [0], "Attr", true, false, 0, 1, "Attr", MappingType.Attribute, typeof (Int64), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
		}

		[Test]
		[Category ("NotWorking")]
		public void ReadTest1 ()
		{
			DataSet ds = CreateTestSet ();

			StringWriter sw = new StringWriter ();
			ds.WriteXmlSchema (sw);

			string schema = sw.ToString ();

			// ReadXmlSchema()
			DataSet ds1 = new DataSet ();
			ds1.Tables.Add (new DataTable("Table1"));
			ds1.Tables.Add (new DataTable("Table2"));
			ds1.Tables[0].ReadXmlSchema (new XmlTextReader (schema, XmlNodeType.Document, null));
			ds1.Tables[1].ReadXmlSchema (new XmlTextReader (schema, XmlNodeType.Document, null));
			ReadTest1Check (ds1);

			// ReadXml() should also be the same
			DataSet ds2 = new DataSet ();
			ds2.Tables.Add (new DataTable ("Table1"));
			ds2.Tables.Add (new DataTable ("Table2"));
			ds2.Tables[0].ReadXml (new XmlTextReader (schema, XmlNodeType.Document, null));
			ds2.Tables[1].ReadXml (new XmlTextReader (schema, XmlNodeType.Document, null));
			ReadTest1Check (ds2);
		}

		private void ReadTest1Check (DataSet ds)
		{
			AssertDataSet ("dataset", ds, "NewDataSet", 2, 1);
			AssertDataTable ("tbl1", ds.Tables [0], "Table1", 3, 0, 0, 1, 1, 0);
			AssertDataTable ("tbl2", ds.Tables [1], "Table2", 3, 0, 1, 0, 1, 0);

			DataRelation rel = ds.Relations [0];
			AssertDataRelation ("rel", rel, "Rel1", false,
				new string [] {"Column1_3"},
				new string [] {"Column2_1"}, true, true);
			AssertUniqueConstraint ("uc", rel.ParentKeyConstraint, 
				"Constraint1", false, new string [] {"Column1_3"});
			AssertForeignKeyConstraint ("fk", rel.ChildKeyConstraint, "Rel1",
				AcceptRejectRule.None, Rule.Cascade, Rule.Cascade,
				new string [] {"Column2_1"}, 
				new string [] {"Column1_3"});
		}

		[Test]
		public void TestSampleFileSimpleTables ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add (new DataTable ("foo"));
			ds.Tables[0].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test005.xsd"));
			AssertDataSet ("005", ds, "NewDataSet", 1, 0);
			DataTable dt = ds.Tables [0];
			AssertDataTable ("tab", dt, "foo", 2, 0, 0, 0, 0, 0);
			AssertDataColumn ("attr", dt.Columns [0], "attr", true, false, 0, 1, "attr", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("text", dt.Columns [1], "foo_text", false, false, 0, 1, "foo_text", MappingType.SimpleContent, typeof (long), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);

			ds = new DataSet ();
			ds.Tables.Add (new DataTable ("foo"));
			ds.Tables[0].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test006.xsd"));
			AssertDataSet ("006", ds, "NewDataSet", 1, 0);
			dt = ds.Tables [0];
			AssertDataTable ("tab", dt, "foo", 2, 0, 0, 0, 0, 0);
			AssertDataColumn ("att1", dt.Columns ["att1"], "att1", true, false, 0, 1, "att1", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, /*0*/-1, String.Empty, false, false);
			AssertDataColumn ("att2", dt.Columns ["att2"], "att2", true, false, 0, 1, "att2", MappingType.Attribute, typeof (int), 2, String.Empty, -1, String.Empty, /*1*/-1, String.Empty, false, false);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestSampleFileComplexTablesExp1 () {
			// Nested simple type element
			DataSet ds = new DataSet ();
			ds.Tables.Add (new DataTable ("uno"));
			ds.Tables[0].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test007.xsd"));
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestSampleFileComplexTablesExp2 () {
			// External simple type element
			DataSet ds = new DataSet ();
			ds.Tables.Add (new DataTable ("uno"));
			ds.Tables[0].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test008.xsd"));
		}

		[Test]
		[Category ("NotWorking")]
		public void TestSampleFileImportSimple ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add( new DataTable("foo"));
			XmlTextReader xtr = null;
			try {
				xtr = new XmlTextReader (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test010.xsd"));
				xtr.XmlResolver = null;
				ds.Tables[0].ReadXmlSchema (xtr);
			} finally {
				if (xtr != null)
					xtr.Close ();
			}
			AssertDataSet ("010", ds, "NewDataSet", 1, 0);
			DataTable dt = ds.Tables [0];
			AssertDataTable ("root", dt, "foo", 1, 0, 0, 0, 0, 0);
			AssertDataColumn ("simple", dt.Columns [0], "bar", false, false, 0, 1, "bar", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
		}

		[Test]
		[Category ("NotWorking")]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestSampleFileComplexTables2 ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add (new DataTable ("e"));
			ds.Tables.Add (new DataTable ("root"));
			ds.Tables[0].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test011.xsd"));
			DataTable dt = ds.Tables[0];
			AssertDataTable ("root", dt, "e", 3, 0, 0, 0, 0, 0);
			AssertDataColumn ("attr", dt.Columns [0], "a", true, false, 0, 1, "a", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, "http://xsdtesting", 0, String.Empty, false, false);
			AssertDataColumn ("simple", dt.Columns [1], "e_text", false, false, 0, 1, "e_text", MappingType.SimpleContent, typeof (decimal), DBNull.Value, String.Empty, -1, "", 1, String.Empty, false, false);
			AssertDataColumn ("hidden", dt.Columns [2], "root_Id", true, false, 0, 1, "root_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, "", 2, String.Empty, false, false);
			ds.Tables[1].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test011.xsd"));
		}

		[Test]
		public void TestSampleFileComplexTables3 ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add (new DataTable ("e"));
			ds.Tables[0].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test013.xsd"));
			DataTable dt = ds.Tables [0];
			AssertDataTable ("root", dt, "e", 2, 0, 0, 0, 0, 0);
			AssertDataColumn ("attr", dt.Columns [0], "a", true, false, 0, 1, "a", MappingType.Attribute, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("simple", dt.Columns [1], "e_text", false, false, 0, 1, "e_text", MappingType.SimpleContent, typeof (decimal), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);
		}

		// bug #58744
		[Test]
		public void TestSampleFileXPath ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add (new DataTable ("Track"));
			ds.Tables[0].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test103.xsd"));
		}

		[Test]
		[Category ("NotWorking")]
		public void TestAnnotatedRelation1 ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add (new DataTable ("p"));
			ds.Tables.Add (new DataTable ("c"));
			ds.Tables[0].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test101.xsd"));
			ds.Tables[1].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test101.xsd"));

			DataTable dt = ds.Tables [0];
			AssertDataTable ("parent_table", dt, "p", 2, 0, 0, 1, 0, 0);
			AssertDataColumn ("pk", dt.Columns [0], "pk", false, false, 0, 1, "pk", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);

			dt = ds.Tables [1];
			AssertDataTable ("child_table", dt, "c", 2, 0, 1, 0, 0, 0);
			AssertDataColumn ("fk", dt.Columns [0], "fk", false, false, 0, 1, "fk", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);

			AssertDataRelation ("rel", ds.Relations [0], "rel", false, new string [] {"pk"}, new string [] {"fk"}, false, false);
		}

		[Test]
		[Category ("NotWorking")]
		public void TestAnnotatedRelation2 ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add (new DataTable ("p"));
			ds.Tables.Add (new DataTable ("c"));
			ds.Tables[0].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test102.xsd"));
			ds.Tables[1].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test102.xsd"));

			DataTable dt = ds.Tables [0];
			AssertDataTable ("parent_table", dt, "p", 2, 0, 0, 1, 0, 0);
			AssertDataColumn ("pk", dt.Columns [0], "pk", false, false, 0, 1, "pk", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);

			dt = ds.Tables [1];
			AssertDataTable ("child_table", dt, "c", 2, 0, 1, 0, 0, 0);
			AssertDataColumn ("fk", dt.Columns [0], "fk", false, false, 0, 1, "fk", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);

			AssertDataRelation ("rel", ds.Relations [0], "rel", true, new string [] {"pk"}, new string [] {"fk"}, false, false);
		}

		[Test]
		[Category ("NotWorking")]
		public void RepeatableSimpleElement ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add (new DataTable ("Foo"));
			ds.Tables.Add (new DataTable ("Bar"));
			ds.Tables[0].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test012.xsd"));
			ds.Tables[1].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test012.xsd"));
			AssertDataSet ("012", ds, "NewDataSet", 2, 1);

			DataTable dt = ds.Tables [0];
			AssertDataTable ("parent", dt, "Foo", 1, 0, 0, 1, 1, 1);
			AssertDataColumn ("key", dt.Columns [0], "Foo_Id", false, true, 0, 1, "Foo_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, true);

			dt = ds.Tables [1];
			AssertDataTable ("repeated", dt, "Bar", 2, 0, 1, 0, 1, 0);
			AssertDataColumn ("data", dt.Columns [0], "Bar_Column", false, false, 0, 1, "Bar_Column", MappingType.SimpleContent, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("refkey", dt.Columns [1], "Foo_Id", true, false, 0, 1, "Foo_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);

			AssertDataRelation ("rel", ds.Relations [0], "Foo_Bar", true, new string [] {"Foo_Id"}, new string [] {"Foo_Id"}, true, true);
		}

		[Test]
		[Category ("NotWorking")]
		public void TestMoreThanOneRepeatableColumns ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add (new DataTable ("root"));
			ds.Tables.Add (new DataTable ("x"));
			ds.Tables.Add (new DataTable ("y"));
			ds.Tables[0].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test014.xsd"));
			ds.Tables[1].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test014.xsd"));
			ds.Tables[2].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test014.xsd"));
			AssertDataSet ("014", ds, "NewDataSet", 3, 2);

			DataTable dt = ds.Tables [0];
			AssertDataTable ("parent", dt, "root", 1, 0, 0, 2, 1, 1);
			AssertDataColumn ("key", dt.Columns [0], "root_Id", false, true, 0, 1, "root_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, true);

			dt = ds.Tables [1];
			AssertDataTable ("repeated", dt, "x", 2, 0, 1, 0, 1, 0);
			AssertDataColumn ("data_1", dt.Columns [0], "x_Column", false, false, 0, 1, "x_Column", MappingType.SimpleContent, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("refkey_1", dt.Columns [1], "root_Id", true, false, 0, 1, "root_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);

			dt = ds.Tables [2];
			AssertDataTable ("repeated", dt, "y", 2, 0, 1, 0, 1, 0);
			AssertDataColumn ("data", dt.Columns [0], "y_Column", false, false, 0, 1, "y_Column", MappingType.SimpleContent, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("refkey", dt.Columns [1], "root_Id", true, false, 0, 1, "root_Id", MappingType.Hidden, typeof (int), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);

			AssertDataRelation ("rel", ds.Relations [0], "root_x", true, new string [] {"root_Id"}, new string [] {"root_Id"}, true, true);

			AssertDataRelation ("rel", ds.Relations [1], "root_y", true, new string [] {"root_Id"}, new string [] {"root_Id"}, true, true);
		}

		[Test]
		public void ReadConstraints ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add (new DataTable ());
			ds.Tables.Add (new DataTable ());
			ds.Tables[0].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test015.xsd"));
			ds.Tables[1].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test015.xsd"));
			Assert.AreEqual (0, ds.Relations.Count, "#1");
			Assert.AreEqual (1, ds.Tables [0].Constraints.Count, "#2" );
			Assert.AreEqual (0, ds.Tables [1].Constraints.Count, "#3" );
			Assert.AreEqual ("Constraint1", ds.Tables [0].Constraints [0].ConstraintName, "#4");
		}

		[Test]
		[Category ("NotWorking")]
		public void ReadAnnotatedRelations_MultipleColumns ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add (new DataTable ());
			ds.Tables.Add (new DataTable ());
			ds.Tables[0].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test016.xsd"));
			ds.Tables[1].ReadXmlSchema (TestResourceHelper.GetFullPathOfResource ("Test/System.Data/schemas/test016.xsd"));
			Assert.AreEqual (1, ds.Relations.Count, "#1");
			Assert.AreEqual ("rel", ds.Relations [0].RelationName, "#2");
			Assert.AreEqual (2, ds.Relations [0].ParentColumns.Length, "#3");
			Assert.AreEqual (2, ds.Relations [0].ChildColumns.Length, "#4");
			Assert.AreEqual(0, ds.Tables [0].Constraints.Count, "#5" );
			Assert.AreEqual(0, ds.Tables [1].Constraints.Count, "#6" );
			AssertDataRelation ("TestRel", ds.Relations [0], "rel", false, new String[] {"col 1","col2"},
					new String[] {"col1","col  2"}, false, false);
		}
	}
}

