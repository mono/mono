//
// MonoTests.System.Web.Services.Configuration.XmlFormatExtensionAttributeTest.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Dave Bettin (dave@opendotnet.com)
//
// Copyright (C) Tim Coleman, 2002
// Copyright (C) Dave Bettin, 2003
//

using NUnit.Framework;
using System;
using System.Web.Services.Configuration;
using System.Web.Services.Description;

namespace MonoTests.System.Web.Services.Configuration {

	[TestFixture]
	public class XmlFormatExtensionAttributeTest : Assertion {


		[Test]
		public void TestConstructors ()
		{
			XmlFormatExtensionAttribute attribute;

		/*	attribute = new XmlFormatExtensionAttribute ();
			AssertEquals (String.Empty, attribute.ElementName);
			AssertEquals (null, attribute.ExtensionPoints);
			AssertEquals (String.Empty, attribute.Namespace);

			string elementName = "binding";
			string ns = "http://schemas.xmlsoap.org/wsdl/http/";
			Type[] types = new Type[4] {typeof (Binding), typeof (Binding), typeof (Binding), typeof (Binding)};

			attribute = new XmlFormatExtensionAttribute (elementName, ns, types[0]);
			AssertEquals (elementName, attribute.ElementName);
			AssertEquals (new Type[1] {types[0]}, attribute.ExtensionPoints);
			AssertEquals (ns, attribute.Namespace);

			attribute = new XmlFormatExtensionAttribute (elementName, ns, types[0], types[1]);
			AssertEquals (elementName, attribute.ElementName);
			AssertEquals (new Type[2] {types[0], types[1]}, attribute.ExtensionPoints);
			AssertEquals (ns, attribute.Namespace);

			attribute = new XmlFormatExtensionAttribute (elementName, ns, types[0], types[1], types[2]);
			AssertEquals (elementName, attribute.ElementName);
			AssertEquals (new Type[3] {types[0], types[1], types[2]}, attribute.ExtensionPoints);
			AssertEquals (ns, attribute.Namespace);

			attribute = new XmlFormatExtensionAttribute (elementName, ns, types[0], types[1], types[2], types[3]);
			AssertEquals (elementName, attribute.ElementName);
			AssertEquals (new Type[4] {types[0], types[1], types[2], types[3]}, attribute.ExtensionPoints);
			AssertEquals (ns, attribute.Namespace);

			attribute = new XmlFormatExtensionAttribute (elementName, ns, types);
			AssertEquals (elementName, attribute.ElementName);
			AssertEquals (types, attribute.ExtensionPoints);
			AssertEquals (ns, attribute.Namespace);*/
		}
	}
}
