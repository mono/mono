//
// ResXDataNodeFileRefGetValueTypeNameTests.cs
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

			string aName = this.GetType ().Assembly.FullName;
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
			ResXDataNode originalNode, returnedNode;
			originalNode = GetNodeFileRefToSerializable ("ser.bbb",true);
			returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			string returnedType = returnedNode.GetValueTypeName (new ReturnSerializableSubClassITRS ());
			Assert.AreEqual ((typeof (serializableSubClass)).AssemblyQualifiedName, returnedType, "#A2");
		}

		[Test]
		public void ITRSUsedWhenNodeCreatedNew ()
		{
			ResXDataNode node;
			node = GetNodeFileRefToSerializable ("ser.bbb",true);

			string returnedType = node.GetValueTypeName (new ReturnSerializableSubClassITRS ());
			Assert.AreEqual ((typeof (serializableSubClass)).AssemblyQualifiedName, returnedType, "#A1");
		}

		[Test]
		public void IfTypeResolutionFailsReturnsOrigString()
		{
			ResXFileRef fileRef = new ResXFileRef ("afile.name", "a.type.name");
			ResXDataNode node = new ResXDataNode ("aname", fileRef);

			string returnedType = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual ("a.type.name", returnedType);
		}

		[Test]
		public void AttemptsTypeResolution ()
		{
			ResXFileRef fileRef = new ResXFileRef ("afile.name", "System.String");
			ResXDataNode node = new ResXDataNode ("aname", fileRef);

			string returnedType = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual (typeof (string).AssemblyQualifiedName, returnedType);
		}

		#region Initial Exploratory Tests

		[Test]
		public void NullAssemblyNamesOK ()
		{
			ResXDataNode node = GetNodeFileRefToIcon ();

			string name = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}

		[Test]
		public void NullITRSOK ()
		{
			ResXDataNode node = GetNodeFileRefToIcon ();

			string name = node.GetValueTypeName ((ITypeResolutionService) null);
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}

		[Test]
		public void WrongITRSOK ()
		{
			ResXDataNode node = GetNodeFileRefToIcon ();

			string name = node.GetValueTypeName (new DummyITRS ());
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}

		[Test]
		public void WrongAssemblyNamesOK ()
		{
			ResXDataNode node = GetNodeFileRefToIcon ();
			AssemblyName [] ass = new AssemblyName [1];
			ass [0] = new AssemblyName ("DummyAssembly");

			string name = node.GetValueTypeName (ass);
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}

		#endregion
	}

}
#endif