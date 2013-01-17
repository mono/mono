//
// ResXDataNodeSerializedGetValueTests.cs
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
using System.Runtime.Serialization.Formatters.Soap;
using System.Reflection;
using System.Text;
using System.Runtime.Serialization;

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

			object newVal = returnedNode.GetValue (new ReturnSerializableSubClassITRS ());
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

			object val = returnedNode.GetValue (new ReturnSerializableSubClassITRS ());
			Assert.IsInstanceOfType (typeof (serializableSubClass), val, "#A2");
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
			string newType = returnedNode.GetValueTypeName (new ReturnSerializableSubClassITRS ());
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
			Object obj = node.GetValue (new ExceptionalITRS ());
			Assert.IsInstanceOfType (typeof (serializable), obj, "#A1");
		}

		[Test, ExpectedException (typeof (SerializationException))]
		public void DeserializationError ()
		{
			ResXDataNode node = GetNodeFromResXReader (serializedResXCorruped);
			Assert.IsNotNull (node, "#A1");
			object val = node.GetValue ((AssemblyName []) null);
		}
		
		[Test]
		public void InvalidMimeTypeFromReaderReturnsNull ()
		{
			ResXDataNode node = GetNodeFromResXReader (serializedResXInvalidMimeType);
			Assert.IsNotNull (node, "#A1");
			object val = node.GetValue ((AssemblyName []) null);
			Assert.IsNull (val, "#A2");
		}

		[Test]
		public void SoapFormattedObject ()
		{
			ResXDataNode node = GetNodeFromResXReader (serializedResXSOAP);
			Assert.IsNotNull (node, "#A1");
			// hard coded assembly name value refers to that generated under 2.0 prefix, so use compatible available class
			object val = node.GetValue (new ReturnSerializableSubClassITRS ());
			Assert.AreEqual ("name=aname;value=avalue", val.ToString (), "#A2");
		}

		[Test]
		public void AssemblyAutomaticallyLoaded ()
		{
			// DummyAssembly must be in the same directory as current assembly to work correctly
			ResXDataNode node = GetNodeFromResXReader (anotherSerializableFromDummyAssembly);
			Assert.IsNotNull (node, "#A1");
			object value = node.GetValue ((AssemblyName []) null);
			Assert.AreEqual ("DummyAssembly.AnotherSerializable, DummyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", value.GetType ().AssemblyQualifiedName, "#A2");
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void ErrorWhenAssemblyMissing ()
		{
			ResXDataNode node = GetNodeFromResXReader (missingSerializableFromMissingAssembly);
			Assert.IsNotNull (node, "#A1");
			object val = node.GetValue ((AssemblyName []) null);
		}

		[Test]
		public void RefToSameObjectNotHeld ()
		{
			ResXDataNode node = GetNodeEmdeddedSerializable ();
			ResXDataNode returnedNode = GetNodeFromResXReader (node);
			Assert.IsNotNull (returnedNode, "#A1");
			serializable ser1 = (serializable) returnedNode.GetValue ((AssemblyName []) null);
			ser1.name = "beenchanged";
			serializable ser2 = (serializable) returnedNode.GetValue ((AssemblyName []) null);
			Assert.AreNotSame (ser1, ser2, "#A2");
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

		static string soapSerializedSerializable =
@"<SOAP-ENV:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:SOAP-ENC=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:clr=""http://schemas.microsoft.com/soap/encoding/clr/1.0"" SOAP-ENV:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
<SOAP-ENV:Body>
<a1:serializable id=""ref-1"" xmlns:a1=""http://schemas.microsoft.com/clr/nsassem/MonoTests.System.Resources/System.Windows.Forms_test_net_2_0%2C%20Version%3D0.0.0.0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Dnull"">
<sername id=""ref-3"">aname</sername>
<servalue id=""ref-4"">avalue</servalue>
</a1:serializable>
</SOAP-ENV:Body>
</SOAP-ENV:Envelope>";

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

