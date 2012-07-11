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
	public class ResXDataNodeTypeConverterGetValueTypeNameTests : MonoTests.System.Windows.Forms.TestHelper {
		string _tempDirectory;
		string _otherTempDirectory;
		
		[Test]
		public void ITRSUsedWithNodeFromReader ()
		{
			// for node returned from ResXResourceReader for an object stored by means of a typeconverter, 
			// check supplying ITRS changes output of method

			ResXDataNode returnedNode, originalNode;

			originalNode = new ResXDataNode ("aNumber", 23L);

			string fileName = GetResXFileWithNode (originalNode, "long.resx");

			// should load assembly referenced in file
			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");

				string returnedType = returnedNode.GetValueTypeName (new AlwaysReturnIntTypeResolutionService ());

				Assert.AreEqual ((typeof (Int32)).AssemblyQualifiedName, returnedType, "#A2");
			}
		}

		[Test]
		public void ITRSUsedEachTimeWhenNodeFromReader ()
		{
			// for node returned from ResXResourceReader for an object stored by means of a typeconverter, 
			// check supplied ITRS changes output each time

			ResXDataNode returnedNode, originalNode;

			originalNode = new ResXDataNode ("aNumber", 23L);

			string fileName = GetResXFileWithNode (originalNode, "long.resx");

			// should load assembly referenced in file
			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");

				string newType = returnedNode.GetValueTypeName (new AlwaysReturnIntTypeResolutionService ());

				Assert.AreEqual (typeof (int).AssemblyQualifiedName, newType, "#A2");

				string origType = returnedNode.GetValueTypeName ((ITypeResolutionService) null);

				Assert.AreEqual (typeof (long).AssemblyQualifiedName, origType, "#A3");				
			}
		}

		[Test]
		public void ITRSNotUsedWhenNodeCreatedNew ()
		{
			// check supplying params to GetValueTypeName of the UseResXDataNode does not change the output
			// of the method for an instance created manually

			ResXDataNode node;
			node = new ResXDataNode ("along", 34L);
			string returnedType = node.GetValueTypeName (new AlwaysReturnIntTypeResolutionService ());
			Assert.AreEqual ((typeof (long)).AssemblyQualifiedName, returnedType, "#A1");
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

	}


}
#endif

