//
// ResXDataNodeAliasTests.cs : These tests are not a lot of use. Support for 
// Aliases not implemented as of Aug 2012. 
// FIXME: delete these tests?
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
using System.Resources;
using System.Collections;
using NUnit.Framework;

namespace MonoTests.System.Resources {
	[TestFixture]
	public class ResXDataNodeAliasTests : ResourcesTestHelper {
		
		[Test, ExpectedException (typeof (TypeLoadException))]
		public void CantAccessValueWereOnlyFullNameInResXForEmbedded () // same as validity check in assemblynames tests
		{
			ResXDataNode node = GetNodeFromResXReader (convertableResX);
			Assert.IsNotNull (node, "#A1");
			object obj = node.GetValue ((AssemblyName []) null);
		}

		[Test, ExpectedException (typeof (TypeLoadException))]
		public void CantAccessValueWereOnlyFullNameAndAliasInResXForEmbedded ()
		{
			ResXDataNode node = GetNodeFromResXReader (convertableResXAlias);
			Assert.IsNotNull (node, "#A1");
			object obj = node.GetValue ((AssemblyName []) null);
		}

		[Test]
		public void CanAccessValueWereOnlyFullNameAndAssemblyInResXForEmbedded ()
		{
			ResXDataNode node = GetNodeFromResXReader (convertableResXAssembly);

			Assert.IsNotNull (node, "#A1");
			object obj = node.GetValue ((AssemblyName []) null);
			// this is the qualified name of the assembly found in dir
			string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

			Assert.AreEqual ("DummyAssembly.Convertable, " + aName, obj.GetType ().AssemblyQualifiedName, "#A2");
		}

		[Test]
		public void CanAccessValueWereFullNameAndQualifiedAssemblyInResXForEmbedded ()
		{
			ResXDataNode node = GetNodeFromResXReader (convertableResXQualifiedAssemblyName);
			Assert.IsNotNull (node, "#A1");

			object obj = node.GetValue ((AssemblyName []) null);
			string aName = "DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
			Assert.AreEqual ("DummyAssembly.Convertable, " + aName, obj.GetType ().AssemblyQualifiedName, "#A2");
		}

		static string convertableResX =
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


		static string convertableResXAssembly =
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
  
  <data name=""test"" type=""DummyAssembly.Convertable, DummyAssembly"">
	<value>im a name	im a value</value>
  </data>
</root>";


		static string convertableResXAlias =
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
  <assembly alias=""DummyAssembly"" name=""DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"" />
  <data name=""test"" type=""DummyAssembly.Convertable"">
	<value>im a name	im a value</value>
  </data>
</root>";

		static string convertableResXQualifiedAssemblyName =
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
  
  <data name=""test"" type=""DummyAssembly.Convertable, DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"">
	<value>im a name	im a value</value>
  </data>
</root>";

	}

}
#endif
