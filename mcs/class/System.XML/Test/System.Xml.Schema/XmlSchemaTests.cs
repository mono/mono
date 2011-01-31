//
// System.Xml.XmlSchemaTests.cs
//
// Author:
//   Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C) 2002 Atsushi Enomoto
//

using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using NUnit.Framework;

namespace MonoTests.System.Xml
{	
	[TestFixture]
	public class XmlSchemaTests : XmlSchemaAssertion
	{
		static readonly bool StrictMsCompliant = Environment.GetEnvironmentVariable ("MONO_STRICT_MS_COMPLIANT") == "yes";

		[Test]
		public void TestRead ()
		{
			XmlSchema schema = GetSchema ("Test/XmlFiles/xsd/1.xsd");
			Assert.AreEqual (6, schema.Items.Count);

			bool fooValidated = false;
			bool barValidated = false;
			string ns = "urn:bar";

			foreach (XmlSchemaObject obj in schema.Items) {
				XmlSchemaElement element = obj as XmlSchemaElement;
				if (element == null)
					continue;
				if (element.Name == "Foo") {
					AssertElement (element, "Foo", 
						XmlQualifiedName.Empty, null,
						QName ("string", XmlSchema.Namespace), null);
					fooValidated = true;
				}
				if (element.Name == "Bar") {
					AssertElement (element, "Bar",
						XmlQualifiedName.Empty, null, QName ("FugaType", ns), null);
					barValidated = true;
				}
			}
			Assert.IsTrue (fooValidated);
			Assert.IsTrue (barValidated);
		}

		[Test]
		public void TestReadFlags ()
		{
			XmlSchema schema = GetSchema ("Test/XmlFiles/xsd/2.xsd");
			schema.Compile (null);
			XmlSchemaElement el = schema.Items [0] as XmlSchemaElement;
			Assert.IsNotNull (el);
			Assert.AreEqual (XmlSchemaDerivationMethod.Extension, el.Block);

			el = schema.Items [1] as XmlSchemaElement;
			Assert.IsNotNull (el);
			Assert.AreEqual (XmlSchemaDerivationMethod.Extension |
				XmlSchemaDerivationMethod.Restriction, el.Block);
		}

		[Test]
		public void TestWriteFlags ()
		{
			XmlSchema schema = GetSchema ("Test/XmlFiles/xsd/2.xsd");
			StringWriter sw = new StringWriter ();
			XmlTextWriter xtw = new XmlTextWriter (sw);
			schema.Write (xtw);
		}

		[Test]
		public void TestCompile ()
		{
			XmlQualifiedName qname;
			XmlSchemaComplexContentExtension xccx;
			XmlSchemaComplexType cType;
			XmlSchemaSequence seq;

			XmlSchema schema = GetSchema ("Test/XmlFiles/xsd/1.xsd");
//			Assert.IsTrue (!schema.IsCompiled);
			schema.Compile (null);
			Assert.IsTrue (schema.IsCompiled);
			string ns = "urn:bar";

			XmlSchemaElement foo = (XmlSchemaElement) schema.Elements [QName ("Foo", ns)];
			Assert.IsNotNull (foo);
			XmlSchemaDatatype stringDatatype = foo.ElementType as XmlSchemaDatatype;
			Assert.IsNotNull (stringDatatype);

			// HogeType
			qname = QName ("HogeType", ns);
			cType = schema.SchemaTypes [qname] as XmlSchemaComplexType;
			Assert.IsNotNull (cType);
			Assert.IsNull (cType.ContentModel);
			AssertCompiledComplexType (cType, qname, 0, 0,
				false, null, true, XmlSchemaContentType.ElementOnly);
			seq = cType.ContentTypeParticle as XmlSchemaSequence;
			Assert.IsNotNull (seq);
			Assert.AreEqual (2, seq.Items.Count);
			XmlSchemaElement refFoo = seq.Items [0] as XmlSchemaElement;
			AssertCompiledElement (refFoo, QName ("Foo", ns), stringDatatype);

			// FugaType
			qname = QName ("FugaType", ns);
			cType = schema.SchemaTypes [qname] as XmlSchemaComplexType;
			Assert.IsNotNull (cType);
			xccx = cType.ContentModel.Content as XmlSchemaComplexContentExtension;
			AssertCompiledComplexContentExtension (
				xccx, 0, false, QName ("HogeType", ns));

			AssertCompiledComplexType (cType, qname, 0, 0,
				false, typeof (XmlSchemaComplexContent),
				true, XmlSchemaContentType.ElementOnly);
			Assert.IsNotNull (cType.BaseSchemaType);

			seq = xccx.Particle as XmlSchemaSequence;
			Assert.IsNotNull (seq);
			Assert.AreEqual (1, seq.Items.Count);
			XmlSchemaElement refBaz = seq.Items [0] as XmlSchemaElement;
			Assert.IsNotNull (refBaz);
			AssertCompiledElement (refBaz, QName ("Baz", ""), stringDatatype);

			qname = QName ("Bar", ns);
			XmlSchemaElement element = schema.Elements [qname] as XmlSchemaElement;
			AssertCompiledElement (element, qname, cType);
		}

