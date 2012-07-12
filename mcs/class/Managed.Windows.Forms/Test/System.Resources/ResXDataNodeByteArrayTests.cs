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
	public class ResXDataNodeByteArrayTests : ResourcesTestHelper {
		
		[Test]
		public void GetValueITRSNotUsedWhenNodeReturnedFromReader ()
		{
			ResXDataNode originalNode, returnedNode;
			originalNode = GetNodeEmdeddedBytes1To10 ();
            returnedNode = GetNodeFromResXReader (originalNode);

            Assert.IsNotNull (returnedNode, "#A1");
			object val = returnedNode.GetValue (new AlwaysReturnIntTypeResolutionService ());
			Assert.IsInstanceOfType (typeof (byte[]), val, "#A2");
		}

		[Test, ExpectedException (typeof (NotImplementedException))]
		public void GetValueITRSIsTouchedWhenNodeReturnedFromReader ()
		{
			ResXDataNode originalNode, returnedNode;
			originalNode = GetNodeEmdeddedBytes1To10 ();
            returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			//would raise error if touched
			object val = returnedNode.GetValue (new ExceptionalTypeResolutionService ());
		}

		[Test]
		public void GetValueITRSNotTouchedWhenNodeCreatedNew ()
		{
			// check supplied params to GetValue are not touched
			// for an instance created manually
			ResXDataNode node;
			node = GetNodeEmdeddedBytes1To10 ();

			//would raise exception if param used
			Object obj = node.GetValue (new ExceptionalTypeResolutionService ());
			Assert.IsInstanceOfType (typeof (byte[]), obj, "#A1");
		}

		[Test]
		public void GetValueTypeNameITRSIsUsedWithNodeFromReader ()
		{
			ResXDataNode originalNode, returnedNode;
			originalNode = GetNodeEmdeddedBytes1To10 ();
			returnedNode = GetNodeFromResXReader (originalNode);

            Assert.IsNotNull (returnedNode, "#A1");
			string returnedType = returnedNode.GetValueTypeName (new AlwaysReturnIntTypeResolutionService ());
			Assert.AreEqual ((typeof (int)).AssemblyQualifiedName, returnedType, "#A2");
		}

		[Test]
		public void GetValueTypeNameITRSIsUsedAfterGetValueCalledWithNodeFromReader ()
		{
			ResXDataNode originalNode, returnedNode;
			originalNode = GetNodeEmdeddedBytes1To10 ();
			returnedNode = GetNodeFromResXReader (originalNode);

            Assert.IsNotNull (returnedNode, "#A1");
			object obj = returnedNode.GetValue ((ITypeResolutionService) null);
			string returnedType = returnedNode.GetValueTypeName (new AlwaysReturnIntTypeResolutionService ());
			Assert.AreEqual ((typeof (int)).AssemblyQualifiedName, returnedType, "#A2");
		}

		[Test]
		public void GetValueTypeNameITRSNotUsedWhenNodeCreatedNew ()
		{
			// check supplying params to GetValueType of the ResXDataNode does not change the output
			// of the method for an instance created manually
			ResXDataNode node;
			node = GetNodeEmdeddedBytes1To10 ();

			string returnedType = node.GetValueTypeName (new AlwaysReturnIntTypeResolutionService ());
			Assert.AreEqual ((typeof (byte[])).AssemblyQualifiedName, returnedType, "#A1");
		}

		[Test]
		public void ChangesToReturnedByteArrayNotLaterWrittenBack ()
		{
            ResXDataNode originalNode, returnedNode, finalNode;
            originalNode = GetNodeEmdeddedBytes1To10 ();
			returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");

			object val = returnedNode.GetValue ((ITypeResolutionService) null);
			Assert.IsInstanceOfType (typeof (byte []), val, "#A2");

			byte[] newBytes = (byte[]) val;
			Assert.AreEqual (1, newBytes [0], "A3");
			newBytes [0] = 99;

			finalNode = GetNodeFromResXReader (returnedNode);
			
			Assert.IsNotNull (finalNode, "#A4");

			object finalVal = finalNode.GetValue ((ITypeResolutionService) null);
			Assert.IsInstanceOfType (typeof (byte []), finalVal, "#A5");
			byte [] finalBytes = (byte []) finalVal;
			// would be 99 if written back
			Assert.AreEqual (1,finalBytes [0],"A6");
		}

	}

	
}
#endif