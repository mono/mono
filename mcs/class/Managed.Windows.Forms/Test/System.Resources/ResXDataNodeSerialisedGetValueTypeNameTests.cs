//
// ResXDataNodeSerializedGetValueTypeNameTests.cs
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
			string returnedType = returnedNode.GetValueTypeName (new ReturnSerializableSubClassITRS ());
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

			string newType = returnedNode.GetValueTypeName (new ReturnSerializableSubClassITRS ());
			Assert.AreNotEqual ((typeof (serializableSubClass)).AssemblyQualifiedName, newType, "#A3");
			Assert.AreEqual ((typeof (serializable)).AssemblyQualifiedName, newType, "#A4");
		}
		
		[Test]
		public void ITRSNotUsedWhenNodeCreatedNew ()
		{
			ResXDataNode node;
			node = GetNodeEmdeddedSerializable ();

			string returnedType = node.GetValueTypeName (new ReturnSerializableSubClassITRS ());
			Assert.AreEqual ((typeof (serializable)).AssemblyQualifiedName, returnedType, "#A1");
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
			string newType = returnedNode.GetValueTypeName (new ReturnSerializableSubClassITRS ());
			Assert.AreNotEqual ((typeof (serializableSubClass)).AssemblyQualifiedName, newType, "#A4");
			Assert.AreEqual ((typeof (serializable)).AssemblyQualifiedName, newType, "#A5");
		}

		[Test]
		public void SoapFormattedObject ()
		{
			ResXDataNode node = GetNodeFromResXReader (serializedResXSOAP);

			Assert.IsNotNull (node, "#A1");
			// hard coded assembly name value refers to that generated under 2.0 prefix, so use compatible available class
			string name = node.GetValueTypeName (new ReturnSerializableSubClassITRS ());
			Assert.AreEqual (typeof (serializableSubClass).AssemblyQualifiedName, name, "#A2");
		}

		[Test]
		public void DeserializationErrorReturnsObjectType ()
		{
			ResXDataNode node = GetNodeFromResXReader (serializedResXCorruped);
			Assert.IsNotNull (node, "#A1");
			string type = node.GetValueTypeName ((AssemblyName []) null);

			Assert.AreEqual (typeof (object).AssemblyQualifiedName,type, "#A2");
		}
		
		[Test]
		public void InvalidMimeTypeFromReaderReturnsNull ()
		{
			ResXDataNode node = GetNodeFromResXReader (serializedResXInvalidMimeType);
			Assert.IsNotNull (node, "#A1");
			string type = node.GetValueTypeName ((AssemblyName []) null);
			Assert.IsNull (type, "#A2");
		}

		[Test]
		public void AssemblyAutomaticallyLoaded ()
		{
			// DummyAssembly must be in the same directory as current assembly to work correctly
			ResXDataNode node = GetNodeFromResXReader (anotherSerializableFromDummyAssembly);
			Assert.IsNotNull (node, "#A1");
			string type = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual ("DummyAssembly.AnotherSerializable, DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", type, "#A2");
		}

		[Test]
		public void ReturnsObjectAssemblyMissing ()
		{
			ResXDataNode node = GetNodeFromResXReader (missingSerializableFromMissingAssembly);
			Assert.IsNotNull (node, "#A1");
			string type = node.GetValueTypeName ((AssemblyName []) null);
			Assert.AreEqual (typeof (object).AssemblyQualifiedName, type, "#A2");
		}

		static string serializedResXInvalidMimeType =
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
  <data name=""test"" mimetype=""application/xxxx"">
	<value>
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
</value>
  </data>
</root>";

		static string serializedResXCorruped =
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
  <data name=""test"" mimetype=""application/x-microsoft.net.object.binary.base64"">
	<value>
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
		AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
</value>
  </data>
</root>";

		static string serializedResXSOAP =
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
  <data name=""test"" mimetype=""application/x-microsoft.net.object.soap.base64"">
	<value>
		PFNPQVAtRU5WOkVudmVsb3BlIHhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2Ui
		IHhtbG5zOnhzZD0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEiIHhtbG5zOlNPQVAtRU5DPSJodHRwOi8vc2No
		ZW1hcy54bWxzb2FwLm9yZy9zb2FwL2VuY29kaW5nLyIgeG1sbnM6U09BUC1FTlY9Imh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAu
		b3JnL3NvYXAvZW52ZWxvcGUvIiB4bWxuczpjbHI9Imh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vc29hcC9lbmNvZGlu
		Zy9jbHIvMS4wIiBTT0FQLUVOVjplbmNvZGluZ1N0eWxlPSJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy9zb2FwL2VuY29k
		aW5nLyI+DQo8U09BUC1FTlY6Qm9keT4NCjxhMTpzZXJpYWxpemFibGUgaWQ9InJlZi0xIiB4bWxuczphMT0iaHR0cDovL3Nj
		aGVtYXMubWljcm9zb2Z0LmNvbS9jbHIvbnNhc3NlbS9Nb25vVGVzdHMuU3lzdGVtLlJlc291cmNlcy9TeXN0ZW0uV2luZG93
		cy5Gb3Jtc190ZXN0X25ldF8yXzAlMkMlMjBWZXJzaW9uJTNEMC4wLjAuMCUyQyUyMEN1bHR1cmUlM0RuZXV0cmFsJTJDJTIw
		UHVibGljS2V5VG9rZW4lM0RudWxsIj4NCjxzZXJuYW1lIGlkPSJyZWYtMyI+YW5hbWU8L3Nlcm5hbWU+DQo8c2VydmFsdWUg
		aWQ9InJlZi00Ij5hdmFsdWU8L3NlcnZhbHVlPg0KPC9hMTpzZXJpYWxpemFibGU+DQo8L1NPQVAtRU5WOkJvZHk+DQo8L1NP
		QVAtRU5WOkVudmVsb3BlPg==
	</value>
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

