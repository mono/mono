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
		void Validate (string xml, string xsd)
		{
			XmlSchema schema = XmlSchema.Read (new StringReader (xsd), null);
			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.ValidationType = ValidationType.Schema;
			settings.Schemas.Add (schema);
			XmlReader reader = XmlReader.Create (new StringReader (xml), settings);
			while (reader.Read ())
				;
		}

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
			string xml = @"<myDoc foo='12' bar='January 1st 1900'/>";
			Validate (xml, xsd);
		}

		[Test]
		public void Bug469713 ()
		{
			string xsd = @"<xs:schema elementFormDefault='qualified' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:element name='Message'>
    <xs:complexType>
      <xs:all>
        <xs:element name='MyDateTime' nillable='true' type='xs:dateTime' />
      </xs:all>
    </xs:complexType>
  </xs:element>
</xs:schema>";
			string xml = @"<Message xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:noNamespaceSchemaLocation='test.xsd'>
        <MyDateTime xsi:nil='true'></MyDateTime>
</Message>";
			Validate (xml, xsd);
		}

		[Test]
		public void Bug496192_496205 ()
		{
			using (var xmlr = new StreamReader ("Test/XmlFiles/496192.xml"))
				using (var xsdr = new StreamReader ("Test/XmlFiles/496192.xsd"))
					Validate (xmlr.ReadToEnd (), xsdr.ReadToEnd ());
		}
		
		[Test]		
		public void Bug501666 ()
		{
			string xsd = @"
			<xs:schema id='Settings'
				targetNamespace='foo'                
				xmlns='foo'
				xmlns:xs='http://www.w3.org/2001/XMLSchema'>

				<xs:element name='Settings' type='Settings'/>

				<xs:complexType name='Settings'>
					<xs:attribute name='port' type='PortNumber' use='required'/>
				</xs:complexType>
                
				<xs:simpleType name='PortNumber'>
					<xs:restriction base='xs:positiveInteger'>
						<xs:minInclusive value='1'/>
						<xs:maxInclusive value='65535'/>
					</xs:restriction>
				</xs:simpleType>
			</xs:schema>";

			string xml = @"<Settings port='1337' xmlns='foo'/>";

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			doc.Schemas.Add (XmlSchema.Read (XmlReader.Create (new StringReader (xsd)), null));
			doc.Validate (null);
		}

		public void Bug502251 ()
		{
			string xsd = @"
   <xs:schema id='foo' targetNamespace='foo' 
     elementFormDefault='qualified' 
     xmlns='foo'     
     xmlns:xs='http://www.w3.org/2001/XMLSchema'>

 <xs:group name='LayoutElementTypes'>
  <xs:choice>   
   <xs:element name='Rows' type='Rows' />
   <xs:element name='Conditional' type='Conditional' />   
  </xs:choice>
 </xs:group>

 <xs:complexType name='Element' abstract='true'>
  <xs:attribute name='id' type='xs:ID' use='optional'/>
 </xs:complexType>

 <xs:complexType name='SingleChildElement' abstract='true'>
  <xs:complexContent>
   <xs:extension base='Element'>
    <xs:group ref='LayoutElementTypes' minOccurs='1' maxOccurs='1' />
   </xs:extension>
  </xs:complexContent>
 </xs:complexType>

 <xs:complexType name='Rows'>
  <xs:complexContent>
   <xs:extension base='Element'>
    <xs:sequence minOccurs='1' maxOccurs='unbounded'>
     <xs:element name='Row' type='Row' />
    </xs:sequence>    
         </xs:extension>
  </xs:complexContent>
 </xs:complexType> 

   <xs:complexType name='Row'>
  <xs:complexContent>
   <xs:extension base='SingleChildElement'>    
   </xs:extension>    
  </xs:complexContent>
 </xs:complexType>

 <xs:complexType name='Conditional'>
  <xs:complexContent>
   <xs:extension base='Element'>    
   </xs:extension>
  </xs:complexContent>
 </xs:complexType>

 <xs:complexType name='Layout'>
  <xs:complexContent>
   <xs:extension base='SingleChildElement'>
   </xs:extension>
  </xs:complexContent>
 </xs:complexType>

 <xs:element name='Layout' type='Layout' />
</xs:schema>";

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (@"<Layout xmlns='foo'>
  <Rows>
    <Row><Conditional/></Row>     
  </Rows>
</Layout>");

			XmlSchema schema = XmlSchema.Read (XmlReader.Create (new StringReader (xsd)), null);

			doc.Schemas.Add (schema);
			doc.Validate (null);
		}

		[Test]
		public void Bug557452 ()
		{
			string xsd = @"
			<xs:schema id='Settings'
				targetNamespace='foo'
				xmlns='foo'
				xmlns:xs='http://www.w3.org/2001/XMLSchema'>

				<xs:element name='Settings' type='Settings'/>

				<xs:complexType name='Settings'>
					<xs:attribute name='port' type='PortNumber' use='required'/>
				</xs:complexType>

				<xs:simpleType name='PortNumber'>
					<xs:restriction base='xs:decimal'>
						<xs:minInclusive value='1'/>
						<xs:maxInclusive value='65535'/>
					</xs:restriction>
				</xs:simpleType>
			</xs:schema>";

			string xml = @"<Settings port='1337' xmlns='foo'/>";

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (xml);
			doc.Schemas.Add (XmlSchema.Read (XmlReader.Create (new StringReader (xsd)), null));
			doc.Validate (null);
		}

		[Test]
		public void Bug584664 ()
		{
			Validate (File.ReadAllText ("Test/XmlFiles/xsd/584664a.xml"), File.ReadAllText ("Test/XmlFiles/xsd/584664a.xsd"));
			Validate (File.ReadAllText ("Test/XmlFiles/xsd/584664b.xml"), File.ReadAllText ("Test/XmlFiles/xsd/584664b.xsd"));
		}

		[Test]
		public void MultipleMissingIds ()
		{
			var schema = XmlSchema.Read (new StringReader (@"<?xml version=""1.0"" encoding=""utf-8""?>
<xs:schema targetNamespace=""urn:multiple-ids"" elementFormDefault=""qualified"" xmlns=""urn:multiple-ids"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
	<xs:element name=""root"">
		<xs:complexType>
			<xs:sequence minOccurs=""0"" maxOccurs=""unbounded"">
				<xs:element name=""item"">
					<xs:complexType>
						<xs:attribute name=""id"" type=""xs:ID"" />
						<xs:attribute name=""parent"" type=""xs:IDREF"" />
					</xs:complexType>
				</xs:element>
			</xs:sequence>
		</xs:complexType>
	</xs:element>
</xs:schema>"), null);
			var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<root xmlns=""urn:multiple-ids"">
	<item id=""id2"" parent=""id1"" />
	<item id=""id3"" parent=""id1"" />
	<item id=""id1"" parent=""id1"" />
</root>";
			var document = new XmlDocument ();
			document.LoadXml (xml);
			document.Schemas = new XmlSchemaSet ();
			document.Schemas.Add (schema);
			document.Validate (null);
		}

		[Test]
		public void FacetsOnBaseSimpleContentRestriction ()
		{
			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.Schemas.Add (null, "Test/XmlFiles/595947.xsd");
			settings.ValidationType = ValidationType.Schema;
			settings.Schemas.Compile ();

			Validate ("TEST 1.1", 1, "0123456789", "0123456789", settings, false);
			Validate ("TEST 1.2", 1, "0123456789***", "0123456789", settings, true);
			Validate ("TEST 1.3", 1, "0123456789", "0123456789***", settings, true);

			Validate ("TEST 2.1", 2, "0123456789", "0123456789", settings, false);
			Validate ("TEST 2.2", 2, "0123456789***", "0123456789", settings, true);
			Validate ("TEST 2.3", 2, "0123456789", "0123456789***", settings, true);

			Validate ("TEST 3.1", 3, "0123456789", "0123456789", settings, false);
			Validate ("TEST 3.2", 3, "0123456789***", "0123456789", settings, true);
			Validate ("TEST 3.3", 3, "0123456789", "0123456789***", settings, true);
		}

		void Validate (string testName, int testNumber, string idValue, string elementValue, XmlReaderSettings settings, bool shouldFail)
		{
			string content = string.Format ("<MyTest{0} Id=\"{1}\">{2}</MyTest{0}>", testNumber, idValue, elementValue);
			try
			{
				XmlReader reader = XmlReader.Create (new StringReader (content), settings);
				XmlDocument document = new XmlDocument ();
				document.Load (reader);
				document.Validate (null);
			} catch (Exception e) {
				if (!shouldFail)
					throw;
				return;
			}
			if (shouldFail)
				Assert.Fail (testName + " should fail");
		}

		[Test]
		public void Bug676993 ()
		{
			Validate (File.ReadAllText ("Test/XmlFiles/676993.xml"), File.ReadAllText ("Test/XmlFiles/676993.xsd"));
		}
		
		[Test]
		public void Bug10245 ()
		{
			string xsd = @"
	<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='urn:foo'>
	  <xs:element name='root'>
		<xs:complexType>
		  <xs:attribute name='d' default='v' use='optional' />
		</xs:complexType>
	  </xs:element>
	</xs:schema>";
			string xml = "<root xmlns='urn:foo' />";
			var xrs = new XmlReaderSettings () { ValidationType = ValidationType.Schema };
			xrs.Schemas.Add (XmlSchema.Read (new StringReader (xsd), null));
			var xr = XmlReader.Create (new StringReader (xml), xrs);
			xr.Read ();
			bool more;
			Assert.AreEqual (2, xr.AttributeCount, "#1");
			int i = 0;
			for (more = xr.MoveToFirstAttribute (); more; more = xr.MoveToNextAttribute ())
				i++;
			Assert.AreEqual (2, i, "#2");
		}
	}
}

#endif