		[Test]
		[ExpectedException (typeof (XmlSchemaException))]
		public void TestCompile_ZeroLength_TargetNamespace ()
		{
			XmlSchema schema = new XmlSchema ();
			schema.TargetNamespace = string.Empty;
			Assert.IsTrue (!schema.IsCompiled);

			// MS.NET 1.x: The Namespace '' is an invalid URI.
			// MS.NET 2.0: The targetNamespace attribute cannot have empty string as its value.
			schema.Compile (null);
		}

		[Test]
		[ExpectedException (typeof (XmlSchemaException))]
		public void TestCompileNonSchema ()
		{
			XmlTextReader xtr = new XmlTextReader ("<root/>", XmlNodeType.Document, null);
			XmlSchema schema = XmlSchema.Read (xtr, null);
			xtr.Close ();
		}

		[Test]
		public void TestSimpleImport ()
		{
			XmlSchema schema = XmlSchema.Read (new XmlTextReader ("Test/XmlFiles/xsd/3.xsd"), null);
			Assert.AreEqual ("urn:foo", schema.TargetNamespace);
			XmlSchemaImport import = schema.Includes [0] as XmlSchemaImport;
			Assert.IsNotNull (import);

			schema.Compile (null);
			Assert.AreEqual (4, schema.Elements.Count);
			Assert.IsNotNull (schema.Elements [QName ("Foo", "urn:foo")]);
			Assert.IsNotNull (schema.Elements [QName ("Bar", "urn:foo")]);
			Assert.IsNotNull (schema.Elements [QName ("Foo", "urn:bar")]);
			Assert.IsNotNull (schema.Elements [QName ("Bar", "urn:bar")]);
			
		}

		[Test]
		public void TestSimpleMutualImport ()
		{
			XmlReader r = new XmlTextReader ("Test/XmlFiles/xsd/inter-inc-1.xsd");
			try {
				XmlSchema.Read (r, null).Compile (null);
			} finally {
				r.Close ();
			}
		}

		[Test]
		public void TestQualification ()
		{
			XmlSchema schema = XmlSchema.Read (new XmlTextReader ("Test/XmlFiles/xsd/5.xsd"), null);
			schema.Compile (null);
			XmlSchemaElement el = schema.Elements [QName ("Foo", "urn:bar")] as XmlSchemaElement;
			Assert.IsNotNull (el);
			XmlSchemaComplexType ct = el.ElementType as XmlSchemaComplexType;
			XmlSchemaSequence seq = ct.ContentTypeParticle as XmlSchemaSequence;
			XmlSchemaElement elp = seq.Items [0] as XmlSchemaElement;
			Assert.AreEqual (QName ("Bar", ""), elp.QualifiedName);

			schema = XmlSchema.Read (new XmlTextReader ("Test/XmlFiles/xsd/6.xsd"), null);
			schema.Compile (null);
			el = schema.Elements [QName ("Foo", "urn:bar")] as XmlSchemaElement;
			Assert.IsNotNull (el);
			ct = el.ElementType as XmlSchemaComplexType;
			seq = ct.ContentTypeParticle as XmlSchemaSequence;
			elp = seq.Items [0] as XmlSchemaElement;
			Assert.AreEqual (QName ("Bar", "urn:bar"), elp.QualifiedName);
		}

