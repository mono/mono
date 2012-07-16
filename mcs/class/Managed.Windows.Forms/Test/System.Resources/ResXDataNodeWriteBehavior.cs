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
using System.Collections;
using NUnit.Framework;
using System.Text;
using System.ComponentModel.Design;

namespace MonoTests.System.Resources {
	[TestFixture]
	public class ResXDataNodeWriteBehavior : ResourcesTestHelper {
		
		[Test]
		public void TypeConverterObjectNotLoaded ()
		{
			string aName = "System.Windows.Forms_test_net_2_0, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

			ResXDataNode node = GetNodeFromResXReader (convertableResXWithoutAssemblyName);
			Assert.IsNotNull (node, "#A1");
			// would cause error if object loaded
			GetNodeFromResXReader (node);
		}

		[Test]
		public void SerializedObjectNotLoaded ()
		{
			ResXDataNode node = GetNodeFromResXReader (serializedResXCorruped);
			Assert.IsNotNull (node, "#A1");
			// would cause error if object loaded
			GetNodeFromResXReader (node);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void FileRefIsLoaded ()
		{
			// .NET does instantiate the encoding until the write, as exception not thrown until write
			ResXDataNode node = GetNodeFromResXReader (fileRefResXCorrupted);
			Assert.IsNotNull (node, "#A1");
			// would cause error if object loaded
			GetNodeFromResXReader (node);
		}

		[Test]
		public void ResXNullRef_WriteBack ()
		{
			ResXDataNode node = new ResXDataNode ("NullRef", (object) null);
			ResXDataNode returnedNode = GetNodeFromResXReader (node);
			Assert.AreEqual (null, returnedNode.GetValue ((AssemblyName[]) null), "#A1");
			ResXDataNode finalNode = GetNodeFromResXReader (returnedNode);
			Assert.AreEqual (null, finalNode.GetValue ((AssemblyName []) null), "#A2");
		}

		[Test]
		public void InvalidMimeType_WriteBack ()
		{
			//FIXME: should check the ResX output to ensure mime type / value info still there
			ResXDataNode node = GetNodeFromResXReader (serializedResXInvalidMimeType);
			ResXDataNode returnedNode = GetNodeFromResXReader (node);

			object obj = returnedNode.GetValue ((AssemblyName[]) null);
			Assert.IsNull (obj, "#A1");
		}

		[Test]
		public void BinTypeConverter_WriteBack ()
		{
			MyBinType mb = new MyBinType ("contents");
			ResXDataNode node = new ResXDataNode ("aname", mb);
			ResXDataNode returnedNode = GetNodeFromResXReader (node);
			Assert.IsNotNull (returnedNode, "#A1");
			ResXDataNode finalNode = GetNodeFromResXReader (returnedNode);
			MyBinType returnedMb = (MyBinType) finalNode.GetValue ((AssemblyName []) null);
			Assert.AreEqual ("contents", returnedMb.Value, "#A2");
		}

		[Test]
		public void FileRefWithEncoding_WriteBack ()
		{
			ResXFileRef fileRef = new ResXFileRef ("afilename", "A.Type.Name", Encoding.UTF7);
			ResXDataNode node = GetNodeFromResXReader (new ResXDataNode ("aname", fileRef));
			Assert.IsNotNull (node, "#A1");
			ResXDataNode returnedNode = GetNodeFromResXReader (node);
			Assert.IsNotNull (returnedNode, "#A2");
			Assert.IsInstanceOfType (Encoding.UTF7.GetType (), returnedNode.FileRef.TextFileEncoding);
		}

		[Test]
		public void ByteArray_WriteBack ()
		{
			byte [] testBytes = new byte [] { 1,2,3,4,5,6,7,8,9,10 };
			ResXDataNode node = new ResXDataNode ("aname", testBytes);
			ResXDataNode returnedNode = GetNodeFromResXReader (node);
			Assert.IsNotNull (returnedNode, "#A1");
			Assert.AreEqual (testBytes, returnedNode.GetValue ((AssemblyName []) null), "#A2");
			ResXDataNode finalNode = GetNodeFromResXReader (returnedNode);
			Assert.IsNotNull (finalNode,"#A3");
			Assert.AreEqual (testBytes, finalNode.GetValue ((AssemblyName []) null), "#A4");
		}

		[Test]
		public void BasePathSetOnResXReaderAffectsFileRef_WriteBack ()
		{
			ResXDataNode returnedNode;
			StringWriter sw = new StringWriter ();
			sw.Write (fileRefResX);

			StringReader sr = new StringReader (sw.GetStringBuilder ().ToString ());

			using (ResXResourceReader reader = new ResXResourceReader (sr)) {
				reader.UseResXDataNodes = true;
				reader.BasePath = "c:\\";
				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();

				ResXDataNode node = ((DictionaryEntry) enumerator.Current).Value as ResXDataNode;
				Assert.IsNotNull (node, "#A1");
				Assert.AreEqual ("c:\\file.name", node.FileRef.FileName, "#A2");
				returnedNode = GetNodeFromResXReader (node);
			}

			Assert.AreEqual ("c:\\file.name", returnedNode.FileRef.FileName, "#A3");
		}

		[Test]
		public void SerializedChangesToReturnedObjectNotLaterWrittenBack ()
		{
			ResXDataNode originalNode, returnedNode, finalNode;

			originalNode = GetNodeEmdeddedSerializable ();
			returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			object val = returnedNode.GetValue ((ITypeResolutionService) null);
			Assert.IsInstanceOfType (typeof (serializable), val, "#A2");

			serializable ser = (serializable) val;

			Assert.AreEqual ("testName", ser.name, "A3");
			ser.name = "changed";
			finalNode = GetNodeFromResXReader (returnedNode);

			Assert.IsNotNull (finalNode, "#A4");
			object finalVal = finalNode.GetValue ((ITypeResolutionService) null);
			Assert.IsInstanceOfType (typeof (serializable), finalVal, "#A5");

			serializable finalSer = (serializable) finalVal;
			// would be "changed" if written back
			Assert.AreEqual ("testName", finalSer.name, "A6");
		}


		static string fileRefResX =
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
  <assembly alias=""System.Windows.Forms"" name=""System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"" />
  <data name=""anode"" type=""System.Resources.ResXFileRef, System.Windows.Forms"">
    <value>file.name;type.name</value>
  </data>
</root>";
		
		static string fileRefResXCorrupted = 
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
  <assembly alias=""System.Windows.Forms"" name=""System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"" />
  <data name=""test"" type=""System.Resources.ResXFileRef, System.Windows.Forms"">
	<value>.\somethingthatdoesntexist.txt;System.String, System.Windows.Forms_test_net_2_0, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;AValidCultureStringThisIsNot</value>
  </data>
</root>";

		static string serializedResXCorruped =
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
  <data name=""test"" mimetype=""application/x-microsoft.net.object.binary.base64"">
	<value>
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
</value>
  </data>
</root>";

		static string serializedResXInvalidMimeType =
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
  <data name=""test"" mimetype=""application/xxxx"">
	<value>
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
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

	}
}

#endif