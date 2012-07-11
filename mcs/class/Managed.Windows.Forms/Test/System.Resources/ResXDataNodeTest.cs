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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// ResXFileRefTest.cs: Unit Tests for ResXFileRef.
//
// Authors:
//	Andreia Gaita	(avidigal@novell.com)
//  Gary Barnett (2012)


#if NET_2_0
using System;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Resources;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Collections;

using NUnit.Framework;
using System.ComponentModel.Design;
using System.Runtime.Serialization.Formatters.Binary;

namespace MonoTests.System.Resources
{
	[TestFixture]
	public class ResXDataNodeTest : MonoTests.System.Windows.Forms.TestHelper
	{
        string _tempDirectory;
        string _otherTempDirectory;

        [Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorEx1 ()
		{
			ResXDataNode d = new ResXDataNode (null, (object)null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorEx2 ()
		{
			ResXDataNode d = new ResXDataNode (null, (ResXFileRef) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorEx3 ()
		{
			ResXDataNode d = new ResXDataNode ("", (object) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorEx4 ()
		{
			ResXDataNode d = new ResXDataNode ("", (ResXFileRef) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorEx5 ()
		{
			ResXDataNode d = new ResXDataNode ("", new ResXFileRef ("filename", "typename"));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ConstructorEx6 ()
		{
			ResXDataNode d = new ResXDataNode ("name", new notserializable ());
		}
        
		[Test]
		public void WriteRead1 ()
		{
			ResXResourceWriter rw = new ResXResourceWriter ("resx.resx");
			serializable ser = new serializable ("aaaaa", "bbbbb");
			ResXDataNode dn = new ResXDataNode ("test", ser);
			dn.Comment = "comment";
			rw.AddResource (dn);
			rw.Close ();

			bool found = false;
			ResXResourceReader rr = new ResXResourceReader ("resx.resx");
            rr.UseResXDataNodes = true;
            IDictionaryEnumerator en = rr.GetEnumerator ();
			while (en.MoveNext ()) {
                ResXDataNode node = ((DictionaryEntry)en.Current).Value as ResXDataNode;
                if (node == null)
                    break;
				serializable o = node.GetValue ((AssemblyName[]) null) as serializable;
				if (o != null) {
					found = true;
					Assert.AreEqual (ser, o, "#A1");
                    Assert.AreEqual ("comment", node.Comment, "#A3");
				}

			}
			rr.Close ();

			Assert.IsTrue (found, "#A2 - Serialized object not found on resx");
		}
        
		[Test]
		public void ConstructorResXFileRef()
		{
            ResXDataNode node = GetNodeFileRefToIcon ();
            Assert.AreEqual (Path.Combine (_tempDirectory, "32x32.ico") , node.FileRef.FileName, "#A1");
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, node.FileRef.TypeName, "#A2");
			Assert.AreEqual ("test", node.Name, "#B1");
		}
        

        [Test]
        public void GetValueEmbeddedNullAssemblyNames ()
        {
            ResXDataNode node = GetNodeEmdeddedIcon ();

            Object ico = node.GetValue ((AssemblyName[])null);
            Assert.IsNotNull (ico, "#A1");
            Assert.IsInstanceOfType (typeof (Icon), ico, "#A2");
        }

        [Test]
        public void GetValueEmbeddedNullITypeResolutionService ()
        {
            ResXDataNode node = GetNodeEmdeddedIcon ();

            Object ico = node.GetValue ((ITypeResolutionService)null);
            Assert.IsNotNull (ico, "#A1");
            Assert.IsInstanceOfType (typeof (Icon), ico, "#A2");
        }

        [Test]
        public void GetValueTypeNameEmbeddedNullAssemblyNames ()
        {
            ResXDataNode node = GetNodeEmdeddedIcon ();

            string name = node.GetValueTypeName ((AssemblyName[])null);
            Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
        }

        [Test]
        public void GetValueTypeNameEmbeddedNullITypeResolutionService ()
        {

            ResXDataNode node = GetNodeEmdeddedIcon ();

            string name = node.GetValueTypeName ((ITypeResolutionService)null);
            Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
        }

        [Test]
        public void GetValueTypeNameEmbeddedWrongITypeResolutionService ()
        {

            ResXDataNode node = GetNodeEmdeddedIcon ();

            string name = node.GetValueTypeName (new DummyTypeResolutionService ());
            Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
        }

        [Test]
        public void GetValueEmbeddedWrongITypeResolutionService ()
        {
            ResXDataNode node = GetNodeEmdeddedIcon ();

            Object ico = node.GetValue (new DummyTypeResolutionService ());
            Assert.IsNotNull (ico, "#A1");
            Assert.IsInstanceOfType (typeof (Icon), ico, "#A2");
        }

        [Test]
        public void GetValueTypeNameEmbeddedWrongAssemblyNames ()
        {

            ResXDataNode node = GetNodeEmdeddedIcon ();

            AssemblyName[] ass = new AssemblyName[1];

            ass[0] = new AssemblyName ("System.Design");

            string name = node.GetValueTypeName (ass);
            Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
        }

        [Test]
        public void GetValueEmbeddedWrongAssemblyNames ()
        {
            ResXDataNode node = GetNodeEmdeddedIcon ();

            AssemblyName[] ass = new AssemblyName[1];

            ass[0] = new AssemblyName ("System.Design");

            Object ico = node.GetValue (ass);
            Assert.IsNotNull (ico, "#A1");
            Assert.IsInstanceOfType (typeof (Icon), ico, "#A2");
        }

        [Test]
        public void DoesNotRequireResXFileToBeOpen_Serializable ()
        {
            
            serializable ser = new serializable ("aaaaa", "bbbbb");
            ResXDataNode dn = new ResXDataNode ("test", ser);
            
			string resXFile = GetResXFileWithNode (dn,"resx.resx");

            ResXResourceReader rr = new ResXResourceReader (resXFile);
            rr.UseResXDataNodes = true;
            IDictionaryEnumerator en = rr.GetEnumerator ();
            en.MoveNext (); 

            ResXDataNode node = ((DictionaryEntry) en.Current).Value as ResXDataNode;
            rr.Close ();

            File.Delete ("resx.resx");

            Assert.IsNotNull (node,"#A1");

            serializable o = node.GetValue ((AssemblyName []) null) as serializable;
            
            Assert.IsNotNull (o, "#A2");
        }

        [Test]
        public void DoesNotRequireResXFileToBeOpen_TypeConverter ()
        {
            
            ResXDataNode dn = new ResXDataNode ("test", 34L);
            
			string resXFile = GetResXFileWithNode (dn,"resx.resx");

            ResXResourceReader rr = new ResXResourceReader (resXFile);
            rr.UseResXDataNodes = true;
            IDictionaryEnumerator en = rr.GetEnumerator ();
            en.MoveNext ();

            ResXDataNode node = ((DictionaryEntry) en.Current).Value as ResXDataNode;
            rr.Close ();

            File.Delete ("resx.resx");

            Assert.IsNotNull (node, "#A1");

            object o = node.GetValue ((AssemblyName []) null);

            Assert.IsInstanceOfType (typeof (long), o, "#A2");
            Assert.AreEqual (34L, o, "#A3");
        }

        //FIXME: should move following tests to files associated with ResXResourceReader Tests

		[Test,ExpectedException (typeof(TypeLoadException))]
        public void AssemblyNamesPassedToResourceReaderDoesNotAffectResXDataNode_TypeConverter ()
        {
            string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };
            
			string resXFile = GetFileFromString ("test.resx", convertableResXWithoutAssemblyName);

            using (ResXResourceReader rr = new ResXResourceReader (resXFile, assemblyNames)) {
	            rr.UseResXDataNodes = true;
	            IDictionaryEnumerator en = rr.GetEnumerator ();
	            en.MoveNext ();

	            ResXDataNode node = ((DictionaryEntry) en.Current).Value as ResXDataNode;
	            
	            Assert.IsNotNull (node, "#A1");

				//should raise exception 
	            object o = node.GetValue ((AssemblyName []) null);
			}
        }

		[Test]
        public void AssemblyNamesPassedToResourceReaderAffectsDictionary_TypeConverter ()
        {
            string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };
            
			string resXFile = GetFileFromString ("test.resx", convertableResXWithoutAssemblyName);

            using (ResXResourceReader rr = new ResXResourceReader (resXFile, assemblyNames)) {
	            IDictionaryEnumerator en = rr.GetEnumerator ();
	            en.MoveNext ();

	            object obj = ((DictionaryEntry) en.Current).Value;
	            
	            Assert.IsNotNull (obj, "#A1");

				Assert.AreEqual ("DummyAssembly.Convertable, " + aName, obj.GetType ().AssemblyQualifiedName, "#A2");

			}
        }

        [Test]
        public void ITRSPassedToResourceReaderDoesNotAffectResXDataNode_TypeConverter ()
        {
            
            ResXDataNode dn = new ResXDataNode ("test", 34L);
            
			string resXFile = GetResXFileWithNode (dn,"resx.resx");

            ResXResourceReader rr = new ResXResourceReader (resXFile, new AlwaysReturnIntTypeResolutionService());
            rr.UseResXDataNodes = true;
            IDictionaryEnumerator en = rr.GetEnumerator ();
            en.MoveNext ();

            ResXDataNode node = ((DictionaryEntry) en.Current).Value as ResXDataNode;
            
            Assert.IsNotNull (node, "#A1");

            object o = node.GetValue ((AssemblyName []) null);

            Assert.IsInstanceOfType (typeof (long), o, "#A2");
            Assert.AreEqual (34L, o, "#A3");

            rr.Close ();
        }

        [Test]
        public void ITRSPassedToResourceReaderDoesNotAffectResXDataNode_Serializable ()
        {
            
            serializable ser = new serializable ("aaaaa", "bbbbb");
            ResXDataNode dn = new ResXDataNode ("test", ser);
            
			string resXFile = GetResXFileWithNode (dn,"resx.resx");

            ResXResourceReader rr = new ResXResourceReader (resXFile, new AlwaysReturnSerializableSubClassTypeResolutionService ());
            rr.UseResXDataNodes = true;
            IDictionaryEnumerator en = rr.GetEnumerator ();
            en.MoveNext ();

            ResXDataNode node = ((DictionaryEntry) en.Current).Value as ResXDataNode;

            Assert.IsNotNull (node, "#A1");

            object o = node.GetValue ((AssemblyName []) null);

            Assert.IsNotInstanceOfType (typeof (serializableSubClass), o, "#A2");
            Assert.IsInstanceOfType (typeof (serializable), o, "#A3");
            rr.Close ();
        }

        [Test]
        public void ITRSPassedToResourceReaderAffectsDictionary_Serializable ()
        {
            
            serializable ser = new serializable ("aaaaa", "bbbbb");
            ResXDataNode dn = new ResXDataNode ("test", ser);
            
			string resXFile = GetResXFileWithNode (dn,"resx.resx");

            ResXResourceReader rr = new ResXResourceReader (resXFile, new AlwaysReturnSerializableSubClassTypeResolutionService ());
 			
            IDictionaryEnumerator en = rr.GetEnumerator ();
            en.MoveNext ();

            object o = ((DictionaryEntry) en.Current).Value;

            Assert.IsNotNull (o, "#A1");

            Assert.IsInstanceOfType (typeof (serializableSubClass), o,"#A2");

            rr.Close ();
        }

		[Test]
        public void ITRSPassedToResourceReaderAffectsDictionary_TypeConverter ()
        {
            
            ResXDataNode dn = new ResXDataNode ("test", 34L);
            
			string resXFile = GetResXFileWithNode (dn,"resx.resx");

            ResXResourceReader rr = new ResXResourceReader (resXFile, new AlwaysReturnIntTypeResolutionService ());
 
            IDictionaryEnumerator en = rr.GetEnumerator ();
            en.MoveNext ();

            object o = ((DictionaryEntry) en.Current).Value;

            Assert.IsNotNull (o, "#A1");

            Assert.IsInstanceOfType (typeof (int), o,"#A2");
            Assert.AreEqual (34, o,"#A3");

            rr.Close ();
        }

        [TearDown]
        protected override void TearDown ()
        {
            //teardown
            if (Directory.Exists (_tempDirectory))
                Directory.Delete (_tempDirectory, true);

            base.TearDown ();
        }

		string GetResXFileWithNode (ResXDataNode node, string filename)
        {
            string fullfileName;

            _tempDirectory = Path.Combine (Path.GetTempPath (), "ResXDataNodeTest");
            _otherTempDirectory = Path.Combine (_tempDirectory, "in");
            if (!Directory.Exists (_otherTempDirectory)) {
                Directory.CreateDirectory (_otherTempDirectory);
            }

            fullfileName = Path.Combine (_tempDirectory, filename);

            using (ResXResourceWriter writer = new ResXResourceWriter (fullfileName)) {
                writer.AddResource (node);
            }

            return fullfileName;
        }

        ResXDataNode GetNodeFileRefToIcon ()
        {
            _tempDirectory = Path.Combine (Path.GetTempPath (), "ResXDataNodeTest");
            _otherTempDirectory = Path.Combine (_tempDirectory, "in");
            if (!Directory.Exists (_otherTempDirectory))
            {
                Directory.CreateDirectory (_otherTempDirectory);
            }

            string refFile = Path.Combine (_tempDirectory, "32x32.ico");
            WriteEmbeddedResource ("32x32.ico", refFile);

            ResXFileRef fileRef = new ResXFileRef (refFile, typeof (Icon).AssemblyQualifiedName);
            ResXDataNode node = new ResXDataNode ("test", fileRef);

            return node;
        }

        ResXDataNode GetNodeEmdeddedIcon ()
        {

            Stream input = typeof (ResXDataNodeTest).Assembly.
                GetManifestResourceStream ("32x32.ico");

            Icon ico = new Icon (input);

            ResXDataNode node = new ResXDataNode ("test", ico);

            return node;
        }
		
		private static void WriteEmbeddedResource (string name, string filename)
		{
			const int size = 512;
			byte [] buffer = new byte [size];
			int count = 0;

			Stream input = typeof (ResXDataNodeTest).Assembly.
				GetManifestResourceStream (name);
			Stream output = File.Open (filename, FileMode.Create);

			try {
				while ((count = input.Read (buffer, 0, size)) > 0) {
					output.Write (buffer, 0, count);
				}
			} finally {
				output.Close ();
			}
		}

		private string GetFileFromString (string filename, string filecontents)
        {
            _tempDirectory = Path.Combine (Path.GetTempPath (), "ResXDataNodeTest");
            _otherTempDirectory = Path.Combine (_tempDirectory, "in");
            if (!Directory.Exists (_otherTempDirectory)) {
                Directory.CreateDirectory (_otherTempDirectory);
            }

            string filepath = Path.Combine (_tempDirectory, filename);
            
            StreamWriter writer = new StreamWriter(filepath,false);

            writer.Write (filecontents);
            writer.Close ();

            return filepath;
        }

        static string convertableResXWithoutAssemblyName =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <!-- 
    Microsoft ResX Schema 
    
    Version 2.0
    
    The primary goals of this format is to allow a simple XML format 
    that is mostly human readable. The generation and parsing of the 
    various data types are done through the TypeConverter classes 
    associated with the data types.
    
    Example:
    
    ... ado.net/XML headers & schema ...
    <resheader name=""resmimetype"">text/microsoft-resx</resheader>
    <resheader name=""version"">2.0</resheader>
    <resheader name=""reader"">System.Resources.ResXResourceReader, System.Windows.Forms, ...</resheader>
    <resheader name=""writer"">System.Resources.ResXResourceWriter, System.Windows.Forms, ...</resheader>
    <data name=""Name1""><value>this is my long string</value><comment>this is a comment</comment></data>
    <data name=""Color1"" type=""System.Drawing.Color, System.Drawing"">Blue</data>
    <data name=""Bitmap1"" mimetype=""application/x-microsoft.net.object.binary.base64"">
        <value>[base64 mime encoded serialized .NET Framework object]</value>
    </data>
    <data name=""Icon1"" type=""System.Drawing.Icon, System.Drawing"" mimetype=""application/x-microsoft.net.object.bytearray.base64"">
        <value>[base64 mime encoded string representing a byte array form of the .NET Framework object]</value>
        <comment>This is a comment</comment>
    </data>
                
    There are any number of ""resheader"" rows that contain simple 
    name/value pairs.
    
    Each data row contains a name, and value. The row also contains a 
    type or mimetype. Type corresponds to a .NET class that support 
    text/value conversion through the TypeConverter architecture. 
    Classes that don't support this are serialized and stored with the 
    mimetype set.
    
    The mimetype is used for serialized objects, and tells the 
    ResXResourceReader how to depersist the object. This is currently not 
    extensible. For a given mimetype the value must be set accordingly:
    
    Note - application/x-microsoft.net.object.binary.base64 is the format 
    that the ResXResourceWriter will generate, however the reader can 
    read any of the formats listed below.
    
    mimetype: application/x-microsoft.net.object.binary.base64
    value   : The object must be serialized with 
            : System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
            : and then encoded with base64 encoding.
    
    mimetype: application/x-microsoft.net.object.soap.base64
    value   : The object must be serialized with 
            : System.Runtime.Serialization.Formatters.Soap.SoapFormatter
            : and then encoded with base64 encoding.

    mimetype: application/x-microsoft.net.object.bytearray.base64
    value   : The object must be serialized into a byte array 
            : using a System.ComponentModel.TypeConverter
            : and then encoded with base64 encoding.
    -->
  <xsd:schema id=""root"" xmlns="""" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
    <xsd:import namespace=""http://www.w3.org/XML/1998/namespace"" />
    <xsd:element name=""root"" msdata:IsDataSet=""true"">
      <xsd:complexType>
        <xsd:choice maxOccurs=""unbounded"">
          <xsd:element name=""metadata"">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" />
              </xsd:sequence>
              <xsd:attribute name=""name"" use=""required"" type=""xsd:string"" />
              <xsd:attribute name=""type"" type=""xsd:string"" />
              <xsd:attribute name=""mimetype"" type=""xsd:string"" />
              <xsd:attribute ref=""xml:space"" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name=""assembly"">
            <xsd:complexType>
              <xsd:attribute name=""alias"" type=""xsd:string"" />
              <xsd:attribute name=""name"" type=""xsd:string"" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name=""data"">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
                <xsd:element name=""comment"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""2"" />
              </xsd:sequence>
              <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" msdata:Ordinal=""1"" />
              <xsd:attribute name=""type"" type=""xsd:string"" msdata:Ordinal=""3"" />
              <xsd:attribute name=""mimetype"" type=""xsd:string"" msdata:Ordinal=""4"" />
              <xsd:attribute ref=""xml:space"" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name=""resheader"">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
              </xsd:sequence>
              <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name=""resmimetype"">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name=""version"">
    <value>2.0</value>
  </resheader>
  <resheader name=""reader"">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name=""writer"">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  
  <data name=""test"" type=""DummyAssembly.Convertable"">
    <value>im a name	im a value</value>
  </data>
</root>";

	}
    
    public class DummyTypeResolutionService : ITypeResolutionService
    {

        public Assembly GetAssembly (AssemblyName name, bool throwOnError)
        {
            return null;
        }

        public Assembly GetAssembly (AssemblyName name)
        {
            return null;
        }

        public string GetPathOfAssembly (AssemblyName name)
        {
            return null;
        }

        public Type GetType (string name, bool throwOnError, bool ignoreCase)
        {
            return null;
        }

        public Type GetType (string name, bool throwOnError)
        {
            return null;
        }

        public Type GetType (string name)
        {
            return null;
        }

        public void ReferenceAssembly (AssemblyName name)
        {
            
        }
    }
    
	class notserializable
	{
		public object test;
		public notserializable ()
		{

		}
	}

	[SerializableAttribute]
	public class serializable : ISerializable
	{
		public string name;
		public string value;

        public serializable ()
        {
        }

		public serializable (string name, string value)
		{
			this.name = name;
			this.value = value;
		}

		public serializable (SerializationInfo info, StreamingContext ctxt)
		{
			name = (string) info.GetValue ("sername", typeof (string));
			value = (String) info.GetValue ("servalue", typeof (string));
		}

        public serializable (Stream stream)
        {
            BinaryFormatter bFormatter = new BinaryFormatter ();
            serializable deser = (serializable) bFormatter.Deserialize (stream);
            stream.Close ();

            name = deser.name;
            value = deser.value;
        }

		public void GetObjectData (SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue ("sername", name);
			info.AddValue ("servalue", value);
		}

		public override string ToString ()
		{
			return String.Format ("name={0};value={1}", this.name, this.value);
		}

		public override bool Equals (object obj)
		{
			serializable o = obj as serializable;
			if (o == null)
				return false;
			return this.name.Equals(o.name) && this.value.Equals(o.value);
		}
	}
}
#endif