		[Test]
		public void TestWriteNamespaces ()
		{
			XmlDocument doc = new XmlDocument ();
			XmlSchema xs;
			StringWriter sw;
			XmlTextWriter xw;

			// empty
			xs = new XmlSchema ();
			sw = new StringWriter ();
			xw = new XmlTextWriter (sw);
			xs.Write (xw);
			doc.LoadXml (sw.ToString ());
			Assert.AreEqual ("<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" />", doc.DocumentElement.OuterXml, "#1");

			// TargetNamespace
			xs = new XmlSchema ();
			sw = new StringWriter ();
			xw = new XmlTextWriter (sw);
			xs.TargetNamespace = "urn:foo";
			xs.Write (xw);
			doc.LoadXml (sw.ToString ());
			Assert.AreEqual ("<xs:schema xmlns:tns=\"urn:foo\" targetNamespace=\"urn:foo\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" />", doc.DocumentElement.OuterXml, "#2");

			// Zero-length TargetNamespace
			xs = new XmlSchema ();
			sw = new StringWriter ();
			xw = new XmlTextWriter (sw);
			xs.TargetNamespace = string.Empty;
			xs.Write (xw);
			doc.LoadXml (sw.ToString ());
			Assert.AreEqual ("<xs:schema targetNamespace=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" />", doc.DocumentElement.OuterXml, "#2b");

			// XmlSerializerNamespaces
			xs = new XmlSchema ();
			sw = new StringWriter ();
			xw = new XmlTextWriter (sw);
			xs.Namespaces.Add ("hoge", "urn:hoge");
			xs.Write (xw);
			doc.LoadXml (sw.ToString ());
			// commenting out. .NET 2.0 outputs xs:schema instead of schema, that also makes sense.
			// Assert.AreEqual ("<schema xmlns:hoge=\"urn:hoge\" xmlns=\"http://www.w3.org/2001/XMLSchema\" />", doc.DocumentElement.OuterXml, "#3");

			// TargetNamespace + XmlSerializerNamespaces
			xs = new XmlSchema ();
			sw = new StringWriter ();
			xw = new XmlTextWriter (sw);
			xs.TargetNamespace = "urn:foo";
			xs.Namespaces.Add ("hoge", "urn:hoge");
			xs.Write (xw);
			doc.LoadXml (sw.ToString ());
			// commenting out. .NET 2.0 outputs xs:schema instead of schema, that also makes sense.
			// Assert.AreEqual ("<schema xmlns:hoge=\"urn:hoge\" targetNamespace=\"urn:foo\" xmlns=\"http://www.w3.org/2001/XMLSchema\" />", doc.DocumentElement.OuterXml, "#4");

			// Add XmlSchema.Namespace to XmlSerializerNamespaces
			xs = new XmlSchema ();
			sw = new StringWriter ();
			xw = new XmlTextWriter (sw);
			xs.Namespaces.Add ("a", XmlSchema.Namespace);
			xs.Write (xw);
			doc.LoadXml (sw.ToString ());
			Assert.AreEqual ("<a:schema xmlns:a=\"http://www.w3.org/2001/XMLSchema\" />", doc.DocumentElement.OuterXml, "#5");

			// UnhandledAttributes + XmlSerializerNamespaces
			xs = new XmlSchema ();
			sw = new StringWriter ();
			xw = new XmlTextWriter (sw);
			XmlAttribute attr = doc.CreateAttribute ("hoge");
			xs.UnhandledAttributes = new XmlAttribute [] {attr};
			xs.Namespaces.Add ("hoge", "urn:hoge");
			xs.Write (xw);
			doc.LoadXml (sw.ToString ());
			// commenting out. .NET 2.0 outputs xs:schema instead of schema, that also makes sense.
			// Assert.AreEqual ("<schema xmlns:hoge=\"urn:hoge\" hoge=\"\" xmlns=\"http://www.w3.org/2001/XMLSchema\" />", doc.DocumentElement.OuterXml, "#6");

			// Adding xmlns to UnhandledAttributes -> no output
			xs = new XmlSchema ();
			sw = new StringWriter ();
			xw = new XmlTextWriter (sw);
			attr = doc.CreateAttribute ("xmlns");
			attr.Value = "urn:foo";
			xs.UnhandledAttributes = new XmlAttribute [] {attr};
			xs.Write (xw);
			doc.LoadXml (sw.ToString ());
			Assert.AreEqual ("<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" />", doc.DocumentElement.OuterXml, "#7");
		}

		[Category ("NotWorking")]
		[Test]
		public void TestWriteNamespaces2 ()
		{
			string xmldecl = "<?xml version=\"1.0\" encoding=\"utf-16\"?>";
			XmlSchema xs = new XmlSchema ();
			XmlSerializerNamespaces nss =
				new XmlSerializerNamespaces ();
			StringWriter sw;
			sw = new StringWriter ();
			xs.Write (new XmlTextWriter (sw));
			Assert.AreEqual (xmldecl + "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" />", sw.ToString (), "#1");

			xs.Namespaces = nss;
			sw = new StringWriter ();
			xs.Write (new XmlTextWriter (sw));
			Assert.AreEqual (xmldecl + "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" />", sw.ToString (), "#2");

			nss.Add ("foo", "urn:foo");
			sw = new StringWriter ();
			xs.Write (new XmlTextWriter (sw));
			// commenting out. .NET 2.0 outputs xs:schema instead of schema, that also makes sense.
			// Assert.AreEqual (xmldecl + "<schema xmlns:foo=\"urn:foo\" xmlns=\"http://www.w3.org/2001/XMLSchema\" />", sw.ToString (), "#3");

			nss.Add ("", "urn:foo");
			sw = new StringWriter ();
			xs.Write (new XmlTextWriter (sw));
			// commenting out. .NET 2.0 outputs xs:schema instead of q1:schema, that also makes sense.
			// Assert.AreEqual (xmldecl + "<q1:schema xmlns:foo=\"urn:foo\" xmlns=\"urn:foo\" xmlns:q1=\"http://www.w3.org/2001/XMLSchema\" />", sw.ToString (), "#4");

			nss.Add ("q1", "urn:q1");
			sw = new StringWriter ();
			xs.Write (new XmlTextWriter (sw));
			//Not sure if testing for exact order of these name spaces is
			// relevent, so using less strict test that passes on MS.NET
			//Assert.AreEqual (xmldecl + "<q2:schema xmlns:foo=\"urn:foo\" xmlns:q1=\"urn:q1\" xmlns=\"urn:foo\" xmlns:q2=\"http://www.w3.org/2001/XMLSchema\" />", sw.ToString ());
			Assert.IsTrue (sw.ToString ().IndexOf ("xmlns:q1=\"urn:q1\"") != -1, "q1");
		}

		[Test]
		public void ReaderPositionAfterRead ()
		{
			string xsd = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' elementFormDefault='qualified'>  <xs:element name='test' type='xs:integer'/></xs:schema>";
			XmlTextReader xtr = new XmlTextReader (xsd, XmlNodeType.Document, null);
			xtr.Read ();
			XmlSchema xs = XmlSchema.Read (xtr, null);
			Assert.AreEqual (XmlNodeType.EndElement, xtr.NodeType);
		}

		[Test]
		// bug #76865
		public void AmbiguityDetectionOnChameleonAnyOther ()
		{
			string xsd = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
<xs:complexType name='TestType'>
  <xs:sequence>
    <xs:any namespace='##other' minOccurs='0' />
    <xs:element name='Item' /> 
    <xs:any namespace='##other' minOccurs='0' />
  </xs:sequence> 
</xs:complexType>
</xs:schema>";
			XmlSchema.Read (new XmlTextReader (xsd, XmlNodeType.Document, null), null);
		}

		[Test]
		// bug #77685
		public void ReadDoesNotIgnoreDocumentationEmptyElement ()
		{
			string schemaxml = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:element name='choice'>
    <xs:annotation><xs:documentation /></xs:annotation>
  </xs:element>
</xs:schema>";
			XmlTextReader tr = new XmlTextReader (
				schemaxml, XmlNodeType.Document, null);
			XmlSchema schema = XmlSchema.Read (tr, null);
			XmlSchemaElement element =
				schema.Items [0] as XmlSchemaElement;
			XmlSchemaAnnotation annotation = element.Annotation;
			XmlSchemaDocumentation doc =
				annotation.Items [0] as XmlSchemaDocumentation;
			Assert.AreEqual (0, doc.Markup.Length);
		}


		[Test]
		// bug #77687
		public void CompileFillsSchemaPropertyInExternal ()
		{
			string schemaFileName = "Test/XmlFiles/xsd/77687.xsd";
			XmlTextReader tr = new XmlTextReader (schemaFileName);

			XmlSchema schema = XmlSchema.Read (tr, null);
			XmlSchemaInclude inc = (XmlSchemaInclude) schema.Includes [0];
			Assert.IsNull (inc.Schema);
			schema.Compile (null);
			tr.Close ();
			Assert.IsNotNull (inc.Schema);
		}

		[Test]
		// bug #78985 (contains two identical field path "@key" in 
		// two different keys where one is in scope within another)
		public void DuplicateKeyFieldAttributePath ()
		{
			string schemaFileName = "Test/XmlFiles/xsd/78985.xsd";
			string xmlFileName = "Test/XmlFiles/xsd/78985.xml";
			XmlTextReader tr = new XmlTextReader (schemaFileName);

			XmlValidatingReader vr = new XmlValidatingReader (
				new XmlTextReader (xmlFileName));
			vr.Schemas.Add (XmlSchema.Read (tr, null));
			while (!vr.EOF)
				vr.Read ();
		}

		[Test]
		public void ThreeLevelNestedInclusion ()
		{
			XmlTextReader r = new XmlTextReader ("Test/XmlFiles/xsd/361818.xsd");
			try {
				XmlSchema xs = XmlSchema.Read (r, null);
				xs.Compile (null);
			} finally {
				r.Close ();
			}
		}

		[Test] // bug #502115
		public void ExtensionRedefineAttribute1 ()
		{
			const string xml = "<Bar xmlns='foo'/>";

			XmlSchema schema = GetSchema ("Test/XmlFiles/xsd/extension-attr-redefine-1.xsd");

#if NET_2_0
			XmlSchemaSet xss = new XmlSchemaSet ();
			xss.Add (schema);
			if (StrictMsCompliant) {
				xss.Compile ();
			} else {
				try {
					xss.Compile ();
					Assert.Fail ();
				} catch (XmlSchemaException) {
				}
				return;
			}

			StringReader sr = new StringReader (xml);

			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.ValidationType = ValidationType.Schema;
			settings.Schemas = xss;
			XmlReader vr = XmlReader.Create (sr, settings);
#else
			if (StrictMsCompliant) {
				schema.Compile (null);
			} else {
				try {
					schema.Compile (null);
					Assert.Fail ();
				} catch (XmlSchemaException) {
				}
				return;
			}

			XmlValidatingReader vr = new XmlValidatingReader (xml,
				XmlNodeType.Document, null);
			vr.Schemas.Add (schema);
			vr.ValidationType = ValidationType.Schema;
#endif

			try {
				vr.Read ();
				Assert.Fail ();
			} catch (XmlSchemaException) {
			}
		}

		[Test] // bug #502115
		public void ExtensionRedefineAttribute2 ()
		{
			const string xml = "<Bar xmlns='foo'/>";

			XmlSchema schema = GetSchema ("Test/XmlFiles/xsd/extension-attr-redefine-2.xsd");

#if NET_2_0
			XmlSchemaSet xss = new XmlSchemaSet ();
			xss.Add (schema);
			xss.Compile ();

			StringReader sr = new StringReader (xml);

			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.ValidationType = ValidationType.Schema;
			settings.Schemas = xss;
			XmlReader vr = XmlReader.Create (sr, settings);
#else
			schema.Compile (null);

			XmlValidatingReader vr = new XmlValidatingReader (xml,
				XmlNodeType.Document, null);
			vr.Schemas.Add (schema);
			vr.ValidationType = ValidationType.Schema;
#endif

			while (vr.Read ()) ;
		}

		[Test] // bug #502115
		public void ExtensionRedefineAttribute3 ()
		{
			const string xml = "<Bar xmlns='foo'/>";

			XmlSchema schema = GetSchema ("Test/XmlFiles/xsd/extension-attr-redefine-3.xsd");

#if NET_2_0
			XmlSchemaSet xss = new XmlSchemaSet ();
			xss.Add (schema);
			if (StrictMsCompliant) {
				xss.Compile ();
			} else {
				try {
					xss.Compile ();
					Assert.Fail ();
				} catch (XmlSchemaException) {
				}
				return;
			}

			StringReader sr = new StringReader ("<Bar xmlns='foo'/>");

			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.ValidationType = ValidationType.Schema;
			settings.Schemas = xss;
			XmlReader vr = XmlReader.Create (sr, settings);
#else
			if (StrictMsCompliant) {
				schema.Compile (null);
			} else {
				try {
					schema.Compile (null);
					Assert.Fail ();
				} catch (XmlSchemaException) {
				}
				return;
			}

			XmlValidatingReader vr = new XmlValidatingReader (xml,
				XmlNodeType.Document, null);
			vr.Schemas.Add (schema);
			vr.ValidationType = ValidationType.Schema;
#endif

			while (vr.Read ()) ;
		}

#if NET_2_0

		internal class XmlTestResolver : XmlResolver
		{			
			Uri receivedUri;

			public override ICredentials Credentials
			{
			    set { throw new NotSupportedException (); }
			}

			public override Uri ResolveUri (Uri baseUri, string relativeUri)
			{
			    return new Uri (relativeUri);
			}
			
			public Uri ReceivedUri
			{
				get { return receivedUri; }
			}

			public override object GetEntity (Uri absoluteUri, string role, Type ofObjectToReturn)
			{
				receivedUri = absoluteUri;
				
				return null;
			}
		}	
		
		[Test]
		public void TestResolveUri ()
		{
			XmlSchemaSet schemaSet = new XmlSchemaSet ();
			FileStream stream = new FileStream ("Test/XmlFiles/xsd/resolveUriSchema.xsd", FileMode.Open);
			schemaSet.Add ("http://tempuri.org/resolveUriSchema.xsd", new XmlTextReader (stream));

			XmlTestResolver resolver = new XmlTestResolver ();		
			
			XmlReaderSettings settings = new XmlReaderSettings ();			
			settings.Schemas.XmlResolver = resolver; 
			settings.Schemas.Add (schemaSet);
			settings.ValidationType = ValidationType.Schema;
			settings.ValidationFlags = XmlSchemaValidationFlags.ProcessInlineSchema | XmlSchemaValidationFlags.ProcessSchemaLocation;
			XmlReader reader = XmlReader.Create (stream, settings);
			
			try
			{
				reader.Read ();		
			}
			catch (XmlException)
			{
				// do nothing - we are expecting this exception because the test xmlresolver returns null from its 
				// GetEntity method.
			}
			
			Assert.AreEqual ("assembly://MyAssembly.Name/MyProjectNameSpace/objects.xsd", resolver.ReceivedUri.OriginalString);
		}

		[Test]
		public void TestImportNoSchemaLocation()
		{
			XmlSchemaSet schemaSet = new XmlSchemaSet ();
			schemaSet.Add (GetSchema ("Test/XmlFiles/xsd/importNamespaceTest.xsd"));
			schemaSet.Add (GetSchema ("Test/XmlFiles/xsd/importedNamespace.xsd"));
			
			XmlReaderSettings settings = new XmlReaderSettings ();
			settings.Schemas.Add (schemaSet);
			settings.ValidationType = ValidationType.Schema;
			
			XmlReader reader = XmlReader.Create ("Test/XmlFiles/xsd/xsdimporttest.xml", settings);
			
			// Parse the file. 
			while (reader.Read()) {}
		}
#endif

		[Test]
		public void TestImportSchemaThatIncludesAnother ()
		{
			XmlSchema xs = GetSchema ("Test/XmlFiles/xsd/importNamespaceTest2.xsd");
			xs.Compile (null);
		}
	}
}
