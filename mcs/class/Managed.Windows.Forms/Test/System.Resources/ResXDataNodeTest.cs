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
//		Andreia Gaita	(avidigal@novell.com)
//  		Gary Barnett	(gary.barnett.mono@gmail.com)


#if NET_2_0
using System;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Resources;
using System.Runtime.Serialization;
using System.Collections;
using NUnit.Framework;
using System.ComponentModel.Design;
using System.Runtime.Serialization.Formatters.Binary;

namespace MonoTests.System.Resources {
	[TestFixture]
	public class ResXDataNodeTest : ResourcesTestHelper
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
		public void ConstructorEx2A ()
		{
			ResXDataNode d = new ResXDataNode (null, new ResXFileRef ("filename", "typename"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorEx2B ()
		{
			ResXDataNode d = new ResXDataNode ("aname", (ResXFileRef) null);
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
		public void Name ()
		{
			ResXDataNode node = new ResXDataNode ("startname", (object) null);
			Assert.AreEqual ("startname", node.Name, "#A1");
			node.Name = "newname";
			Assert.AreEqual ("newname", node.Name, "#A2");
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void NameCantBeNull ()
		{
			ResXDataNode node = new ResXDataNode ("startname", (object) null);
			node.Name = null;
		}
	
		[Test, ExpectedException (typeof (ArgumentException))]
		public void NameCantBeEmpty ()
		{
			ResXDataNode node = new ResXDataNode ("name", (object) null);
			node.Name = "";
		}

		[Test]
		public void FileRef ()
		{
			ResXFileRef fileRef = new ResXFileRef ("fileName", "Type.Name");
			ResXDataNode node = new ResXDataNode ("name", fileRef);
			Assert.AreEqual (fileRef, node.FileRef, "#A1");
		}

		[Test]
		public void Comment ()
		{
			ResXDataNode node = new ResXDataNode ("name", (object) null);
			node.Comment = "acomment";
			Assert.AreEqual ("acomment", node.Comment, "#A1");
		}

		[Test]
		public void CommentNullToStringEmpty ()
		{
			ResXDataNode node = new ResXDataNode ("name", (object) null);
			node.Comment = null;
			Assert.AreEqual (String.Empty, node.Comment, "#A1");
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
				serializable o = node.GetValue ((AssemblyName []) null) as serializable;
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
			Assert.IsNotNull (node.FileRef, "#A1");
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, node.FileRef.TypeName, "#A2");
			Assert.AreEqual ("test", node.Name, "#A3");
		}

		[Test]
		public void NullObjectGetValueTypeNameIsNull ()
		{
			ResXDataNode node = new ResXDataNode ("aname", (object) null);
			Assert.IsNull (node.GetValueTypeName ((AssemblyName []) null), "#A1");
		}

		[Test]
		public void NullObjectWrittenToResXOK ()
		{
			ResXDataNode node = new ResXDataNode ("aname", (object) null);
			ResXDataNode returnedNode = GetNodeFromResXReader (node);
			Assert.IsNotNull (returnedNode, "#A1");
			Assert.IsNull (returnedNode.GetValue ((AssemblyName []) null), "#A2");
		}

		[Test]
		public void NullObjectReturnedFromResXGetValueTypeNameReturnsObject ()
		{
			ResXDataNode node = new ResXDataNode ("aname", (object) null);
			ResXDataNode returnedNode = GetNodeFromResXReader (node);
			Assert.IsNotNull (returnedNode, "#A1");
			Assert.IsNull (returnedNode.GetValue ((AssemblyName []) null), "#A2");
			string type = returnedNode.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual (typeof (object).AssemblyQualifiedName, type, "#A3");
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
		public void ITRSPassedToResourceReaderDoesNotAffectResXDataNode_TypeConverter ()
		{
			ResXDataNode dn = new ResXDataNode ("test", 34L);
			
			string resXFile = GetResXFileWithNode (dn,"resx.resx");

			ResXResourceReader rr = new ResXResourceReader (resXFile, new ReturnIntITRS ());
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

			ResXResourceReader rr = new ResXResourceReader (resXFile, new ReturnSerializableSubClassITRS ());
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
		public void BasePathSetOnResXResourceReaderDoesAffectResXDataNode ()
		{
			ResXFileRef fileRef = new ResXFileRef ("file.name", "type.name");
			ResXDataNode node = new ResXDataNode("anode", fileRef);
			string resXFile = GetResXFileWithNode (node, "afilename.xxx");

			using (ResXResourceReader rr = new ResXResourceReader (resXFile)) {
				rr.BasePath = "basePath";
				rr.UseResXDataNodes = true;
				IDictionaryEnumerator en = rr.GetEnumerator ();
				en.MoveNext ();

				ResXDataNode returnedNode = ((DictionaryEntry) en.Current).Value as ResXDataNode;

				Assert.IsNotNull (node, "#A1");
				Assert.AreEqual (Path.Combine ("basePath", "file.name"), returnedNode.FileRef.FileName, "#A2");
			}
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

