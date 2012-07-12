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
// Copyright (c) 2012 Gary Barnett
//
// Authors:
//	Gary Barnett

#if NET_2_0
using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using System.Collections;
using NUnit.Framework;
using System.ComponentModel;
using System.Globalization;

namespace MonoTests.System.Resources {
	[TestFixture]
    public class ResXDataNodeAssemblyNameTests : ResourcesTestHelper {
		/*
		[Test]
		public void CanPassAssemblyNameToGetValueToReturnSpecificVersionOfObjectClassInstance ()
		{
			// tries to force use of 2.0 assembly but DOESNT WORK

			ResXDataNode originalNode, returnedNode;
			originalNode = GetNodeEmdeddedIcon ();
			string fileName = GetResXFileWithNode (originalNode, "test.resx");

			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");

				string aName = "System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
				AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

				object val = returnedNode.GetValue (assemblyNames);

				//typeof(Icon).AssemblyQualifiedName

				Assert.AreEqual ("System.Drawing.Icon, " + aName, val.GetType ().AssemblyQualifiedName, "#A2");

				//Assert.IsInstanceOfType (typeof (serializableSubClass), val, "#A2");
			}
		}

		[Test]
		public void CanPassAssemblyNameToGetValueTypeNameToReturnSpecificVersionOfObject ()
		{
			// tries to force use of 2.0 assembly DOESNT WORK - returns 4.0.0.0

			ResXDataNode originalNode, returnedNode;
			originalNode = GetNodeEmdeddedIcon ();
			string fileName = GetResXFileWithNode (originalNode, "test.resx");

			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");

				string aName = "System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
				AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

				string returnedName = returnedNode.GetValueTypeName (assemblyNames);

				//typeof(Icon).AssemblyQualifiedName

				Assert.AreEqual ("System.Drawing.Icon, " + aName, returnedName, "#A2");

				//Assert.IsInstanceOfType (typeof (serializableSubClass), val, "#A2");
			}
		}
		
		[Test]
		public void GetValueParamIsUsedWhenFileRefCreatedNewTRYOUT ()
		{
			// doesnt work

			ResXDataNode node;

			node = GetNodeFileRefToSerializableWithoutQualifiedTypeName ("ser.bbb");
			
			string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

			// string aName = typeof (serializable).AssemblyQualifiedName;
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

			object val = node.GetValue ((AssemblyName[]) null);
			
			//object val = node.GetValue ((AssemblyName[]) null);
		}
		
		*/

		[Test, ExpectedException (typeof (TypeLoadException))]
		public void GetValueAssemblyNameUsedWereOnlyFullNameInResXForEmbedded_TestValidityCheck ()
		{
			// just a check, if this passes other tests will give false results
		    ResXDataNode node = GetNodeFromResXReader (convertableResXWithoutAssemblyName);

            Assert.IsNotNull (node, "#A1");
		    object obj = node.GetValue ((AssemblyName []) null);
		}

		[Test]
		public void GetValueAssemblyNameUsedWhereOnlyFullNameInResXForEmbedded ()
		{
			// DummyAssembly must be in the same directory as current assembly to work correctly
			string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

            ResXDataNode node = GetNodeFromResXReader (convertableResXWithoutAssemblyName);

            Assert.IsNotNull (node, "#A1");
			object obj = node.GetValue (assemblyNames);
			Assert.AreEqual ("DummyAssembly.Convertable, " + aName, obj.GetType ().AssemblyQualifiedName);
		}

		[Test, ExpectedException (typeof (TypeLoadException))]
		public void GetValueAssemblyNameRequiredEachTimeWhereOnlyFullNameInResXForEmbedded ()
		{
			// DummyAssembly must be in the same directory as current assembly to work correctly
			string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

            ResXDataNode node = GetNodeFromResXReader (convertableResXWithoutAssemblyName);

            Assert.IsNotNull (node, "#A1");
			object obj = node.GetValue (assemblyNames);
			Assert.AreEqual ("DummyAssembly.Convertable, " + aName, obj.GetType ().AssemblyQualifiedName, "#A2");
			object obj2 = node.GetValue ((AssemblyName []) null); //should cause exception here
			
		}
		//FIXME: does the way this test is run by NUnit affect the validity of the results showing that you need assembly name to pull type from current assembly?
		[Test, ExpectedException (typeof (TypeLoadException))]
		public void CantLoadTypeFromThisAssemblyWithOnlyFullName ()
		{
            ResXDataNode node = GetNodeFromResXReader (thisAssemblyConvertableResXWithoutAssemblyName);
            Assert.IsNotNull (node, "#A1");
			object obj = node.GetValue ((AssemblyName []) null);
		}

		[Test]
		public void CanLoadTypeFromThisAssemblyWithOnlyFullNamePassingAssemblyNames ()
		{
			string aName = "System.Windows.Forms_test_net_2_0, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

            ResXDataNode node = GetNodeFromResXReader (thisAssemblyConvertableResXWithoutAssemblyName);

            Assert.IsNotNull (node, "#A1");
			// would cause exception if couldnt find type
			object obj = node.GetValue (assemblyNames);
			Assert.IsInstanceOfType (typeof (ThisAssemblyConvertable), obj, "#A2");
		}

		[Test]
		public void GetValueTypeNameReturnsFullNameWereOnlyFullNameInResXForEmbedded ()
		{
			// just a check, if this passes other tests will give false results
            ResXDataNode node = GetNodeFromResXReader (convertableResXWithoutAssemblyName);

            Assert.IsNotNull (node, "#A1");
			string returnedType = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual ("DummyAssembly.Convertable", returnedType, "#A2");
		}

		[Test]
		public void GetValueTypeNameAssemblyNameUsedWhereOnlyFullNameInResXForEmbedded ()
		{
			// DummyAssembly must be in the same directory as current assembly to work correctly
			string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

            ResXDataNode node = GetNodeFromResXReader (convertableResXWithoutAssemblyName);

            Assert.IsNotNull (node, "#A1");
			string returnedType = node.GetValueTypeName (assemblyNames);
			Assert.AreEqual ("DummyAssembly.Convertable, " + aName, returnedType, "#A2");
		}

		[Test]
		public void GetValueTypeNameAssemblyNameUsedEachTimeWhereOnlyFullNameInResXForEmbedded ()
		{
			// DummyAssembly must be in the same directory as current assembly to work correctly
			string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

            ResXDataNode node = GetNodeFromResXReader (convertableResXWithoutAssemblyName);

            Assert.IsNotNull (node, "#A1");
			string returnedName = node.GetValueTypeName (assemblyNames);
			Assert.AreEqual ("DummyAssembly.Convertable, " + aName, returnedName, "#A2");
			string nameWithNullParam = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual ("DummyAssembly.Convertable", nameWithNullParam, "#A3");
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

		static string thisAssemblyConvertableResXWithoutAssemblyName =
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
  
  <data name=""test"" type=""MonoTests.System.Resources.ThisAssemblyConvertable"">
	<value>im a name	im a value</value>
  </data>
</root>";
        
	}

}
#endif