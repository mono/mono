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
	public class ResXDataNodeFileRefGetValueTests : ResourcesTestHelper {
		/*
		[Test, ExpectedException (typeof (NotImplementedException))]
		public void ITRSTouchedWhenNodeFromReader ()
		{
			// for a node returned from ResXResourceReader with FileRef, 
			// check params supplied to GetValue method of ResXDataNode are touched
			ResXDataNode originalNode, returnedNode;
			originalNode = GetNodeFileRefToSerializable ("ser.bbb",true);
			returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			// raises error if touched
			Icon ico = (Icon) returnedNode.GetValue (new ExceptionalTypeResolutionService ());
		}
		*/
		[Test]
		public void ITRSNotUsedWhenNodeFromReader ()
		{
			// for a node returned from reader with a FileRef, 
			// check ITRS supplied to GetValue method not actually used
			ResXDataNode originalNode, returnedNode;
			originalNode = GetNodeFileRefToSerializable ("ser.bbb",true);
			returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			object val = returnedNode.GetValue (new AlwaysReturnSerializableSubClassTypeResolutionService ());
			Assert.IsNotInstanceOfType (typeof (serializableSubClass), val, "#A2");
			Assert.IsInstanceOfType (typeof (serializable), val, "#A3");
		}

		[Test, ExpectedException(typeof (TypeLoadException))]
		public void CantGetValueWithOnlyFullNameAsType ()
		{
			ResXDataNode originalNode, returnedNode;
			originalNode = GetNodeFileRefToSerializable ("ser.bbb", false);
			returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			object obj = returnedNode.GetValue ((AssemblyName[]) null);
		}

		[Test, ExpectedException (typeof (TypeLoadException))]
		public void CantGetValueWithOnlyFullNameAsTypeByProvidingAssemblyName ()
		{
			ResXDataNode originalNode, returnedNode;

			string aName = "System.Windows.Forms_test_net_2_0, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

			originalNode = GetNodeFileRefToSerializable ("ser.bbb", false);
			returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			object obj = returnedNode.GetValue (assemblyNames);
		}
		/*
		[Test, ExpectedException (typeof (NotImplementedException))]
		public void ITRSTouchedWhenNodeCreatedNew ()
		{
			// check supplyied params to GetValue of the ResXDataNode are touched for
			// an instance initialised by me
			ResXDataNode node;
			node = GetNodeFileRefToSerializable ("ser.bbb",true);

			//raises exception if param touched
			Object obj = node.GetValue (new ExceptionalTypeResolutionService ());
		}
		*/
		[Test]
		public void ITRSNotUsedWhenNodeCreatedNew ()
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
		public void ResXFileRefNullAssemblyNamesOK ()
		{
			ResXDataNode node = GetNodeFileRefToIcon ();

			Object ico = node.GetValue ((AssemblyName []) null);
			Assert.IsNotNull (ico, "#A1");
			Assert.IsInstanceOfType (typeof (Icon), ico, "#A2");
		}

		[Test]
		public void ResXFileRefNullITypeResolutionServiceOK ()
		{
			ResXDataNode node = GetNodeFileRefToIcon ();

			Object ico = node.GetValue ((ITypeResolutionService) null);
			Assert.IsNotNull (ico, "#A1");
			Assert.IsInstanceOfType (typeof (Icon), ico, "#A2");
		}

		[Test]
		public void ResXFileRefWrongITypeResolutionServiceOK ()
		{
			ResXDataNode node = GetNodeFileRefToIcon ();

			Object ico = node.GetValue (new DummyTypeResolutionService ());
			Assert.IsNotNull (ico, "#A1");
			Assert.IsInstanceOfType (typeof (Icon), ico, "#A2");
		}
		
		[Test]
		public void ResXFileRefWrongAssemblyNamesOK ()
		{
			ResXDataNode node = GetNodeFileRefToIcon ();
			AssemblyName [] ass = new AssemblyName [1];
			ass [0] = new AssemblyName ("System.Design");

			Object ico = node.GetValue (ass);
			Assert.IsNotNull (ico, "#A1");
			Assert.IsInstanceOfType (typeof (Icon), ico, "#A2");
		}

		#endregion
		
	}

}
#endif