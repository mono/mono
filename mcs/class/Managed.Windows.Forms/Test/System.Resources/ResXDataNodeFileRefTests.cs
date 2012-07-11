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

namespace MonoTests.System.Resources {
	[TestFixture]
	public class ResXDataNodeFileRefTests : MonoTests.System.Windows.Forms.TestHelper {
		string _tempDirectory;
		string _otherTempDirectory;

		[Test, ExpectedException (typeof (NotImplementedException))]
		public void GetValueITRSTouchedWhenNodeFromReader ()
		{
			// for a node returned from ResXResourceReader with FileRef, 
			// check params supplied to GetValue method of ResXDataNode are touched

			ResXDataNode originalNode, returnedNode;

			originalNode = GetNodeFileRefToSerializable ("ser.bbb",true);

			string fileName = GetResXFileWithNode (originalNode);

			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");
				// raises error if touched
				Icon ico = (Icon) returnedNode.GetValue (new ExceptionalTypeResolutionService ());

			}
		}

		[Test]
		public void GetValueITRSNotUsedWhenNodeFromReader ()
		{
			// for a node returend from reader with a FileRef, 
			// check ITRS supplied to GetValue method not actually used

			ResXDataNode originalNode, returnedNode;

			originalNode = GetNodeFileRefToSerializable ("ser.bbb",true);

			string fileName = GetResXFileWithNode (originalNode);

			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");

				object val = returnedNode.GetValue (new AlwaysReturnSerializableSubClassTypeResolutionService ());

				Assert.IsNotInstanceOfType (typeof (serializableSubClass), val, "#A2");
				Assert.IsInstanceOfType (typeof (serializable), val, "#A3");
			}
		}

		[Test, ExpectedException(typeof (TypeLoadException))]
		public void CantGetValueWithOnlyFullNameAsType ()
		{
			ResXDataNode originalNode, returnedNode;

			originalNode = GetNodeFileRefToSerializable ("ser.bbb", false);

			string fileName = GetResXFileWithNode (originalNode);

			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");

				object obj = returnedNode.GetValue ((AssemblyName[]) null);

			}
		}

		[Test, ExpectedException (typeof (TypeLoadException))]
		public void CantGetValueWithOnlyFullNameAsTypeByProvidingAssemblyName ()
		{
			ResXDataNode originalNode, returnedNode;

			string aName = "System.Windows.Forms_test_net_2_0, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

			originalNode = GetNodeFileRefToSerializable ("ser.bbb", false);

			string fileName = GetResXFileWithNode (originalNode);

			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");

				object obj = returnedNode.GetValue (assemblyNames);
			}
		}

		[Test]
		public void CanGetStrongNameFromGetValueTypeNameWithOnlyFullNameAsTypeByProvidingAssemblyName ()
		{
			ResXDataNode originalNode, returnedNode;

			string aName = "System.Windows.Forms_test_net_2_0, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

			originalNode = GetNodeFileRefToSerializable ("ser.bbb", false);

			string fileName = GetResXFileWithNode (originalNode);

			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");

				string typeName = returnedNode.GetValueTypeName (assemblyNames);

				Assert.AreEqual ("MonoTests.System.Resources.serializable, " + aName, typeName, "#A2");
			}
		}

		public void CanGetValueTypeNameWithOnlyFullNameAsType ()
		{
			ResXDataNode originalNode, returnedNode;

			originalNode = GetNodeFileRefToSerializable ("ser.bbb", false);

			string fileName = GetResXFileWithNode (originalNode);

			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");

				string typeName = returnedNode.GetValueTypeName ((AssemblyName []) null);

				Assert.AreEqual ((typeof (serializable)).FullName, typeName, "#A2");
			}
		}

		[Test]
		public void GetValueTypeNameITRSUsedWhenNodeFromReader ()
		{
			// for a node returned from ResXResourceReader, check supplying params to 
			// GetValueTypeName changes the output of the method

			ResXDataNode originalNode, returnedNode;

			originalNode = GetNodeFileRefToSerializable ("ser.bbb",true);

			string fileName = GetResXFileWithNode (originalNode);

			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");

				string returnedType = returnedNode.GetValueTypeName (new AlwaysReturnSerializableSubClassTypeResolutionService ());

				Assert.AreEqual ((typeof (serializableSubClass)).AssemblyQualifiedName, returnedType, "#A2");
			}
		}

		[Test]
		public void GetValueTypeNameITRSUsedWhenNodeCreatedNew ()
		{
			// check supplying params GetValueTypeName of the 
			// UseResXDataNode does the output of the method for an instance
			// initialised manually

			ResXDataNode node;

			node = GetNodeFileRefToSerializable ("ser.bbb",true);

			string returnedType = node.GetValueTypeName (new AlwaysReturnSerializableSubClassTypeResolutionService ());

			Assert.AreEqual ((typeof (serializableSubClass)).AssemblyQualifiedName, returnedType, "#A1");
		}

		[Test, ExpectedException (typeof (NotImplementedException))]
		public void GetValueITRSTouchedWhenNodeCreatedNew ()
		{
			// check supplyied params to GetValue of the ResXDataNode are touched for
			// an instance initialised by me

			ResXDataNode node;

			node = GetNodeFileRefToSerializable ("ser.bbb",true);

			//raises exception if param touched
			Object obj = node.GetValue (new ExceptionalTypeResolutionService ());
		}

		[Test]
		public void GetValueITRSNotUsedWhenNodeCreatedNew ()
		{
			// check supplyied params to GetValue of the ResXDataNode are not used for
			// an instance initialised by me

			ResXDataNode node;

			node = GetNodeFileRefToSerializable ("ser.bbb",true);

			object val = node.GetValue (new AlwaysReturnSerializableSubClassTypeResolutionService ());

			Assert.IsNotInstanceOfType (typeof (serializableSubClass), val, "#A1");
			Assert.IsInstanceOfType (typeof (serializable), val, "#A2");
		}

		#region Initial Exploratory Tests

		[Test]
		public void GetValueResXFileRefNullAssemblyNames ()
		{
			ResXDataNode node = GetNodeFileRefToIcon ();

			Object ico = node.GetValue ((AssemblyName []) null);
			Assert.IsNotNull (ico, "#A1");
			Assert.IsInstanceOfType (typeof (Icon), ico, "#A2");
		}

		[Test]
		public void GetValueResXFileRefNullITypeResolutionService ()
		{
			ResXDataNode node = GetNodeFileRefToIcon ();

			Object ico = node.GetValue ((ITypeResolutionService) null);
			Assert.IsNotNull (ico, "#A1");
			Assert.IsInstanceOfType (typeof (Icon), ico, "#A2");
		}

		[Test]
		public void GetValueTypeNameResXFileRefNullAssemblyNames ()
		{
			ResXDataNode node = GetNodeFileRefToIcon ();

			string name = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}

		[Test]
		public void GetValueTypeNameResXFileRefNullITypeResolutionService ()
		{

			ResXDataNode node = GetNodeFileRefToIcon ();

			string name = node.GetValueTypeName ((ITypeResolutionService) null);
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}

		[Test]
		public void GetValueTypeNameResXFileRefWrongITypeResolutionService ()
		{

			ResXDataNode node = GetNodeFileRefToIcon ();

			string name = node.GetValueTypeName (new DummyTypeResolutionService ());
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}

		[Test]
		public void GetValueResXFileRefWrongITypeResolutionService ()
		{
			ResXDataNode node = GetNodeFileRefToIcon ();

			Object ico = node.GetValue (new DummyTypeResolutionService ());
			Assert.IsNotNull (ico, "#A1");
			Assert.IsInstanceOfType (typeof (Icon), ico, "#A2");
		}

		[Test]
		public void GetValueTypeNameResXFileRefWrongAssemblyNames ()
		{

			ResXDataNode node = GetNodeFileRefToIcon ();

			AssemblyName [] ass = new AssemblyName [1];

			ass [0] = new AssemblyName ("System.Design");

			string name = node.GetValueTypeName (ass);
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}

		[Test]
		public void GetValueResXFileRefWrongAssemblyNames ()
		{
			ResXDataNode node = GetNodeFileRefToIcon ();

			AssemblyName [] ass = new AssemblyName [1];

			ass [0] = new AssemblyName ("System.Design");

			Object ico = node.GetValue (ass);
			Assert.IsNotNull (ico, "#A1");
			Assert.IsInstanceOfType (typeof (Icon), ico, "#A2");
		}

		#endregion

		[TearDown]
		protected override void TearDown ()
		{
			//teardown
			if (Directory.Exists (_tempDirectory))
				Directory.Delete (_tempDirectory, true);

			base.TearDown ();
		}

		string GetResXFileWithNode (ResXDataNode node)
		{
			string fileName;

			_tempDirectory = Path.Combine (Path.GetTempPath (), "ResXDataNodeTest");
			_otherTempDirectory = Path.Combine (_tempDirectory, "in");
			if (!Directory.Exists (_otherTempDirectory)) {
				Directory.CreateDirectory (_otherTempDirectory);
			}

			fileName = Path.Combine (_tempDirectory, "myresx.resx");

			using (ResXResourceWriter writer = new ResXResourceWriter (fileName)) {
				writer.AddResource (node);
			}

			return fileName;
		}

		ResXDataNode GetNodeFileRefToSerializable (string filename, bool assemblyQualifiedName)
		{
			_tempDirectory = Path.Combine (Path.GetTempPath (), "ResXDataNodeTest");
			_otherTempDirectory = Path.Combine (_tempDirectory, "in");
			if (!Directory.Exists (_otherTempDirectory)) {
				Directory.CreateDirectory (_otherTempDirectory);
			}

			string refFile = Path.Combine (_tempDirectory, filename);

			serializable ser = new serializable ("name", "value");

			WriteToFile (refFile, ser);

			string typeName;

			if (assemblyQualifiedName)
				typeName = typeof (serializable).AssemblyQualifiedName;
			else
				typeName = typeof (serializable).FullName;

			ResXFileRef fileRef = new ResXFileRef (refFile, typeName);

			ResXDataNode node = new ResXDataNode ("test", fileRef);

			return node;
		}

		private static void WriteToFile (string filepath, serializable ser)
		{

			Stream stream = File.Open (filepath, FileMode.Create);
			BinaryFormatter bFormatter = new BinaryFormatter ();
			bFormatter.Serialize (stream, ser);
			stream.Close ();
		}

		ResXDataNode GetNodeFileRefToIcon ()
		{
			_tempDirectory = Path.Combine (Path.GetTempPath (), "ResXDataNodeTest");
			_otherTempDirectory = Path.Combine (_tempDirectory, "in");
			if (!Directory.Exists (_otherTempDirectory)) {
				Directory.CreateDirectory (_otherTempDirectory);
			}

			string refFile = Path.Combine (_tempDirectory, "32x32.ico");
			WriteEmbeddedResource ("32x32.ico", refFile);

			ResXFileRef fileRef = new ResXFileRef (refFile, typeof (Icon).AssemblyQualifiedName);
			ResXDataNode node = new ResXDataNode ("test", fileRef);

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

	}

}
#endif