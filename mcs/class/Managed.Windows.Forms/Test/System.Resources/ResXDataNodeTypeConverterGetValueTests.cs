//
// ResXDataNodeTypeConverterGetValueTests.cs
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
			ResXDataNode node;
			node = new ResXDataNode ("along", 34L);

			object obj = node.GetValue (new ReturnIntITRS ());
			Assert.IsInstanceOfType (typeof (long), obj, "#A1");
		}

		[Test]
		public void ITRSUsedEachTimeWithNodeFromReader ()
		{
			ResXDataNode returnedNode, originalNode;
			originalNode = new ResXDataNode ("aNumber", 23L);
			returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");

			object newVal = returnedNode.GetValue (new ReturnIntITRS ());
			Assert.AreEqual (typeof (int).AssemblyQualifiedName, newVal.GetType ().AssemblyQualifiedName, "#A2");

			object origVal = returnedNode.GetValue ((ITypeResolutionService) null);
			Assert.AreEqual (typeof (long).AssemblyQualifiedName, origVal.GetType ().AssemblyQualifiedName, "#A3");
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

		[Test]
		public void AssemblyNameUsedWhereOnlyFullNameInResX ()
		{
			// DummyAssembly must be in the same directory as current assembly to work correctly
			string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

			ResXDataNode node = GetNodeFromResXReader (convertableResXWithoutAssemblyName);

			Assert.IsNotNull (node, "#A1");
			object obj = node.GetValue (assemblyNames);
			Assert.AreEqual ("DummyAssembly.Convertable, " + aName, obj.GetType ().AssemblyQualifiedName);
		}

		[Test, ExpectedException (typeof (TypeLoadException))]
		public void AssemblyNameRequiredEachTimeWhereOnlyFullNameInResX ()
		{
			// DummyAssembly must be in the same directory as current assembly to work correctly
			string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

			ResXDataNode node = GetNodeFromResXReader (convertableResXWithoutAssemblyName);

			Assert.IsNotNull (node, "#A1");
			object obj = node.GetValue (assemblyNames);
			Assert.AreEqual ("DummyAssembly.Convertable, " + aName, obj.GetType ().AssemblyQualifiedName, "#A2");
			object obj2 = node.GetValue ((AssemblyName []) null); //should cause exception here
			
		}
		//FIXME: does the way this test is run by NUnit affect the validity of the results showing that you need assembly name to pull type from current assembly?
		[Test, ExpectedException (typeof (TypeLoadException))]
		public void CantLoadTypeFromThisAssemblyWithOnlyFullName ()
		{
			ResXDataNode node = GetNodeFromResXReader (thisAssemblyConvertableResXWithoutAssemblyName);
			Assert.IsNotNull (node, "#A1");
			object obj = node.GetValue ((AssemblyName []) null);
		}

		[Test]
		public void CanLoadTypeFromThisAssemblyWithOnlyFullNamePassingAssemblyNames ()
		{
			string aName = this.GetType ().Assembly.FullName;
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

			ResXDataNode node = GetNodeFromResXReader (thisAssemblyConvertableResXWithoutAssemblyName);

			Assert.IsNotNull (node, "#A1");
			// would cause exception if couldnt find type
			object obj = node.GetValue (assemblyNames);
			Assert.IsInstanceOfType (typeof (ThisAssemblyConvertable), obj, "#A2");
		}

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
		public void NullITRSOK ()
		{
			ResXDataNode node = GetNodeEmdeddedIcon ();
			
			Object ico = node.GetValue ((ITypeResolutionService) null);
			Assert.IsNotNull (ico, "#A1");
			Assert.IsInstanceOfType (typeof (Icon), ico, "#A2");
		}
		
		[Test]
		public void WrongITRSOK ()
		{
			ResXDataNode node = GetNodeEmdeddedIcon ();
			
			Object ico = node.GetValue (new DummyITRS ());
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

		static string missingSerializableFromMissingAssembly =
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
  <data name=""aname"" mimetype=""application/x-microsoft.net.object.binary.base64"">
    <value>
        AAEAAAD/////AQAAAAAAAAAMAgAAAEZNaXNzaW5nQXNzZW1ibHksIFZlcnNpb249MS4wLjAuMCwgQ3Vs
        dHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1udWxsBQEAAAAhRHVtbXlBc3NlbWJseS5NaXNzaW5n
        U2VyaWFsaXphYmxlAgAAAAdzZXJuYW1lCHNlcnZhbHVlAQECAAAABgMAAAAFYW5hbWUGBAAAAAZhdmFs
        dWUL
</value>
  </data>
</root>";

		static string anotherSerializableFromDummyAssembly =
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
  <data name=""aname"" mimetype=""application/x-microsoft.net.object.binary.base64"">
    <value>
        AAEAAAD/////AQAAAAAAAAAMAgAAAEREdW1teUFzc2VtYmx5LCBWZXJzaW9uPTEuMC4wLjAsIEN1bHR1
        cmU9bmV1dHJhbCwgUHVibGljS2V5VG9rZW49bnVsbAUBAAAAIUR1bW15QXNzZW1ibHkuQW5vdGhlclNl
        cmlhbGl6YWJsZQIAAAAHc2VybmFtZQhzZXJ2YWx1ZQEBAgAAAAYDAAAABWFuYW1lBgQAAAAGYXZhbHVl
        Cw==
</value>
  </data>
</root>";

		static string convertableResXWithoutAssemblyName =
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
  
  <data name=""test"" type=""DummyAssembly.Convertable"">
	<value>im a name	im a value</value>
  </data>
</root>";

		static string thisAssemblyConvertableResXWithoutAssemblyName =
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
  
  <data name=""test"" type=""MonoTests.System.Resources.ThisAssemblyConvertable"">
	<value>im a name	im a value</value>
  </data>
</root>";
	}


}
#endif

