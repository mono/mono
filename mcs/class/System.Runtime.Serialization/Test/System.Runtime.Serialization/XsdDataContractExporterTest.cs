//
// XsdDataContractExporterTest.cs
//
// Author:
//	Ankit Jain  <JAnkit@novell.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;

using System.IO;
using System.Runtime.Serialization;
using NUnit.Framework;
using System.Xml.Schema;
using System.Collections;
using System.Xml.Serialization;
using System.Reflection;
using System.Xml;

using QName = System.Xml.XmlQualifiedName;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class XsdDataContractExporterTest
	{
		internal const string MSSimpleNamespace =
			"http://schemas.microsoft.com/2003/10/Serialization/";
		internal const string MSArraysNamespace =
			"http://schemas.microsoft.com/2003/10/Serialization/Arrays";
		internal const string DefaultClrNamespaceBase =
			"http://schemas.datacontract.org/2004/07/";

		[Test]
		public void Ctor1 ()
		{
			XsdDataContractExporter xdce = new XsdDataContractExporter ();
			Assert.IsNotNull (xdce.Schemas);
		}

		[Test]
		public void PrimitiveType ()
		{
			XsdDataContractExporter xdce = new XsdDataContractExporter ();
			Assert.AreEqual (1, xdce.Schemas.Count);

			Assert.IsNull (xdce.GetSchemaType (typeof (int)));
			Assert.AreEqual (new QName ("int", XmlSchema.Namespace), xdce.GetSchemaTypeName (typeof (int)));

			xdce.Export (typeof (int));
			Assert.IsNull (xdce.GetSchemaType (typeof (int)));
			Assert.AreEqual (new QName ("int", XmlSchema.Namespace), xdce.GetSchemaTypeName (typeof (int)));
		}

		[Test]
		public void CanExportTest ()
		{
			XsdDataContractExporter xdce = new XsdDataContractExporter ();
			Assert.IsTrue (xdce.CanExport (typeof (int)), "#1");
			Assert.IsTrue (xdce.CanExport (typeof (dc)), "#2");

			//No DataContract/Serializable etc -> changed in 3.5
			Assert.IsTrue (xdce.CanExport (this.GetType ()), "#3");
		}

		[Test]
		public void GetSchemaTypeTest ()
		{
			XsdDataContractExporter xdce = new XsdDataContractExporter ();
			Assert.IsNull (xdce.GetSchemaType (typeof (dc)));
			Assert.AreEqual (new QName ("_dc", "http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"), xdce.GetSchemaTypeName (typeof (dc)));
		}

		[Test]
		public void Test2 ()
		{
			XsdDataContractExporter xdce = new XsdDataContractExporter ();
			xdce.Export (typeof (dc));
			XmlSchemaSet set = xdce.Schemas;

			xdce = new XsdDataContractExporter (set);
			try {
				xdce.Export (typeof (dc));
			} catch (XmlSchemaException xe) {
				return;
			} catch (Exception e) {
				Assert.Fail ("Expected XmlSchemaException, but got " + e.GetType ().ToString ());
			}

			Assert.Fail ("Expected XmlSchemaException");
		}

		[Test]
		public void EnumTest ()
		{
			XsdDataContractExporter xdce = new XsdDataContractExporter ();
			xdce.Export (typeof (XColors));

			CheckEnum (xdce.Schemas, colors_qname, new List<string> (new string [] { "_Red" }));
		}

		[Test]
		public void EnumNoDcTest ()
		{
			XsdDataContractExporter xdce = new XsdDataContractExporter ();
			xdce.Export (typeof (EnumNoDc));

			CheckEnum (xdce.Schemas,
				new QName ("EnumNoDc",
					"http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization"),
				new List<string> (new string [] { "Red", "Green", "Blue" }));
		}

		//Test case for class dc
		[Test]
		public void DcTest ()
		{
			XsdDataContractExporter xdce = new XsdDataContractExporter ();
			xdce.Export (typeof (dc));
			CheckDcFull (xdce.Schemas);
		}

		[Test]
		public void Dc3Test ()
		{
			//Check for duplicate dc2 ?
			XsdDataContractExporter xdce = new XsdDataContractExporter ();
			xdce.Export (typeof (dc3));
			CheckDcFull (xdce.Schemas);
		}

		[Test]
		public void Dc3Test2 ()
		{
			//Check for duplicate dc2 ?
			XsdDataContractExporter xdce = new XsdDataContractExporter ();
			xdce.Export (typeof (dc3));
			xdce.Export (typeof (dc3));
			CheckDcFull (xdce.Schemas);
		}

		[Test]
		public void GetSchemaTypeName ()
		{
			var xdce = new XsdDataContractExporter ();
			// bug #670539
			Assert.AreEqual (new XmlQualifiedName ("ArrayOfstring", MSArraysNamespace), xdce.GetSchemaTypeName (typeof (IEnumerable<string>)), "#1");
		}

		//Helper methods

		XmlSchemas GetSchemas (XmlSchemaSet set)
		{
			XmlSchemas schemas = new XmlSchemas ();
			foreach (XmlSchema schema in set.Schemas ())
				schemas.Add (schema);

			return schemas;
		}

		void CheckEnum (XmlSchemaSet schemas, QName qname, List<string> values)
		{
			XmlSchemaSimpleType simple = schemas.GlobalTypes [qname] as XmlSchemaSimpleType;
			Assert.IsNotNull (simple, "#ce1");

			XmlSchemaSimpleTypeRestriction restriction = simple.Content as XmlSchemaSimpleTypeRestriction;
			Assert.IsNotNull (restriction, "#ce2");
			Assert.AreEqual (new QName ("string", XmlSchema.Namespace), restriction.BaseTypeName, "#ce3");

			//Check the values
			Assert.AreEqual (values.Count, restriction.Facets.Count, "#ce4");
			values.Sort ();

			List<string> facets = new List<string> ();
			foreach (XmlSchemaObject obj in restriction.Facets) {
				XmlSchemaEnumerationFacet facet = obj as XmlSchemaEnumerationFacet;
				Assert.IsNotNull (facet, "#ce5");
				facets.Add (facet.Value);
			}

			facets.Sort ();
			for (int i = 0;i < values.Count;i++)
				Assert.AreEqual (values [i], facets [i], "#ce6");

			//Check the corresponding element
			CheckElement (schemas, qname);
		}

		void CheckElement (XmlSchemaSet schemas, QName qname)
		{
			XmlSchemaElement element = schemas.GlobalElements [qname] as XmlSchemaElement;
			Assert.IsNotNull (element, "#c1");
			Assert.IsTrue (element.IsNillable, "#c2");
			Assert.AreEqual (qname, element.SchemaTypeName, "#c3");
		}

		XmlSchemaComplexType GetSchemaComplexType (XmlSchemaSet schemas, QName qname)
		{
			XmlSchemaComplexType type = schemas.GlobalTypes [qname] as XmlSchemaComplexType;
			Assert.IsNotNull (type, "ComplexType " + qname.ToString () + " not found.");

			return type;
		}

		//Check the <element .. > in a sequence
		void CheckElementReference (XmlSchemaObject obj, string name, QName schema_type, bool nillable)
		{
			XmlSchemaElement element = obj as XmlSchemaElement;
			Assert.IsNotNull (element, "XmlSchemaElement not found for " + schema_type.ToString ());

			Assert.AreEqual (name, element.Name, "#v1, Element name did not match");
			//FIXME: Assert.AreEqual (0, element.MinOccurs, "#v0, MinOccurs should be 0 for element '" + name + "'");
			Assert.AreEqual (schema_type, element.SchemaTypeName, "#v2, SchemaTypeName for element '" + element.Name + "' did not match.");
			Assert.AreEqual (nillable, element.IsNillable, "#v3, Element '" + element.Name + "', schema type = '" + schema_type + "' should have nillable = " + nillable);
		}

		void CheckArray (XmlSchemaSet schemas, QName qname, QName element_qname)
		{
			XmlSchemaComplexType type = GetSchemaComplexType (schemas, qname);
			XmlSchemaSequence sequence = type.Particle as XmlSchemaSequence;
			Assert.IsNotNull (sequence, "#ca1");

			Assert.AreEqual (1, sequence.Items.Count, "#ca2, Sequence.Items.Count");
			CheckElementReference (
				sequence.Items [0],
				element_qname.Name,
				element_qname,
				element_qname.Namespace != XmlSchema.Namespace);

			XmlSchemaElement element = (XmlSchemaElement) sequence.Items [0];
			Assert.AreEqual ("unbounded", element.MaxOccursString, "#ca3");

			CheckElement (schemas, qname);
		}

		QName colors_qname = new QName ("_XColors", "http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization");
		QName dc_qname = new QName ("_dc", "http://schemas.datacontract.org/2004/07/MonoTests.System.Runtime.Serialization");
		QName other_qname = new QName ("_other", "http://schemas.datacontract.org/2004/07/OtherNs");


		void CheckDcFull (XmlSchemaSet schemas)
		{
			Assert.IsTrue (schemas.IsCompiled, "#dt0, XmlSchemaSet not compiled");
			XmlSchemaComplexType type = GetSchemaComplexType (schemas, dc_qname);
			XmlSchemaSequence sequence = type.Particle as XmlSchemaSequence;
			Assert.IsNotNull (sequence, "#dt1");

			Assert.AreEqual (5, sequence.Items.Count, "#dt2, Sequence.Items.Count");
			CheckElementReference (sequence.Items [0], "_color",
				colors_qname, false);
			CheckEnum (schemas, colors_qname, new List<string> (new string [] { "_Red" }));

			CheckElementReference (sequence.Items [1], "_foo",
				new QName ("string", XmlSchema.Namespace), true);

			CheckElementReference (sequence.Items [2], "_o",
				new QName ("ArrayOf_other", "http://schemas.datacontract.org/2004/07/OtherNs"), true);
			CheckArray (schemas, new QName ("ArrayOf_other", "http://schemas.datacontract.org/2004/07/OtherNs"), other_qname);

			CheckElementReference (sequence.Items [3], "_single_o",
				new QName ("_other", "http://schemas.datacontract.org/2004/07/OtherNs"), true);
			CheckOther (schemas);

			CheckElementReference (sequence.Items [4], "i_array",
				new QName ("ArrayOfint", "http://schemas.microsoft.com/2003/10/Serialization/Arrays"), true);
			CheckArray (schemas, new QName ("ArrayOfint", "http://schemas.microsoft.com/2003/10/Serialization/Arrays"),
				new QName ("int", XmlSchema.Namespace));

			CheckElement (schemas, dc_qname);
		}

		void CheckOther (XmlSchemaSet schemas)
		{
			XmlSchemaComplexType type = GetSchemaComplexType (schemas, other_qname);
			XmlSchemaSequence sequence = type.Particle as XmlSchemaSequence;
			Assert.IsNotNull (sequence, "#ct0");

			Assert.AreEqual (1, sequence.Items.Count, "#ct1");
			CheckElementReference (sequence.Items [0], "_field_int", new QName ("int", XmlSchema.Namespace), false);
		}

	}

	[DataContract (Name = "_XColors")]
	public enum XColors
	{
		[EnumMember (Value = "_Red")]
		Red,
		Green,
		Blue
	}

	public enum EnumNoDc
	{
		Red,
		Green,
		Blue
	}

	[DataContract (Name = "_dc")]
	public class dc
	{
		[DataMember (Name = "_foo")]
		public string foo;

		int not_used;

		[DataMember (Name = "_color")]
		XColors color;

		//[DataMember]
		public dc me;

		[DataMember (Name = "_o")]
		public OtherNs.other [] o;

		[DataMember (Name = "_single_o")]
		public OtherNs.other single_o;

		[DataMember]
		public int [] i_array;
	}

	[DataContract (Name = "_dc2")]
	public class dc2 : dc
	{
		[DataMember (Name = "_foo2")]
		string foo2;
	}

	[DataContract]
	public abstract class abstract_class
	{
		[DataMember]
		public string foo;
	}

	[Serializable]
	public class base_xs
	{
		public int base_int;
		private string base_string;
	}

	[Serializable]
	public class xs : base_xs
	{
		private string ignore;
		public string useme;
	}

	[DataContract]
	public class dc_with_basexs : base_xs
	{
		[DataMember (Name = "_foo")]
		public string foo;
	}

	[XmlRoot]
	public class xa
	{
		[XmlElement]
		public string foo;

		[XmlAttribute]
		public int bar;
	}

	[DataContract (Name = "_dc3")]
	public class dc3
	{
		[DataMember (Name = "_first")]
		dc2 first;

		[DataMember (Name = "_second")]
		dc2 second;
	}

}

namespace OtherNs
{
	[DataContract (Name = "_other")]
	public class other /*: MonoTests.System.Runtime.Serialization.dc*/
	{
		[DataMember (Name = "_field_int")]
		public int field_int;
	}
}

