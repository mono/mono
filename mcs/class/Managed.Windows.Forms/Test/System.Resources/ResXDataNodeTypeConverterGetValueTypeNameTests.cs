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
using System.Drawing;
using System.Reflection;

namespace MonoTests.System.Resources
{
	[TestFixture]
	public class ResXDataNodeTypeConverterGetValueTypeNameTests : ResourcesTestHelper {
		[Test]
		public void ITRSUsedWithNodeFromReader ()
		{
			// for node returned from ResXResourceReader for an object stored by means of a typeconverter, 
			// check supplying ITRS changes output of method
			ResXDataNode returnedNode, originalNode;
			originalNode = new ResXDataNode ("aNumber", 23L);
			returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			string returnedType = returnedNode.GetValueTypeName (new AlwaysReturnIntTypeResolutionService ());
			Assert.AreEqual ((typeof (Int32)).AssemblyQualifiedName, returnedType, "#A2");
		}

		[Test]
		public void ITRSUsedEachTimeWhenNodeFromReader ()
		{
			// for node returned from ResXResourceReader for an object stored by means of a typeconverter, 
			// check supplied ITRS changes output each time
			ResXDataNode returnedNode, originalNode;
			originalNode = new ResXDataNode ("aNumber", 23L);
			returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			string newType = returnedNode.GetValueTypeName (new AlwaysReturnIntTypeResolutionService ());
			Assert.AreEqual (typeof (int).AssemblyQualifiedName, newType, "#A2");
			string origType = returnedNode.GetValueTypeName ((ITypeResolutionService) null);
			Assert.AreEqual (typeof (long).AssemblyQualifiedName, origType, "#A3");				
			
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

		#region initial

		[Test]
		public void EmbeddedNullITypeResolutionServiceOK ()
		{
			ResXDataNode node = GetNodeEmdeddedIcon ();

			string name = node.GetValueTypeName ((ITypeResolutionService) null);
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}

		[Test]
		public void EmbeddedWrongITypeResolutionServiceOK ()
		{
			ResXDataNode node = GetNodeEmdeddedIcon ();

			string name = node.GetValueTypeName (new DummyTypeResolutionService ());
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}

		[Test]
		public void EmbeddedWrongAssemblyNamesOK ()
		{
			ResXDataNode node = GetNodeEmdeddedIcon ();
			AssemblyName [] ass = new AssemblyName [1];

			ass [0] = new AssemblyName ("System.Design");

			string name = node.GetValueTypeName (ass);
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}

		[Test]
		public void EmbeddedNullAssemblyNamesOK ()
		{
			ResXDataNode node = GetNodeEmdeddedIcon ();

			string name = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}

		#endregion
	}


}
#endif

