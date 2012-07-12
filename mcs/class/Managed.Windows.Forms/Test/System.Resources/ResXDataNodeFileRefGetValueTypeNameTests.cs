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
//  Gary Barnett

#if NET_2_0
using System;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Resources;
using System.Collections;
using NUnit.Framework;
using System.ComponentModel.Design;
using System.Runtime.Serialization.Formatters.Binary;

namespace MonoTests.System.Resources {
	[TestFixture]
	public class ResXDataNodeFileRefGetValueTypeNameTests : ResourcesTestHelper {
		[Test]
		public void CanGetStrongNameFromGetValueTypeNameWithOnlyFullNameAsTypeByProvidingAssemblyName ()
		{
			ResXDataNode originalNode, returnedNode;

			string aName = "System.Windows.Forms_test_net_2_0, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

			originalNode = GetNodeFileRefToSerializable ("ser.bbb", false);
            returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			string typeName = returnedNode.GetValueTypeName (assemblyNames);
			Assert.AreEqual ("MonoTests.System.Resources.serializable, " + aName, typeName, "#A2");
		}

		public void CanGetValueTypeNameWithOnlyFullNameAsType ()
		{
			ResXDataNode originalNode, returnedNode;

			originalNode = GetNodeFileRefToSerializable ("ser.bbb", false);
            returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			string typeName = returnedNode.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual ((typeof (serializable)).FullName, typeName, "#A2");
		}

		[Test]
		public void ITRSUsedWhenNodeFromReader ()
		{
			// for a node returned from ResXResourceReader, check supplying params to 
			// GetValueTypeName changes the output of the method
			ResXDataNode originalNode, returnedNode;
			originalNode = GetNodeFileRefToSerializable ("ser.bbb",true);
            returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			string returnedType = returnedNode.GetValueTypeName (new AlwaysReturnSerializableSubClassTypeResolutionService ());
			Assert.AreEqual ((typeof (serializableSubClass)).AssemblyQualifiedName, returnedType, "#A2");
		}

		[Test]
		public void ITRSUsedWhenNodeCreatedNew ()
		{
			// check supplying params GetValueTypeName of the 
			// UseResXDataNode does the output of the method for an instance
			// initialised manually
			ResXDataNode node;
			node = GetNodeFileRefToSerializable ("ser.bbb",true);

			string returnedType = node.GetValueTypeName (new AlwaysReturnSerializableSubClassTypeResolutionService ());
			Assert.AreEqual ((typeof (serializableSubClass)).AssemblyQualifiedName, returnedType, "#A1");
		}

		#region Initial Exploratory Tests

		[Test]
		public void ResXFileRefNullAssemblyNamesOK ()
		{
			ResXDataNode node = GetNodeFileRefToIcon ();

			string name = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}

		[Test]
		public void ResXFileRefNullITypeResolutionServiceOK ()
		{
			ResXDataNode node = GetNodeFileRefToIcon ();

			string name = node.GetValueTypeName ((ITypeResolutionService) null);
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}

		[Test]
		public void ResXFileRefWrongITypeResolutionServiceOK ()
		{
			ResXDataNode node = GetNodeFileRefToIcon ();

			string name = node.GetValueTypeName (new DummyTypeResolutionService ());
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}

		[Test]
		public void ResXFileRefWrongAssemblyNamesOK ()
		{
			ResXDataNode node = GetNodeFileRefToIcon ();
			AssemblyName [] ass = new AssemblyName [1];
			ass [0] = new AssemblyName ("System.Design");

			string name = node.GetValueTypeName (ass);
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}

		#endregion
        
	}

}
#endif