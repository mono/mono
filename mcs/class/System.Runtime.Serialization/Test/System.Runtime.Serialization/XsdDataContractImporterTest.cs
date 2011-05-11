//
// XsdDataContractImporterTest.cs
//
// Author:
//	Ankit Jain <jankit@novell.com>
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

//
// This test code contains tests for both DataContractSerializer and
// NetDataContractSerializer. The code could be mostly common.
//

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.CSharp;
using NUnit.Framework;

using QName = System.Xml.XmlQualifiedName;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	public class XsdContractImporterTest
	{
		MetadataSet metadata;
		XmlSchemaSet xss;

		[SetUp]
		public void Setup ()
		{
			XmlReader xr = XmlTextReader.Create ("Test/System.Runtime.Serialization/one.xml");
			metadata = MetadataSet.ReadFrom (xr);
			NewXmlSchemaSet ();
		}

		private XmlSchemaSet NewXmlSchemaSet ()
		{
			xss = new XmlSchemaSet ();
			foreach (MetadataSection section in metadata.MetadataSections)
				if (section.Metadata is XmlSchema)
					xss.Add (section.Metadata as XmlSchema);

			Assert.AreEqual (3, xss.Schemas ().Count, "#1");
			return xss;
		}

		private XsdDataContractImporter GetImporter ()
		{
			CodeCompileUnit ccu = new CodeCompileUnit ();
			return new XsdDataContractImporter (ccu);
		}

		[Test]
		public void CtorTest ()
		{
			XsdDataContractImporter xi = new XsdDataContractImporter ();
			Assert.IsNotNull (xi.CodeCompileUnit, "#c01");

			xi = new XsdDataContractImporter (null);
			Assert.IsNotNull (xi.CodeCompileUnit, "#c02");

			xi = new XsdDataContractImporter (new CodeCompileUnit ());
		}


		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetCodeTypeReferenceTest ()
		{
			XsdDataContractImporter xsdi = GetImporter ();
			xsdi.GetCodeTypeReference (new XmlQualifiedName ("dc", "http://schemas.datacontract.org/2004/07/"));
		}

		[Test]
		public void GetCodeTypeReferenceTest2 ()
		{
			NewXmlSchemaSet ();

			Assert.IsFalse (xss.IsCompiled, "#g01");

			XsdDataContractImporter xsdi = GetImporter ();
			xsdi.Import (xss, new XmlQualifiedName ("dc", "http://schemas.datacontract.org/2004/07/"));
			Assert.IsTrue (xss.IsCompiled, "#g02");

			CodeTypeReference type = xsdi.GetCodeTypeReference (new XmlQualifiedName ("dc", "http://schemas.datacontract.org/2004/07/"));

			//FIXME: How should this type ref be checked?
			Assert.IsNotNull (type, "#g03");
			Assert.AreEqual (type.BaseType, "dc", "#g04");
		}

		[Test]
		public void CanImportTest ()
		{
			NewXmlSchemaSet ();
			XsdDataContractImporter xsdi = GetImporter ();

			Assert.IsFalse (xss.IsCompiled, "#ci01");

			Assert.IsTrue (xsdi.CanImport (xss, new XmlQualifiedName ("dc", "http://schemas.datacontract.org/2004/07/")), "#ci02");
			Assert.IsTrue (xss.IsCompiled, "#ci03");

			Assert.IsFalse (xsdi.CanImport (xss, new XmlQualifiedName ("Echo", "http://myns/echo")), "#ci04");
			Assert.IsTrue (xsdi.CanImport (xss, new XmlQualifiedName ("int", "http://www.w3.org/2001/XMLSchema")), "#ci05");

			Assert.IsTrue (xsdi.CanImport (xss), "#ci06");

			Assert.IsTrue (xsdi.CanImport (xss, 
				xss.GlobalElements [new XmlQualifiedName ("dc", "http://schemas.datacontract.org/2004/07/")] as XmlSchemaElement), 
				"#ci07");

			Assert.IsTrue (xsdi.CanImport (xss,
				xss.GlobalElements [new XmlQualifiedName ("Echo", "http://myns/echo")] as XmlSchemaElement),
				"#ci08");
		}

		[Test]
		public void CanImportTest2 ()
		{
			NewXmlSchemaSet ();
			XsdDataContractImporter xsdi = GetImporter ();

			List<XmlQualifiedName> names = new List<XmlQualifiedName> ();
			names.Add (new XmlQualifiedName ("Echo", "http://myns/echo"));
			Assert.IsFalse (xsdi.CanImport (xss, names), "#ci20");

			names.Add (new XmlQualifiedName ("dc", "http://schemas.datacontract.org/2004/07/"));
			Assert.IsFalse (xsdi.CanImport (xss, names), "#ci21");

			names.Clear ();
			names.Add (new XmlQualifiedName ("dc", "http://schemas.datacontract.org/2004/07/"));
			Assert.IsTrue (xsdi.CanImport (xss, names), "#ci22");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CanImportNullTest1 ()
		{
			XsdDataContractImporter xsdi = GetImporter ();
			xsdi.CanImport (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CanImportNullTest2 ()
		{
			XsdDataContractImporter xsdi = GetImporter ();
			xsdi.CanImport (xss, (XmlQualifiedName) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CanImportNullTest3 ()
		{
			XsdDataContractImporter xsdi = GetImporter ();
			xsdi.CanImport (xss, (XmlSchemaElement) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ImportTestNullSchemas ()
		{
			XsdDataContractImporter xsdi = GetImporter ();
			xsdi.Import (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ImportTestNullSchemas2 ()
		{
			XsdDataContractImporter xsdi = GetImporter ();
			xsdi.Import (null, new XmlQualifiedName ("foo"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ImportTestNullSchemas3 ()
		{
			XsdDataContractImporter xsdi = GetImporter ();
			xsdi.Import (null, new List<XmlQualifiedName> ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ImportTestNullTypeName ()
		{
			XsdDataContractImporter xsdi = GetImporter ();
			xsdi.Import (new XmlSchemaSet (), (XmlQualifiedName) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ImportTestNullElement ()
		{
			XsdDataContractImporter xsdi = GetImporter ();
			xsdi.Import (new XmlSchemaSet (), (XmlSchemaElement) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ImportTestNullCollection ()
		{
			XsdDataContractImporter xsdi = GetImporter ();
			xsdi.Import (new XmlSchemaSet (), (ICollection<XmlQualifiedName>) null);
		}

		[Test]
		[Category ("NotWorking")] // importing almost-invalid element. This test is almost missing the point.
		public void ImportTest ()
		{
			XsdDataContractImporter xsdi = GetImporter ();
			XmlSchemaElement element = new XmlSchemaElement();
			Assert.IsTrue (xsdi.CanImport (xss, new QName ("dc", "http://schemas.datacontract.org/2004/07/")), "#i01");
			Assert.IsTrue (xsdi.CanImport (xss, element), "#i01-2");
			Assert.AreEqual (new XmlQualifiedName ("anyType", XmlSchema.Namespace), xsdi.Import (xss, element), "#i02");

			CodeCompileUnit ccu = xsdi.CodeCompileUnit;
			Assert.AreEqual (1, ccu.Namespaces.Count, "#i03");
			Assert.AreEqual ("", ccu.Namespaces [0].Name, "#i04");

			Assert.AreEqual (1, ccu.Namespaces [0].Types.Count, "#i05");

			Dictionary<string, string> mbrs = new Dictionary<string, string> ();
			mbrs.Add ("foo", "System.String");
			CheckDC (ccu.Namespaces [0].Types [0], "dc", mbrs, "#i06");
		}

		[Test]
		public void ImportDataContract1 ()
		{
			NewXmlSchemaSet ();
			Assert.IsFalse (xss.IsCompiled, "#i20");

			XsdDataContractImporter xsdi = GetImporter ();

			xsdi.Import (xss, new XmlQualifiedName ("dc", "http://schemas.datacontract.org/2004/07/"));
			Assert.IsTrue (xss.IsCompiled, "#i21");
			CodeCompileUnit ccu = xsdi.CodeCompileUnit;

			Assert.AreEqual (1, ccu.Namespaces.Count, "#i22");
			Assert.AreEqual ("", ccu.Namespaces [0].Name, "#i23");

			Assert.AreEqual (1, ccu.Namespaces [0].Types.Count, "#i24");

			Dictionary<string, string> mbrs = new Dictionary<string, string> ();
			mbrs.Add ("foo", "System.String");
			CheckDC (ccu.Namespaces [0].Types [0], "dc", mbrs, "#i25");

			ccu.Namespaces.Clear ();
			xsdi.Import (xss, new XmlQualifiedName ("dc", "http://schemas.datacontract.org/2004/07/"));
			//Importing same data contract again with same importer
			Assert.AreEqual (0, ccu.Namespaces.Count, "#i26");
		}

		[Test]
		public void ImportDataContract2 ()
		{
			NewXmlSchemaSet ();
			XsdDataContractImporter xsdi = GetImporter ();

			xss.Compile ();
			xsdi.Import (xss, xss.GlobalElements [new XmlQualifiedName ("Echo", "http://myns/echo")] as XmlSchemaElement);
			CodeCompileUnit ccu = xsdi.CodeCompileUnit;

			Assert.AreEqual (2, ccu.Namespaces.Count, "#i29");
			Dictionary<string, string> args = new Dictionary<string, string> ();
			args.Add ("msg", "System.String");
			args.Add ("num", "System.Int32");
			args.Add ("d", "dc");

			CheckDC (ccu.Namespaces [0].Types [0], "Echo", args, "#i30");

			args.Clear ();
			args.Add ("foo", "System.String");
			CheckDC (ccu.Namespaces [1].Types [0], "dc", args, "#i31");

			ccu.Namespaces.Clear ();
			xsdi.Import (xss, new XmlQualifiedName ("dc", "http://schemas.datacontract.org/2004/07/"));
			Assert.AreEqual (0, ccu.Namespaces.Count);
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void ImportMessageEcho ()
		{
			XsdDataContractImporter xsdi = GetImporter ();
			xsdi.Import (xss, new XmlQualifiedName ("Echo", "http://myns/echo"));
		}

		[Test]
		public void ImportAll ()
		{
			NewXmlSchemaSet ();
			XsdDataContractImporter xsdi = GetImporter ();

			CodeCompileUnit ccu = xsdi.CodeCompileUnit;
			xsdi.Import (xss);

			Assert.AreEqual (2, ccu.Namespaces.Count, "#i40");
			Assert.AreEqual ("myns.echo", ccu.Namespaces [0].Name, "#i41");
			Assert.AreEqual ("", ccu.Namespaces [1].Name, "#i42");

			Assert.AreEqual (4, ccu.Namespaces [0].Types.Count, "#i43");

			/* ns : myns.echo
			 * Messages */
			Dictionary <string, string> args = new Dictionary <string, string> ();
			args.Add ("msg", "System.String");
			args.Add ("num", "System.Int32");
			args.Add ("d", "dc");
			
			CheckDC (ccu.Namespaces [0].Types [0], "Echo", args, "#i44");

			args.Clear ();
			args.Add ("EchoResult", "System.String");
			CheckDC (ccu.Namespaces [0].Types [1], "EchoResponse", args, "#i45");

			args.Clear ();
			args.Add ("it", "System.Int32");
			args.Add ("prefix", "System.String");
			CheckDC (ccu.Namespaces [0].Types [2], "DoubleIt", args, "#i46");

			args.Clear ();
			args.Add ("DoubleItResult", "System.String");
			CheckDC (ccu.Namespaces [0].Types [3], "DoubleItResponse", args, "#i47");

			/* ns: "" */
			args.Clear ();
			args.Add ("foo", "System.String");
			CheckDC (ccu.Namespaces [1].Types [0], "dc", args, "#i48");

			ccu.Namespaces.Clear ();
			xsdi.Import (xss);
			//Importing same data contract again with same importer
			Assert.AreEqual (0, ccu.Namespaces.Count, "#i49");
		}

		[Test]
		public void ImportSkipArrayOfPrimitives ()
		{
			var ccu = new CodeCompileUnit ();
			var xdi = new XsdDataContractImporter (ccu);
			var xss = new XmlSchemaSet ();
			xss.Add (null, "Test/Resources/Schemas/schema1.xsd");
			xss.Add (null, "Test/Resources/Schemas/schema2.xsd");
			xdi.Import (xss);
			var sw = new StringWriter ();
			new CSharpCodeProvider ().GenerateCodeFromCompileUnit (ccu, sw, null);
			Assert.IsTrue (sw.ToString ().IndexOf ("ArrayOfint") < 0, "#1");
		}

		[Test]
		public void ImportGivesAppropriateNamespaces ()
		{
			var ccu = new CodeCompileUnit ();
			var xdi = new XsdDataContractImporter (ccu);
			var xss = new XmlSchemaSet ();
			xss.Add (null, "Test/Resources/Schemas/schema1.xsd");
			xss.Add (null, "Test/Resources/Schemas/schema2.xsd");
			xss.Add (null, "Test/Resources/Schemas/schema3.xsd");
			xdi.Import (xss);
			var sw = new StringWriter ();
			bool t = false, te = false;
			foreach (CodeNamespace cns in ccu.Namespaces) {
				if (cns.Name == "tempuri.org")
					t = true;
				else if (cns.Name == "tempuri.org.ext")
					te = true;
				Assert.AreEqual ("GetSearchDataResponse", cns.Types [0].Name, "#1." + cns.Name);
			}
			Assert.IsTrue (t, "t");
			Assert.IsTrue (t, "te");
		}

		CodeCompileUnit DoImport (params string [] schemaFiles)
		{
			return DoImport (false, schemaFiles);
		}

		CodeCompileUnit DoImport (bool xmlType, params string [] schemaFiles)
		{
			var ccu = new CodeCompileUnit ();
			var xdi = new XsdDataContractImporter (ccu);
			if (xmlType)
				xdi.Options = new ImportOptions () { ImportXmlType = true };
			var xss = new XmlSchemaSet ();
			foreach (var schemaFile in schemaFiles)
				xss.Add (null, schemaFile);
			xdi.Import (xss);

			return ccu;
		}

		string GenerateCode (CodeCompileUnit ccu)
		{
			var sw = new StringWriter ();
			new CSharpCodeProvider ().GenerateCodeFromCompileUnit (ccu, sw, null);
			return sw.ToString ();
		}

		// FIXME: this set of tests need further assertion in each test case. Right now it just checks if things import or fail just fine.

		[Test]
		public void ImportTestX0 ()
		{
			DoImport ("Test/Resources/Schemas/ns0.xsd");
		}

		[Test]
		public void ImportTestX0_2 ()
		{
			var ccu = DoImport (true, "Test/Resources/Schemas/ns0.xsd");
			Assert.IsTrue (GenerateCode (ccu).IndexOf ("class") < 0, "#1");
		}

		[Test]
		public void ImportTestX1 ()
		{
			DoImport ("Test/Resources/Schemas/ns1.xsd");
		}

		[Test]
		public void ImportTestX1_2 ()
		{
			Assert.AreEqual (GenerateCode (DoImport ("Test/Resources/Schemas/ns1.xsd")), GenerateCode (DoImport (true, "Test/Resources/Schemas/ns1.xsd")), "#1");
		}

		[Test]
		public void ImportTestX2 ()
		{
			DoImport ("Test/Resources/Schemas/ns2.xsd");
		}

		[Test]
		public void ImportTestX2_2 ()
		{
			Assert.AreEqual (GenerateCode (DoImport ("Test/Resources/Schemas/ns2.xsd")), GenerateCode (DoImport (true, "Test/Resources/Schemas/ns2.xsd")), "#1");
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void ImportTestX3 ()
		{
			DoImport ("Test/Resources/Schemas/ns3.xsd");
		}

		[Test]
		[Category ("NotWorking")]
		public void ImportTestX3_2 ()
		{
			var ccu = DoImport (true, "Test/Resources/Schemas/ns3.xsd");
			var code = GenerateCode (ccu);
			Assert.IsTrue (code.IndexOf ("class T2") > 0, "#1");
			Assert.IsTrue (code.IndexOf ("IXmlSerializable") > 0, "#2");
			Assert.IsTrue (code.IndexOf ("WriteXml") > 0, "#3");
			Assert.IsTrue (code.IndexOf ("XmlRootAttribute(ElementName=\"E2\", Namespace=\"urn:bar\"") > 0, "#4");
			Assert.IsTrue (code.IndexOf ("XmlSchemaProviderAttribute(\"ExportSchema\")") > 0, "#5");
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void ImportTestX4 ()
		{
			DoImport ("Test/Resources/Schemas/ns4.xsd");
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void ImportTestX5 ()
		{
			DoImport ("Test/Resources/Schemas/ns5.xsd");
		}

		[Test]
		public void ImportTestX6 ()
		{
			DoImport ("Test/Resources/Schemas/ns6.xsd",
				  "Test/Resources/Schemas/ns0.xsd");
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void ImportTestX7 ()
		{
			DoImport ("Test/Resources/Schemas/ns7.xsd",
				  "Test/Resources/Schemas/ns0.xsd");
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void ImportTestX8 ()
		{
			DoImport ("Test/Resources/Schemas/ns8.xsd");
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void ImportTestX9 ()
		{
			DoImport ("Test/Resources/Schemas/ns9.xsd");
		}

		[Test]
		public void ImportTestX10 ()
		{
			DoImport ("Test/Resources/Schemas/ns10.xsd");
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void ImportTestX11 ()
		{
			DoImport ("Test/Resources/Schemas/ns11.xsd");
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void ImportTestX12 ()
		{
			DoImport ("Test/Resources/Schemas/ns12.xsd");
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void ImportTestX13 ()
		{
			DoImport ("Test/Resources/Schemas/ns13.xsd");
		}

		[Test]
		public void ImportTestX14 ()
		{
			DoImport ("Test/Resources/Schemas/ns14.xsd");
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void ImportTestX15 ()
		{
			DoImport ("Test/Resources/Schemas/ns15.xsd");
		}

		[Test]
		public void ImportTestX16 ()
		{
			DoImport ("Test/Resources/Schemas/ns16.xsd");
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void ImportTestX17 ()
		{
			DoImport ("Test/Resources/Schemas/ns17.xsd");
		}

		[Test]
		public void ImportTestX18 ()
		{
			DoImport ("Test/Resources/Schemas/ns18.xsd");
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void ImportTestX19 ()
		{
			DoImport ("Test/Resources/Schemas/ns19.xsd");
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void ImportTestX20 ()
		{
			DoImport ("Test/Resources/Schemas/ns20.xsd");
		}

		[Test]
		public void ImportTestX21 ()
		{
			DoImport ("Test/Resources/Schemas/ns21.xsd");
		}

		[Test]
		public void ImportTestX22 ()
		{
			DoImport ("Test/Resources/Schemas/ns22.xsd");
		}

		[Test]
		public void ImportTestX23 ()
		{
			DoImport ("Test/Resources/Schemas/ns23.xsd");
		}

		[ExpectedException (typeof (InvalidDataContractException))]
		[Test]
		public void ImportTestX24 ()
		{
			DoImport ("Test/Resources/Schemas/ns24.xsd");
		}

		[Test]
		public void ImportTestX25 ()
		{
			DoImport ("Test/Resources/Schemas/ns25.xsd");
		}

		[Test]
		public void ImportTestX26 ()
		{
			DoImport ("Test/Resources/Schemas/ns26.xsd");
		}

		[Test]
		public void ImportTestX27 ()
		{
			DoImport ("Test/Resources/Schemas/ns27.xsd");
		}

		[Test]
		public void ImportTestX28 ()
		{
			DoImport ("Test/Resources/Schemas/ns28.xsd");
		}

		[Test]
		[ExpectedException (typeof (InvalidDataContractException))]
		public void ImportTestX29 ()
		{
			DoImport ("Test/Resources/Schemas/ns29.xsd");
		}

		[Test]
		public void ImportTestX30 ()
		{
			DoImport ("Test/Resources/Schemas/ns30.xsd");
		}

		[Test]
		public void ImportTestX31 ()
		{
			DoImport ("Test/Resources/Schemas/ns31.xsd");
		}

		[Test]
		public void ImportTestX32 ()
		{
			DoImport ("Test/Resources/Schemas/ns32.xsd");
		}

		[Test]
		public void ImportTestX33 ()
		{
			DoImport ("Test/Resources/Schemas/ns33.xsd");
		}

		[Test]
		public void ImportTestX34 ()
		{
			DoImport (true, "Test/Resources/Schemas/ns34.xsd", "Test/Resources/Schemas/ns34_2.xsd");
		}

		/* Helper methods */
		private void CheckDC (CodeTypeDeclaration type, string name, Dictionary<string, string> members, string msg)
		{
			// "dc" DataContract
			Assert.AreEqual (name, type.Name, msg + "d");
			//FIXME: Assert.AreEqual (MemberAttributes.Public, type.Attributes);
			Assert.IsTrue (type.IsClass, msg + "e");
			Assert.IsTrue (type.IsPartial, msg + "f");

			CheckDataContractAttribute (type, msg);
			CheckExtensionData (type, msg);

			foreach (KeyValuePair<string, string> pair in members)
				CheckDataMember (type, pair.Key, pair.Value, true, msg);
		}

		private void CheckExtensionData (CodeTypeDeclaration type, string msg)
		{
			CheckDataMember (type, "extensionDataField", "ExtensionData", "System.Runtime.Serialization.ExtensionDataObject", false, msg);
		}

		private void CheckDataMember (CodeTypeDeclaration type, string name, string type_name, bool check_attr, string msg)
		{
			CheckDataMember (type, name + "Field", name, type_name, check_attr, msg);
		}
		 
		private void CheckDataMember (CodeTypeDeclaration type, string field_name, string prop_name, string type_name, bool check_attr, string msg)
		{
			// "field"
			CodeMemberProperty p = FindMember (type, prop_name) as CodeMemberProperty;
			Assert.IsNotNull (p, msg + "-dm0");
			Assert.IsTrue (p.HasGet, msg + "-dm1");
			Assert.IsTrue (p.HasSet, msg + "-dm2");
			Assert.AreEqual (type_name, p.Type.BaseType, msg + "-dm3");

			CodeMemberField f = FindMember (type, field_name) as CodeMemberField;
			Assert.IsNotNull (f, msg + "-dm4");
			Assert.AreEqual (type_name, f.Type.BaseType, "-dm5");

			if (check_attr)
				CheckDataContractAttribute (type, msg);
		}

		private void CheckDataContractAttribute (CodeTypeDeclaration type, string msg)
		{
			// DebuggerStepThrouAttribute is insignificant. So, no reason to check the attribute count.
			// Assert.AreEqual (3, type.CustomAttributes.Count, msg + "a");

			// DebuggerStepThroughAttribute - skip it

			//GeneratedCodeAttribute
			var l = new List<CodeAttributeDeclaration> ();
			foreach (CodeAttributeDeclaration a in type.CustomAttributes)
				l.Add (a);
			Assert.IsTrue (l.Any (a => a.Name == "System.CodeDom.Compiler.GeneratedCodeAttribute"), msg + "b");

			var ca = l.FirstOrDefault (a => a.Name == "System.Runtime.Serialization.DataContractAttribute");
			Assert.IsNotNull (ca, msg + "b");
			Assert.AreEqual (2, ca.Arguments.Count, msg + "d");
		}

		CodeTypeMember FindMember (CodeTypeDeclaration type, string name)
		{
			foreach (CodeTypeMember m in type.Members)
				if (m.Name == name)
					return m;
			return null;
		}

		[Test]
		public void ImportXsdBuiltInTypes ()
		{
			DoImport ("Test/Resources/Schemas/xml.xsd");
		}
	}

}
