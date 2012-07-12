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
using System.Resources;
using System.Collections;
using NUnit.Framework;
using System.ComponentModel.Design;

namespace MonoTests.System.Resources {
	[TestFixture]
	public class ResXDataNodeSerializedGetValueTests : ResourcesTestHelper {
		[Test]
		public void ITRSOnlyUsedFirstTimeWithNodeFromReader ()
		{
			ResXDataNode originalNode, returnedNode;
			originalNode = GetNodeEmdeddedSerializable ();
            returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");

			object defaultVal = returnedNode.GetValue ((ITypeResolutionService) null);
			Assert.IsInstanceOfType (typeof (serializable), defaultVal, "#A2");
			Assert.IsNotInstanceOfType (typeof (serializableSubClass), defaultVal, "#A3");

			object newVal = returnedNode.GetValue (new AlwaysReturnSerializableSubClassTypeResolutionService ());
			Assert.IsNotInstanceOfType (typeof (serializableSubClass), newVal, "#A4");
			Assert.IsInstanceOfType (typeof (serializable), newVal, "#A5");
		}

		[Test]
		public void ITRSUsedWhenNodeReturnedFromReader ()
		{
			ResXDataNode originalNode, returnedNode;
			originalNode = GetNodeEmdeddedSerializable ();
            returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");

			object val = returnedNode.GetValue (new AlwaysReturnSerializableSubClassTypeResolutionService ());
			Assert.IsInstanceOfType (typeof (serializableSubClass), val, "#A2");
		}

		[Test]
		public void OriginalTypeUsedWhenWritingBackToResX ()
		{
			// check although calls subsequent to an ITRS being supplied to GetValue return that resolved type
			// when the node is written back using ResXResourceWriter it uses the original type

			ResXDataNode originalNode, returnedNode, finalNode;

			originalNode = GetNodeEmdeddedSerializable ();
			returnedNode = GetNodeFromResXReader (originalNode);
            
			Assert.IsNotNull (returnedNode, "#A1");
			object val = returnedNode.GetValue (new AlwaysReturnSerializableSubClassTypeResolutionService ());
			Assert.IsInstanceOfType (typeof (serializableSubClass), val, "#A2");

			finalNode = GetNodeFromResXReader (returnedNode);
			Assert.IsNotNull (finalNode, "#A3");

			object finalVal = finalNode.GetValue ((ITypeResolutionService) null);
			Assert.IsNotInstanceOfType (typeof (serializableSubClass), finalVal, "#A4");
			Assert.IsInstanceOfType (typeof (serializable), finalVal, "#A5");
		}

		[Test]
		public void ITRSIsIgnoredIfGetValueTypeNameAlreadyCalledWithAnotherITRS ()
		{
			// check that first call to GetValueTypeName sets the type for GetValue

			ResXDataNode originalNode, returnedNode;

			originalNode = GetNodeEmdeddedSerializable ();
            returnedNode = GetNodeFromResXReader (originalNode);
			Assert.IsNotNull (returnedNode, "#A1");

			//get value type passing params
			string newType = returnedNode.GetValueTypeName (new AlwaysReturnSerializableSubClassTypeResolutionService ());
			Assert.AreEqual ((typeof (serializableSubClass)).AssemblyQualifiedName, newType, "#A2");
			Assert.AreNotEqual ((typeof (serializable)).AssemblyQualifiedName, newType, "#A3");

			// get value passing null params
			object val = returnedNode.GetValue ((ITypeResolutionService) null);
			// Assert.IsNotInstanceOfType (typeof (serializable), val, "#A5"); this would fail as subclasses are id-ed as instances of parents
			Assert.IsInstanceOfType (typeof (serializableSubClass), val, "#A4");
		}

		[Test]
		public void ITRSNotTouchedWhenNodeCreatedNew ()
		{
			// check supplied params to GetValue are not touched
			// for an instance created manually
			ResXDataNode node = GetNodeEmdeddedSerializable ();

			//would raise exception if param used
			Object obj = node.GetValue (new ExceptionalTypeResolutionService ());
			Assert.IsInstanceOfType (typeof (serializable), obj, "#A1");
		}

		[Test]
        public void ChangesToReturnedObjectNotLaterWrittenBack ()
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
	}
}
#endif

