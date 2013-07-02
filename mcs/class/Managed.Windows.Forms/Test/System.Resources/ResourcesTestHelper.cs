//
// ResourcesTestHelper.cs : Base class for new resource tests with methods 
// required across many.
// 
// Author:
//	Gary Barnett (gary.barnett.mono@gmail.com)
// 
// Copyright (C) Gary Barnett (2012)
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

#if NET_2_0

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Collections;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;

namespace MonoTests.System.Resources {
	public class ResourcesTestHelper {
		string tempFileWithIcon = null;
		string tempFileWithSerializable = null;

		[SetUp]
		protected virtual void SetUp ()
		{

		}

		protected ResXDataNode GetNodeFromResXReader (ResXDataNode node)
		{
			StringWriter sw = new StringWriter ();
			using (ResXResourceWriter writer = new ResXResourceWriter (sw)) {
				writer.AddResource (node);
			}

			StringReader sr = new StringReader (sw.GetStringBuilder ().ToString ());

			using (ResXResourceReader reader = new ResXResourceReader (sr)) {
				reader.UseResXDataNodes = true;
				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();

				return ((DictionaryEntry) enumerator.Current).Value as ResXDataNode;
			}
		}

		protected ResXDataNode GetNodeFromResXReader (string contents)
		{
			StringReader sr = new StringReader (contents);

			using (ResXResourceReader reader = new ResXResourceReader (sr)) {
				reader.UseResXDataNodes = true;
				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();

				return (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;
			}
		}

		public ResXDataNode GetNodeEmdeddedIcon ()
		{
			Stream input = typeof (ResXDataNodeTest).Assembly.
				GetManifestResourceStream ("32x32.ico");

			Icon ico = new Icon (input);
			ResXDataNode node = new ResXDataNode ("test", ico);
			return node;
		}

		public ResXDataNode GetNodeFileRefToIcon ()
		{
			tempFileWithIcon = Path.GetTempFileName (); // remember to delete file in teardown
			Path.ChangeExtension (tempFileWithIcon, "ico");

			WriteEmbeddedResource ("32x32.ico", tempFileWithIcon);
			ResXFileRef fileRef = new ResXFileRef (tempFileWithIcon, typeof (Icon).AssemblyQualifiedName);
			ResXDataNode node = new ResXDataNode ("test", fileRef);

			return node;
		}

		void WriteEmbeddedResource (string name, string filename)
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

		public ResXDataNode GetNodeEmdeddedBytes1To10 ()
		{
			byte [] someBytes = new byte [] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			ResXDataNode node = new ResXDataNode ("test", someBytes);
			return node;
		}

		public ResXDataNode GetNodeEmdeddedSerializable ()
		{
			serializable ser = new serializable ("testName", "testValue");
			ResXDataNode node = new ResXDataNode ("test", ser);
			return node;
		}

		public ResXDataNode GetNodeFileRefToSerializable (string filename, bool assemblyQualifiedName)
		{
			tempFileWithSerializable = Path.GetTempFileName ();  // remember to delete file in teardown
			serializable ser = new serializable ("name", "value");
			
			SerializeToFile (tempFileWithSerializable, ser);

			string typeName;

			if (assemblyQualifiedName)
				typeName = typeof (serializable).AssemblyQualifiedName;
			else
				typeName = typeof (serializable).FullName;

			ResXFileRef fileRef = new ResXFileRef (tempFileWithSerializable, typeName);
			ResXDataNode node = new ResXDataNode ("test", fileRef);

			return node;
		}

		static void SerializeToFile (string filepath, serializable ser)
		{
			Stream stream = File.Open (filepath, FileMode.Create);
			BinaryFormatter bFormatter = new BinaryFormatter ();
			bFormatter.Serialize (stream, ser);
			stream.Close ();
		}

		[TearDown]
		protected virtual void TearDown ()
		{
			if (tempFileWithIcon != null) {
				File.Delete (tempFileWithIcon);
				tempFileWithIcon = null;
			}

			if (tempFileWithSerializable != null) {
				File.Delete (tempFileWithSerializable);
				tempFileWithSerializable = null;
			}
		}
	}
}
#endif

