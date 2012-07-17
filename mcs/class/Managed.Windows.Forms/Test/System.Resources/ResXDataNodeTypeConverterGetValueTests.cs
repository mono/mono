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
using System.Reflection;
using System.Drawing;

namespace MonoTests.System.Resources
{
	[TestFixture]
	public class ResXDataNodeTypeConverterGetValueTests : ResourcesTestHelper {
		[Test]
		public void ITRSNotUsedWhenCreatedNew ()
		{
			// check supplying params to GetValue of the UseResXDataNode does not change the output
			// of the method for an instance created manually
			ResXDataNode node;
			node = new ResXDataNode ("along", 34L);

			object obj = node.GetValue (new AlwaysReturnIntTypeResolutionService ());
			Assert.IsInstanceOfType (typeof (long), obj, "#A1");
		}

		[Test]
		public void ITRSUsedEachTimeWithNodeFromReader ()
		{
			// check GetValue uses ITRS param each time its called for a node from a ResXResourceReader 
			// for an object stored by means of a typeconverter, 
			ResXDataNode returnedNode, originalNode;
			originalNode = new ResXDataNode ("aNumber", 23L);
			returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");

			object newVal = returnedNode.GetValue (new AlwaysReturnIntTypeResolutionService ());
			Assert.AreEqual (typeof (int).AssemblyQualifiedName, newVal.GetType ().AssemblyQualifiedName, "#A2");

			object origVal = returnedNode.GetValue ((ITypeResolutionService) null);
			Assert.AreEqual (typeof (long).AssemblyQualifiedName, origVal.GetType ().AssemblyQualifiedName, "#A3");
		}

		[Test, ExpectedException (typeof (NotImplementedException))]
		public void GetValueParamIsTouchedWhenEmbeddedReturnedFromResXResourceReader ()
		{
			// after running the enumerator of ResXResourceReader with UseResXDataNodes set 
			// to true, check params supplied to GetValue method
			// of ResXDataNode are used to deserialise
			// for now just throwing exception in param object to ensure its accessed
			ResXDataNode originalNode, returnedNode;
				
			originalNode = GetNodeEmdeddedIcon ();
			returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			// should raise error
			Icon ico = (Icon)returnedNode.GetValue (new ExceptionalTypeResolutionService ());
		}

		[Test]
		public void InvalidMimeTypeAndTypeReturnedFromReader_ObjectIsNull ()
		{
			ResXDataNode node = GetNodeFromResXReader (typeconResXInvalidMimeTypeAndType);
			Assert.IsNotNull (node, "#A1");
			object obj = node.GetValue ((AssemblyName []) null);
			Assert.IsNull (obj, "#A2");
		}

		[Test, ExpectedException (typeof (TypeLoadException))]
		public void InvalidTypeReturnedFromReader_Exceptions ()
		{
			ResXDataNode node = GetNodeFromResXReader (typeconResXInvalidType);
			Assert.IsNotNull (node, "#A1");
			object obj = node.GetValue ((AssemblyName []) null);
		}

		static string typeconResXInvalidType =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  
  <resheader name=""resmimetype"">
	<value>text/microsoft-resx</value>
  </resheader>
  <resheader name=""version"">
	<value>2.0</value>
  </resheader>
  <resheader name=""reader"">
	<value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name=""writer"">
	<value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <data name=""test"" type=""A.type"">
	<value>42</value>
  </data>
</root>";

		static string typeconResXInvalidMimeTypeAndType =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  
  <resheader name=""resmimetype"">
	<value>text/microsoft-resx</value>
  </resheader>
  <resheader name=""version"">
	<value>2.0</value>
  </resheader>
  <resheader name=""reader"">
	<value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name=""writer"">
	<value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <data name=""test"" type=""A.type"" mimetype=""application/xxxx"">
	<value>42</value>
  </data>
</root>";

		#region initial

		[Test]
		public void NullAssemblyNamesOK ()
		{
			ResXDataNode node = GetNodeEmdeddedIcon ();

			Object ico = node.GetValue ((AssemblyName []) null);
			Assert.IsNotNull (ico, "#A1");
			Assert.IsInstanceOfType (typeof (Icon), ico, "#A2");
		}

		[Test]
		public void NullITypeResolutionServiceOK ()
		{
			ResXDataNode node = GetNodeEmdeddedIcon ();

			Object ico = node.GetValue ((ITypeResolutionService) null);
			Assert.IsNotNull (ico, "#A1");
			Assert.IsInstanceOfType (typeof (Icon), ico, "#A2");
		}

		[Test]
		public void WrongITypeResolutionServiceOK ()
		{
			ResXDataNode node = GetNodeEmdeddedIcon ();

			Object ico = node.GetValue (new DummyTypeResolutionService ());
			Assert.IsNotNull (ico, "#A1");
			Assert.IsInstanceOfType (typeof (Icon), ico, "#A2");
		}

		[Test]
		public void WrongAssemblyNamesOK ()
		{
			ResXDataNode node = GetNodeEmdeddedIcon ();
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

