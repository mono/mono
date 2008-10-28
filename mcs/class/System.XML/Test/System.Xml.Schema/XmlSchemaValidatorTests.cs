//
// XmlSchemaValidatorTests.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2008 Novell Inc.
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
	public class XmlSchemaValidatorTests
	{
		[Test]
		public void XsdAnyToSkipAttributeValidation ()
		{
			// bug #358408
			XmlSchemaSet schemas = new XmlSchemaSet ();
			schemas.Add (null, "Test/XmlFiles/xsd/358408.xsd");
			XmlSchemaValidator v = new XmlSchemaValidator (
				new NameTable (),
				schemas,
				new XmlNamespaceManager (new NameTable ()),
				XmlSchemaValidationFlags.ProcessIdentityConstraints);
			v.Initialize ();
			v.ValidateWhitespace (" ");
			XmlSchemaInfo info = new XmlSchemaInfo ();
			ArrayList list = new ArrayList ();

			v.ValidateElement ("configuration", "", info, null, null, null, null);
			v.GetUnspecifiedDefaultAttributes (list);
			v.ValidateEndOfAttributes (info);

			v.ValidateWhitespace (" ");

			v.ValidateElement ("host", "", info, null, null, null, null);
			v.ValidateAttribute ("auto-start", "", "true", info);
			list.Clear ();
			v.GetUnspecifiedDefaultAttributes (list);
			v.ValidateEndOfAttributes (info);
			v.ValidateEndElement (null);//info);

			v.ValidateWhitespace (" ");

			v.ValidateElement ("service-managers", "", info, null, null, null, null);
			list.Clear ();
			v.GetUnspecifiedDefaultAttributes (list);
			v.ValidateEndOfAttributes (info);

			v.ValidateWhitespace (" ");

			v.ValidateElement ("service-manager", "", info, null, null, null, null);
			list.Clear ();
			v.GetUnspecifiedDefaultAttributes (list);
			v.ValidateEndOfAttributes (info);

			v.ValidateWhitespace (" ");

			v.ValidateElement ("foo", "", info, null, null, null, null);
			v.ValidateAttribute ("bar", "", "", info);
		}

		[Test]
		public void SkipInvolved () // bug #422581
		{
			XmlReader schemaReader = XmlReader.Create ("Test/XmlFiles/xsd/422581.xsd");
			XmlSchema schema = XmlSchema.Read (schemaReader, null);
			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.ValidationType = ValidationType.Schema;
			settings.Schemas.Add (schema);
			XmlReader reader = XmlReader.Create ("Test/XmlFiles/xsd/422581.xml", settings);
			while (reader.Read ());
		}

		[Test]
		public void Bug433774 ()
		{
			string xsd = @"<xs:schema targetNamespace='urn:foo' xmlns='urn:foo' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:element name='Root'>
    <xs:complexType>
      <xs:sequence></xs:sequence>
      <xs:attribute name='version' type='xs:string' fixed='3' />
    </xs:complexType>
  </xs:element>
</xs:schema>";
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml ("<Root version='3' xmlns='urn:foo'/>");
			XmlSchemaSet schemaSet = new XmlSchemaSet();
			schemaSet.Add (XmlSchema.Read (XmlReader.Create (new StringReader (xsd)), null));
			doc.Schemas = schemaSet;
			XmlNode root = doc.DocumentElement;
			doc.Validate (null, root);
		}

		[Test]
		[ExpectedException (typeof (XmlSchemaValidationException))]
		public void Bug435206 ()
		{
			string xsd = @"<xs:schema attributeFormDefault='unqualified' elementFormDefault='qualified' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:element name='myDoc'>
    <xs:complexType>
      <xs:attribute name='foo' type='xs:unsignedLong' use='required' />
      <xs:attribute name='bar' type='xs:dateTime' use='required' />
    </xs:complexType>
  </xs:element>
</xs:schema>";

			XmlSchema schema = XmlSchema.Read (new StringReader (xsd), null);
			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.ValidationType = ValidationType.Schema;
			settings.Schemas.Add (schema);
			XmlReader reader = XmlReader.Create (new StringReader (@"<myDoc foo='12' bar='January 1st 1900'/>"), settings);
			while (reader.Read ())
				;
		}
	}
}

#endif
