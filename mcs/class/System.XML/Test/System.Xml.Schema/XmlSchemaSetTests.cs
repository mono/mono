//
// System.Xml.XmlSchemaSetTests.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2004 Novell Inc.
//
#if NET_2_0

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlSchemaSetTests
	{
		[Test]
		public void Add ()
		{
			XmlSchemaSet ss = new XmlSchemaSet ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' />");
			ss.Add (null, new XmlNodeReader (doc)); // null targetNamespace
			ss.Compile ();

			// same document, different targetNamespace
			ss.Add ("ab", new XmlNodeReader (doc));

			// Add(null, xmlReader) -> targetNamespace in the schema
			doc.LoadXml ("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:foo' />");
			ss.Add (null, new XmlNodeReader (doc));

			Assert.AreEqual (3, ss.Count);

			bool chameleon = false;
			bool ab = false;
			bool urnfoo = false;

			foreach (XmlSchema schema in ss.Schemas ()) {
				if (schema.TargetNamespace == null)
					chameleon = true;
				else if (schema.TargetNamespace == "ab")
					ab = true;
				else if (schema.TargetNamespace == "urn:foo")
					urnfoo = true;
			}
			Assert.IsTrue (chameleon, "chameleon schema missing");
			Assert.IsTrue (ab, "target-remapped schema missing");
			Assert.IsTrue (urnfoo, "target specified in the schema ignored");
		}

		[Test]
		[ExpectedException (typeof (XmlSchemaException))]
		public void AddWrongTargetNamespace ()
		{
			string xsd = @"<xs:schema targetNamespace='urn:foo' xmlns:xs='http://www.w3.org/2001/XMLSchema'><xs:element name='el' type='xs:int' /></xs:schema>";
			string xml = "<el xmlns='urn:foo'>a</el>";
			XmlSchemaSet xss = new XmlSchemaSet ();
			// unlike null, "" is regarded as an explicit
			// empty namespace indication.
			xss.Add ("", new XmlTextReader (new StringReader (xsd)));
		}

		[Test]
		public void AddSchemaThenReader ()
		{
			XmlSchemaSet ss = new XmlSchemaSet ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' />");
			XmlSchema xs = new XmlSchema ();
			xs.TargetNamespace = "ab";
			ss.Add (xs);
			ss.Add ("ab", new XmlNodeReader (doc));
		}

		[Test]
		[Category ("NotWorking")] // How can we differentiate this
		// case and the testcase above?
		[ExpectedException (typeof (ArgumentException))]
		public void AddReaderTwice ()
		{
			XmlSchemaSet ss = new XmlSchemaSet ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' />");
			ss.Add ("ab", new XmlNodeReader (doc));
			ss.Add ("ab", new XmlNodeReader (doc));
		}

		[Test]
		public void AddSchemaTwice ()
		{
			XmlSchemaSet ss = new XmlSchemaSet ();
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:ab' />");
			ss.Add (XmlSchema.Read (new XmlNodeReader (doc), null));
			ss.Add (XmlSchema.Read (new XmlNodeReader (doc), null));
		}

		[Test]
		public void CompilationSettings ()
		{
			Assert.IsNotNull (new XmlSchemaSet ().CompilationSettings);
			new XmlSchemaSet ().CompilationSettings = null;
		}

		[Test]
		public void DisableUpaCheck ()
		{
			string schema = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:complexType name='Foo'>
    <xs:sequence>
      <xs:choice minOccurs='0'>
        <xs:element name='el'/>
      </xs:choice>
      <xs:element name='el' />
    </xs:sequence>
  </xs:complexType>
</xs:schema>";
			XmlSchema xs = XmlSchema.Read (new XmlTextReader (
				schema, XmlNodeType.Document, null), null);
			XmlSchemaSet xss = new XmlSchemaSet ();
			xss.Add (xs);
			xss.CompilationSettings.EnableUpaCheck = false;

			xss.Compile ();
		}

		[Test]
		public void AddRollbackIsCompiled ()
		{
			XmlSchemaSet ss = new XmlSchemaSet ();
			ss.Add (new XmlSchema ());
			ss.Compile ();
			Assert.IsTrue (ss.IsCompiled, "#1");
			XmlSchema sc = new XmlSchema (); // compiled one
			sc.Compile (null);
			ss.Add (sc);
			Assert.IsFalse (ss.IsCompiled, "#2");
			ss.Add (new XmlSchema ()); // not-compiled one
			Assert.IsFalse (ss.IsCompiled, "#3");
			XmlSchema s;

			s = new XmlSchema ();
			s.TargetNamespace = "urn:foo";
			XmlSchemaElement el;
			el = new XmlSchemaElement ();
			el.Name = "root";
			s.Items.Add (el);
			ss.Add (s);

			s = new XmlSchema ();
			s.TargetNamespace = "urn:foo";
			el = new XmlSchemaElement ();
			el.Name = "foo";
			s.Items.Add (el);
			ss.Add (s);
			ss.Compile ();
			Assert.IsTrue (ss.IsCompiled, "#4");
			ss.RemoveRecursive (s);
			Assert.IsTrue (ss.IsCompiled, "#5");
		}

		[Test] // bug #77489
		public void CrossSchemaReferences ()
		{
			string schema1 = @"<xsd:schema id=""Base.Schema"" elementFormDefault=""qualified"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
	<xsd:complexType name=""itemBase"" abstract=""true""> 
		<xsd:attribute name=""id"" type=""xsd:string""
use=""required""/> 
		<xsd:attribute name=""type"" type=""xsd:string""
use=""required""/> 
	</xsd:complexType> 
</xsd:schema>";

			string schema2 = @"<xsd:schema id=""Sub.Schema"" elementFormDefault=""qualified"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
	<xsd:complexType name=""item""> 
		<xsd:complexContent> 
			<xsd:extension base=""itemBase""> 
				<xsd:attribute name=""itemName""
type=""xsd:string"" use=""required""/> 
			</xsd:extension> 
		</xsd:complexContent> 
	</xsd:complexType> 
</xsd:schema>";
			XmlSchemaSet schemas = new XmlSchemaSet ();
			schemas.Add (XmlSchema.Read (new StringReader (schema1), null));
			schemas.Add (XmlSchema.Read (new StringReader (schema2), null));
			schemas.Compile ();
		}

		[Test]
		public void ImportSubstitutionGroupDBR ()
		{
			// This bug happened when
			// 1) a schema imports another schema,
			// 2) there is a substitutionGroup which is involved in
			//    complexContent schema conformance check, and
			// 3) the included schema is already added to XmlSchemaSet.
			XmlSchemaSet xss = new XmlSchemaSet ();
			xss.Add (null, "Test/XmlFiles/xsd/import-subst-dbr-base.xsd");
			xss.Add (null, "Test/XmlFiles/xsd/import-subst-dbr-ext.xsd");
			// should not result in lack of substitutionGroup
			// (and conformance error as its result)
			xss.Compile ();
		}

		[Test]
		public void AddWithNullTargetNS () // bug #571650
		{
			var xsdraw = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'><xs:element name='foo' /></xs:schema>";
			var schemas = new XmlSchemaSet ();
			var xsd = schemas.Add ("", XmlReader.Create (new StringReader (xsdraw)));
			Assert.IsNull (xsd.TargetNamespace, "#1");
		}

		[Test] // part of bug #670945
		public void TwoSchemasInSameDocumentUri ()
		{
			string xsd1 = @"
    <xs:schema
    targetNamespace='http://www.onvif.org/ver10/schema'
    elementFormDefault='qualified'
    xmlns:xs='http://www.w3.org/2001/XMLSchema'
    xmlns:tt='http://www.onvif.org/ver10/schema'>

      <xs:complexType name='SystemDateTime'>
        <xs:sequence>
          <xs:element name='foobar' type='xs:string' minOccurs='0' />
          <xs:element name='Extension' type='tt:SystemDateTimeExtension' minOccurs='0'/>
        </xs:sequence>
        <!-- xs:anyAttribute processContents='lax'/ -->
      </xs:complexType>

      <xs:complexType name='SystemDateTimeExtension'>
        <xs:sequence>
          <xs:any namespace='##any' processContents='lax' minOccurs='0' maxOccurs='unbounded'/>
        </xs:sequence>
      </xs:complexType>

    </xs:schema>";

			string xsd2 = @"
    <xs:schema
      targetNamespace='http://www.onvif.org/ver10/device/wsdl'
      xmlns:xs='http://www.w3.org/2001/XMLSchema'
      xmlns:tt='http://www.onvif.org/ver10/schema'
      xmlns:tds='http://www.onvif.org/ver10/device/wsdl' 
      elementFormDefault='qualified'>
      <xs:element name='GetSystemDateAndTime'>
        <xs:complexType>
          <xs:sequence/>

        </xs:complexType>
      </xs:element>
      <xs:element name='GetSystemDateAndTimeResponse'>
        <xs:complexType>
          <xs:sequence>
            <xs:element name='SystemDateAndTime' type='tt:SystemDateTime' />
          </xs:sequence>
        </xs:complexType>
      </xs:element>
    </xs:schema>";

			var xss = new XmlSchemaSet ();
			var xs1 = XmlSchema.Read (new StringReader (xsd1), null);
			xs1.SourceUri = "http://localhost:8080/dummy.wsdl";
			xs1.LineNumber = 5;
			xss.Add (xs1);
			var xs2 = XmlSchema.Read (new StringReader (xsd2), null);
			xs2.SourceUri = "http://localhost:8080/dummy.wsdl";
			xs2.LineNumber = 50;
			xss.Add (xs2);
			xss.Compile ();
			Assert.IsNotNull (xss.GlobalElements [new XmlQualifiedName ("GetSystemDateAndTimeResponse", "http://www.onvif.org/ver10/device/wsdl")], "#1");
		}
	}
}
#endif
