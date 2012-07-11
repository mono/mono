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
	public class ResXDataNodeByteArrayTests : MonoTests.System.Windows.Forms.TestHelper {
		string _tempDirectory;
		string _otherTempDirectory;
		
		[Test]
		public void GetValueITRSNotUsedWhenNodeReturnedFromReader ()
		{
				
			ResXDataNode originalNode, returnedNode;

			originalNode = GetNodeEmdeddedBytes ();

			string fileName = GetResXFileWithNode (originalNode, "test.resx");

			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");
				
				object val = returnedNode.GetValue (new AlwaysReturnIntTypeResolutionService ());
				Assert.IsInstanceOfType (typeof (byte[]), val, "#A2");
			}
		}

		[Test, ExpectedException (typeof (NotImplementedException))]
		public void GetValueITRSIsTouchedWhenNodeReturnedFromReader ()
		{

			ResXDataNode originalNode, returnedNode;

			originalNode = GetNodeEmdeddedBytes ();

			string fileName = GetResXFileWithNode (originalNode, "test.resx");

			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");
				//would raise error if touched
				object val = returnedNode.GetValue (new ExceptionalTypeResolutionService ());
				
			}
		}

		[Test]
		public void GetValueITRSNotTouchedWhenNodeCreatedNew ()
		{
			// check supplied params to GetValue are not touched
			// for an instance created manually

			ResXDataNode node;

			node = GetNodeEmdeddedBytes ();

			//would raise exception if param used
			Object obj = node.GetValue (new ExceptionalTypeResolutionService ());
			Assert.IsInstanceOfType (typeof (byte[]), obj, "#A1");
		}

		[Test]
		public void GetValueTypeNameITRSIsUsedWithNodeFromReader ()
		{

			ResXDataNode originalNode, returnedNode;

			originalNode = GetNodeEmdeddedBytes ();

			string fileName = GetResXFileWithNode (originalNode, "test.resx");

			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");

				string returnedType = returnedNode.GetValueTypeName (new AlwaysReturnIntTypeResolutionService ());

				Assert.AreEqual ((typeof (int)).AssemblyQualifiedName, returnedType, "#A2");
			}
		}

		[Test]
		public void GetValueTypeNameITRSIsUsedAfterGetValueCalledWithNodeFromReader ()
		{

			ResXDataNode originalNode, returnedNode;

			originalNode = GetNodeEmdeddedBytes ();

			string fileName = GetResXFileWithNode (originalNode, "test.resx");

			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");

				object obj = returnedNode.GetValue ((ITypeResolutionService) null);

				string returnedType = returnedNode.GetValueTypeName (new AlwaysReturnIntTypeResolutionService ());

				Assert.AreEqual ((typeof (int)).AssemblyQualifiedName, returnedType, "#A2");
			}
		}

		[Test]
		public void GetValueTypeNameITRSNotUsedWhenNodeCreatedNew ()
		{
			// check supplying params to GetValueType of the ResXDataNode does not change the output
			// of the method for an instance created manually

			ResXDataNode node;
			node = GetNodeEmdeddedBytes ();
			string returnedType = node.GetValueTypeName (new AlwaysReturnIntTypeResolutionService ());
			Assert.AreEqual ((typeof (byte[])).AssemblyQualifiedName, returnedType, "#A1");
		}

		[Test]
		public void ChangesToReturnedByteArrayNotLaterWrittenBack ()
		{

			ResXDataNode originalNode = GetNodeEmdeddedBytes ();

			string fileName = GetResXFileWithNode (originalNode, "test.resx");

			string newFileName;

			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				ResXDataNode returnedNode;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");

				object val = returnedNode.GetValue ((ITypeResolutionService) null);
				Assert.IsInstanceOfType (typeof (byte []), val, "#A2");

				byte[] newBytes = (byte[]) val;

				Assert.AreEqual (1, newBytes [0], "A3");

				newBytes [0] = 99;

				newFileName = GetResXFileWithNode (returnedNode,"another.resx");
			}

			using (ResXResourceReader reader = new ResXResourceReader (newFileName)) {
				reader.UseResXDataNodes = true;

				ResXDataNode returnedNode;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A4");

				object val = returnedNode.GetValue ((ITypeResolutionService) null);
				Assert.IsInstanceOfType (typeof (byte []), val, "#A5");

				byte [] newBytes = (byte []) val;
				// would be 99 if written back
				Assert.AreEqual (1,newBytes [0],"A6");
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

		ResXDataNode GetNodeEmdeddedBytes ()
		{
			byte[] someBytes = new byte[] {1,2,3,4,5,6,7,8,9,10};
			ResXDataNode node = new ResXDataNode ("test", someBytes);
			return node;
		}

	}

	
}
#endif