//
// DataSetReadXmlSchemaTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//

using System;
using System.IO;
using System.Data;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataSetReadXmlSchemaTest : DataSetAssertion
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

		[Test]
		public void SingleElementTreatmentDifference ()
		{
			// This is one of the most complicated case. When the content
			// type particle of 'Root' element is a complex element, it
			// is DataSet element. Otherwise, it is just a data table.
			//
			// But also note that there is another test named
			// LocaleOnRootWithoutIsDataSet(), that tests if locale on
			// the (mere) data table modifies *DataSet's* locale.
			string xsbase = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' id='hoge'>
	<xs:element name='Root'> <!-- becomes table -->
		<xs:complexType>
			<xs:choice maxOccurs='unbounded'>
				{0}
			</xs:choice>
		</xs:complexType>
	</xs:element>
</xs:schema>";
			string simple = "<xs:element name='Child' type='xs:string' />";
			string complex = @"<xs:element name='Child'>
					<xs:complexType>
						<xs:attribute name='a1' />
						<xs:attribute name='a2'/>
					</xs:complexType>
				</xs:element>";

			DataSet ds = new DataSet ();

			string xs = String.Format (xsbase, simple);
			ds.ReadXmlSchema (new StringReader (xs));
			AssertDataSet ("simple", ds, "hoge", 1);
			AssertDataTable ("simple", ds.Tables [0], "Root", 1, 0);

			ds = new DataSet ();
			xs = String.Format (xsbase, complex);
			ds.ReadXmlSchema (new StringReader (xs));
			AssertDataSet ("complex", ds, "Root", 1);
			AssertDataTable ("complex", ds.Tables [0], "Child", 2, 0);
		}

		[Test]
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
			ds.ReadXmlSchema (new StringReader (xs));
			// Here "unusedType" table is never imported.
			AssertDataSet ("ds", ds, "hoge", 1);
			AssertDataTable ("dt", ds.Tables [0], "Root", 1, 0);
		}

		[Test]
		public void SimpleTypeComponentsIgnored ()
		{
			string xs = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	<xs:element name='Root' type='xs:string'/>
	<xs:attribute name='Attr' type='xs:string'/>
</xs:schema>";

			DataSet ds = new DataSet ();
			ds.ReadXmlSchema (new StringReader (xs));
			// nothing is imported.
			AssertDataSet ("ds", ds, "NewDataSet", 0);
		}

		[Test]
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

			// Even if a global element uses a complexType, it will be
			// ignored if the element has msdata:IsDataSet='true'
			string xs = String.Format (xsbase, "true");

			DataSet ds = new DataSet ();
			ds.ReadXmlSchema (new StringReader (xs));
			AssertDataSet ("ds", ds, "Root", 0); // name is "Root"

			// But when explicit msdata:IsDataSet value is "false", then
			// treat as usual.
			xs = String.Format (xsbase, "false");

			ds = new DataSet ();
			ds.ReadXmlSchema (new StringReader (xs));
			AssertDataSet ("ds", ds, "NewDataSet", 1);
		}

		[Test]
		[ExpectedException (typeof (DataException))]
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
			ds.ReadXmlSchema (new StringReader (xs));
		}

		[Test]
		public void IsDataSetOnLocalElementIgnored ()
		{
			string xsbase = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata'>
	<xs:element name='Root' type='unusedType'>
	</xs:element>
	<xs:complexType name='unusedType'>
		<xs:sequence>
			<xs:element name='Child' type='xs:string' msdata:IsDataSet='True' />
		</xs:sequence>
	</xs:complexType>
</xs:schema>";

			// msdata:IsDataSet does not affect even if the value is invalid
			string xs = String.Format (xsbase, "true");

			DataSet ds = new DataSet ();
			ds.ReadXmlSchema (new StringReader (xs));
			// Child should not be regarded as DataSet element
			AssertDataSet ("ds", ds, "NewDataSet", 1);
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
			ds.ReadXmlSchema (new StringReader (xs));
			AssertDataSet ("ds", ds, "NewDataSet", 1);
			AssertEquals ("ja-JP", ds.Locale.Name);
			DataTable dt = ds.Tables [0];
			AssertDataTable ("dt", dt, "Root", 2, 0);
			AssertDataColumn ("col1", dt.Columns [0], "Attr", true, false, 0, 1, "Attr", MappingType.Attribute, typeof (Int64), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
			AssertDataColumn ("col2", dt.Columns [1], "Child", false, false, 0, 1, "Child", MappingType.Element, typeof (string), DBNull.Value, String.Empty, -1, String.Empty, 1, String.Empty, false, false);
		}


		[Test]
		public void ElementHasIdentityConstraint ()
		{
			string constraints = @"
		<xs:key name='key'>
			<xs:selector xpath='./any/string_is_OK/R1'/>
			<xs:field xpath='Child2'/>
		</xs:key>
		<xs:keyref name='kref' refer='key'>
			<xs:selector xpath='.//R2'/>
			<xs:field xpath='Child2'/>
		</xs:keyref>";
			string xsbase = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:msdata='urn:schemas-microsoft-com:xml-msdata'>
	<xs:element name='DS' msdata:IsDataSet='true'>
		<xs:complexType>
			<xs:choice>
				<xs:element ref='R1' />
				<xs:element ref='R2' />
			</xs:choice>
		</xs:complexType>
		{0}
	</xs:element>
	<xs:element name='R1' type='RootType'>
	      {1}
	</xs:element>
	<xs:element name='R2' type='RootType'>
	</xs:element>
	<xs:complexType name='RootType'>
		<xs:choice>
			<xs:element name='Child1' type='xs:string'>
				{2}
			</xs:element>
			<xs:element name='Child2' type='xs:string' />
		</xs:choice>
		<xs:attribute name='Attr' type='xs:integer' />
	</xs:complexType>
</xs:schema>";

			// Constraints on DataSet element.
			// Note that in xs:key xpath is crazy except for the last step
			string xs = String.Format (xsbase, constraints, String.Empty, String.Empty);
			DataSet ds = new DataSet ();
			ds.ReadXmlSchema (new StringReader (xs));
			AssertEquals (1, ds.Relations.Count);

			// Constraints on another global element - just ignored
			xs = String.Format (xsbase, String.Empty, constraints, String.Empty);
			ds = new DataSet ();
			ds.ReadXmlSchema (new StringReader (xs));
			AssertEquals (0, ds.Relations.Count);

			// Constraints on local element - just ignored
			xs = String.Format (xsbase, String.Empty, String.Empty, constraints);
			ds = new DataSet ();
			ds.ReadXmlSchema (new StringReader (xs));
			AssertEquals (0, ds.Relations.Count);
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
			ds.ReadXmlSchema (new StringReader (xs));
			AssertDataSet ("ds", ds, "DS", 3);
			DataTable dt = ds.Tables [0];
			AssertDataTable ("R3", dt, "R3", 3, 0);
			AssertDataColumn ("col1", dt.Columns [0], "Attr", true, false, 0, 1, "Attr", MappingType.Attribute, typeof (Int64), DBNull.Value, String.Empty, -1, String.Empty, 0, String.Empty, false, false);
		}

		[Test]
		public void ReadTest1 ()
		{
			DataSet ds = CreateTestSet ();

			StringWriter sw = new StringWriter ();
			ds.WriteXmlSchema (sw);

			string schema = sw.ToString ();

			// ReadXmlSchema()
			ds = new DataSet ();
			ds.ReadXmlSchema (new XmlTextReader (schema, XmlNodeType.Document, null));
			ReadTest1Check (ds);

			// ReadXml() should also be the same
			ds = new DataSet ();
			ds.ReadXml (new XmlTextReader (schema, XmlNodeType.Document, null));
			ReadTest1Check (ds);
		}

		private void ReadTest1Check (DataSet ds)
		{
			AssertDataSet ("dataset", ds, "NewDataSet", 2);
			AssertDataTable ("tbl1", ds.Tables [0], "Table1", 3, 0);
			AssertDataTable ("tbl2", ds.Tables [1], "Table2", 3, 0);

			DataRelation rel = ds.Relations [0];
			AssertDataRelation ("rel", rel, "Rel1",
				new string [] {"Column1_3"},
				new string [] {"Column2_1"}, true, true);
			AssertUniqueConstraint ("uc", rel.ParentKeyConstraint, 
				"Constraint1", false, new string [] {"Column1_3"});
			AssertForeignKeyConstraint ("fk", rel.ChildKeyConstraint, "Rel1",
				AcceptRejectRule.None, Rule.Cascade, Rule.Cascade,
				new string [] {"Column2_1"}, 
				new string [] {"Column1_3"});
		}
	}
}
