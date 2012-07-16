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

		[Test]
		public void GetValueAssemblyNameUsedWhereOnlyFullNameInResX_TypeConverter ()
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
		public void GetValueAssemblyNameRequiredEachTimeWhereOnlyFullNameInResX_TypeConverter ()
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
		public void CantLoadTypeFromThisAssemblyWithOnlyFullName_TypeConverter ()
		{
			ResXDataNode node = GetNodeFromResXReader (thisAssemblyConvertableResXWithoutAssemblyName);
			Assert.IsNotNull (node, "#A1");
			object obj = node.GetValue ((AssemblyName []) null);
		}

		[Test]
		public void CanLoadTypeFromThisAssemblyWithOnlyFullNamePassingAssemblyNames_TypeConverter ()
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
		public void GetValueTypeNameReturnsFullNameWereOnlyFullNameInResX_TypeConverter ()
		{
			// just a check, if this passes other tests will give false results
			ResXDataNode node = GetNodeFromResXReader (convertableResXWithoutAssemblyName);

			Assert.IsNotNull (node, "#A1");
			string returnedType = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual ("DummyAssembly.Convertable", returnedType, "#A2");
		}

		[Test]
		public void GetValueTypeNameAssemblyNameUsedWhereOnlyFullNameInResX_TypeConverter ()
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
		public void GetValueTypeNameAssemblyNameUsedEachTimeWhereOnlyFullNameInResX_TypeConverter ()
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

		[Test]
		public void AssemblyAutomaticallyLoaded_Serialized_GetValue ()
		{
			ResXDataNode node = GetNodeFromResXReader (anotherSerializableFromDummyAssembly);
			Assert.IsNotNull (node, "#A1");
			object value = node.GetValue ((AssemblyName[]) null);
			Assert.AreEqual ("DummyAssembly.AnotherSerializable, DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", value.GetType ().AssemblyQualifiedName, "#A2");
		}

		[Test]
		public void AssemblyAutomaticallyLoaded_Serialized_GetValueTypeName ()
		{
			ResXDataNode node = GetNodeFromResXReader (anotherSerializableFromDummyAssembly);
			Assert.IsNotNull (node, "#A1");
			string type = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual ("DummyAssembly.AnotherSerializable, DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", type, "#A2");
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void ErrorWhenAssemblyMissing_Serialized_GetValue ()
		{
			ResXDataNode node = GetNodeFromResXReader (missingSerializableFromMissingAssembly);
			Assert.IsNotNull (node, "#A1");
			object val = node.GetValue ((AssemblyName[]) null);
		}

		[Test]
		public void ReturnsObjectAssemblyMissing_Serialized_GetValueTypeName ()
		{
			ResXDataNode node = GetNodeFromResXReader (missingSerializableFromMissingAssembly);
			Assert.IsNotNull (node, "#A1");
			string type = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual (typeof (object).AssemblyQualifiedName, type, "#A2");
		}

		static string missingSerializableFromMissingAssembly =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
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
  <data name=""aname"" mimetype=""application/x-microsoft.net.object.binary.base64"">
    <value>
        AAEAAAD/////AQAAAAAAAAAMAgAAAEZNaXNzaW5nQXNzZW1ibHksIFZlcnNpb249MS4wLjAuMCwgQ3Vs
        dHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1udWxsBQEAAAAhRHVtbXlBc3NlbWJseS5NaXNzaW5n
        U2VyaWFsaXphYmxlAgAAAAdzZXJuYW1lCHNlcnZhbHVlAQECAAAABgMAAAAFYW5hbWUGBAAAAAZhdmFs
        dWUL
</value>
  </data>
</root>";

		static string anotherSerializableFromDummyAssembly =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
 
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
  <data name=""aname"" mimetype=""application/x-microsoft.net.object.binary.base64"">
    <value>
        AAEAAAD/////AQAAAAAAAAAMAgAAAEREdW1teUFzc2VtYmx5LCBWZXJzaW9uPTEuMC4wLjAsIEN1bHR1
        cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49bnVsbAUBAAAAIUR1bW15QXNzZW1ibHkuQW5vdGhlclNl
        cmlhbGl6YWJsZQIAAAAHc2VybmFtZQhzZXJ2YWx1ZQEBAgAAAAYDAAAABWFuYW1lBgQAAAAGYXZhbHVl
        Cw==
</value>
  </data>
</root>";

		static string convertableResXWithoutAssemblyName =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  
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