//
// ResXDataNodeTypeConverterGetValueTypeNameTests.cs
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
using System.Drawing;
using System.Reflection;

namespace MonoTests.System.Resources
{
	[TestFixture]
	public class ResXDataNodeTypeConverterGetValueTypeNameTests : ResourcesTestHelper {
		[Test]
		public void ITRSUsedWithNodeFromReader ()
		{
			ResXDataNode returnedNode, originalNode;
			originalNode = new ResXDataNode ("aNumber", 23L);
			returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			string returnedType = returnedNode.GetValueTypeName (new ReturnIntITRS ());
			Assert.AreEqual ((typeof (Int32)).AssemblyQualifiedName, returnedType, "#A2");
		}

		[Test]
		public void ITRSUsedEachTimeWhenNodeFromReader ()
		{
			ResXDataNode returnedNode, originalNode;
			originalNode = new ResXDataNode ("aNumber", 23L);
			returnedNode = GetNodeFromResXReader (originalNode);

			Assert.IsNotNull (returnedNode, "#A1");
			string newType = returnedNode.GetValueTypeName (new ReturnIntITRS ());
			Assert.AreEqual (typeof (int).AssemblyQualifiedName, newType, "#A2");
			string origType = returnedNode.GetValueTypeName ((ITypeResolutionService) null);
			Assert.AreEqual (typeof (long).AssemblyQualifiedName, origType, "#A3");				
		}

		[Test]
		public void ITRSNotUsedWhenNodeCreatedNew ()
		{
			ResXDataNode node;
			node = new ResXDataNode ("along", 34L);

			string returnedType = node.GetValueTypeName (new ReturnIntITRS ());
			Assert.AreEqual ((typeof (long)).AssemblyQualifiedName, returnedType, "#A1");
		}

		[Test]
		public void InvalidMimeTypeReturnedFromReader_ReturnsStringIfCantResolveType ()
		{
			ResXDataNode node = GetNodeFromResXReader (typeconResXInvalidMimeTypeAndType);
			Assert.IsNotNull (node, "#A1");
			string type = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual ("A.type", type, "#A2");
		}

		[Test]
		public void InvalidMimeTypeReturnedFromReader_TriesToResolve ()
		{
			ResXDataNode node = GetNodeFromResXReader (typeconResXInvalidMimeType);
			Assert.IsNotNull (node, "#A1");
			string type = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual (typeof (string).AssemblyQualifiedName, type, "#A2");
		}

		[Test]
		public void ReturnsFullNameWereOnlyFullNameInResX ()
		{
			ResXDataNode node = GetNodeFromResXReader (convertableResXWithoutAssemblyName);

			Assert.IsNotNull (node, "#A1");
			string returnedType = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual ("DummyAssembly.Convertable", returnedType, "#A2");
		}

		[Test]
		public void AssemblyNameUsedWhereOnlyFullNameInResX ()
		{
			// DummyAssembly must be in the same directory as current assembly to work correctly
			string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

			ResXDataNode node = GetNodeFromResXReader (convertableResXWithoutAssemblyName);

			Assert.IsNotNull (node, "#A1");
			string returnedType = node.GetValueTypeName (assemblyNames);
			Assert.AreEqual ("DummyAssembly.Convertable, " + aName, returnedType, "#A2");
		}

		[Test]
		public void AssemblyNameUsedEachTimeWhereOnlyFullNameInResX ()
		{
			// DummyAssembly must be in the same directory as current assembly to work correctly
			string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
			AssemblyName [] assemblyNames = new AssemblyName [] { new AssemblyName (aName) };

			ResXDataNode node = GetNodeFromResXReader (convertableResXWithoutAssemblyName);

			Assert.IsNotNull (node, "#A1");
			string returnedName = node.GetValueTypeName (assemblyNames);
			Assert.AreEqual ("DummyAssembly.Convertable, " + aName, returnedName, "#A2");
			string nameWithNullParam = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual ("DummyAssembly.Convertable", nameWithNullParam, "#A3");
		}

		#region initial
		
		[Test]
		public void NullITRSServiceOK ()
		{
			ResXDataNode node = GetNodeEmdeddedIcon ();
			
			string name = node.GetValueTypeName ((ITypeResolutionService) null);
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}
		
		[Test]
		public void WrongITRSOK ()
		{
			ResXDataNode node = GetNodeEmdeddedIcon ();
			
			string name = node.GetValueTypeName (new DummyITRS ());
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}
		
		[Test]
		public void WrongAssemblyNamesOK ()
		{
			ResXDataNode node = GetNodeEmdeddedIcon ();
			AssemblyName [] ass = new AssemblyName [1];
			
			ass [0] = new AssemblyName ("System.Design");
			
			string name = node.GetValueTypeName (ass);
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}
		
		[Test]
		public void NullAssemblyNamesOK ()
		{
			ResXDataNode node = GetNodeEmdeddedIcon ();
			
			string name = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual (typeof (Icon).AssemblyQualifiedName, name);
		}
		
#endregion

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

		static string typeconResXInvalidMimeType =
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
  <data name=""test"" type=""System.String"" mimetype=""application/xxxx"">
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

