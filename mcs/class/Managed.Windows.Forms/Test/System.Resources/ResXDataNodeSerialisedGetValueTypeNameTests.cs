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
using System.ComponentModel.Design;
using System.Runtime.Serialization.Formatters.Binary;

namespace MonoTests.System.Resources {
	[TestFixture]
	public class ResXDataNodeSerializedGetValueTypeNameTests : ResourcesTestHelper {
		[Test]
		public void ITRSUsedWithNodeFromReader ()
		{
			ResXDataNode originalNode, returnedNode;
			originalNode = GetNodeEmdeddedSerializable ();
            returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			string returnedType = returnedNode.GetValueTypeName (new AlwaysReturnSerializableSubClassTypeResolutionService ());
			Assert.AreEqual ((typeof (serializableSubClass)).AssemblyQualifiedName, returnedType, "#A2");
		}

		[Test]
		public void ITRSOnlyUsedFirstTimeWithNodeFromReader ()
		{
			// check ITRS supplied to GetValueTypeName method for a node returned from reader are used when 
			// retrieving the value first time and returns this same value ignoring any new ITRS passed thereafter
			ResXDataNode originalNode, returnedNode;
			originalNode = GetNodeEmdeddedSerializable ();
            returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			string defaultType = returnedNode.GetValueTypeName ((ITypeResolutionService) null);
			Assert.AreEqual ((typeof (serializable)).AssemblyQualifiedName, defaultType, "#A2");

			string newType = returnedNode.GetValueTypeName (new AlwaysReturnSerializableSubClassTypeResolutionService ());
			Assert.AreNotEqual ((typeof (serializableSubClass)).AssemblyQualifiedName, newType, "#A3");
			Assert.AreEqual ((typeof (serializable)).AssemblyQualifiedName, newType, "#A4");
		}
		
		[Test]
		public void ITRSNotUsedWhenNodeCreatedNew ()
		{
			// check supplying params to GetValueType of the UseResXDataNode does not change the output
			// of the method for an instance created manually
			ResXDataNode node;
			node = GetNodeEmdeddedSerializable ();

			string returnedType = node.GetValueTypeName (new AlwaysReturnSerializableSubClassTypeResolutionService ());
			Assert.AreEqual ((typeof (serializable)).AssemblyQualifiedName, returnedType, "#A1");
		}

		[Test]
		public void ITRSNotTouchedWhenNodeCreatedNew ()
		{
			// check supplied params to GetValueType of the UseResXDataNode are not touched
			// for an instance created manually
			ResXDataNode node;
			node = GetNodeEmdeddedSerializable ();

			// would raise exception if accessed
			string returnedType = node.GetValueTypeName (new ExceptionalTypeResolutionService ());
			//Assert.AreEqual ((typeof (serializable)).AssemblyQualifiedName, returnedType, "#A1");
		}

		[Test]
		public void ITRSIsIgnoredIfGetValueAlreadyCalledWithAnotherITRS ()
		{
			// check that first call to GetValue sets the type for GetValueTypeName
			ResXDataNode originalNode, returnedNode;
			originalNode = GetNodeEmdeddedSerializable ();
            returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			// get value passing no params
			object val = returnedNode.GetValue ((ITypeResolutionService) null);
			Assert.IsInstanceOfType (typeof (serializable), val, "#A2");
			Assert.IsNotInstanceOfType (typeof (serializableSubClass), val, "#A3");

			//get value type passing different params
			string newType = returnedNode.GetValueTypeName (new AlwaysReturnSerializableSubClassTypeResolutionService ());
			Assert.AreNotEqual ((typeof (serializableSubClass)).AssemblyQualifiedName, newType, "#A4");
			Assert.AreEqual ((typeof (serializable)).AssemblyQualifiedName, newType, "#A5");
		}
	}
	
}
#endif

