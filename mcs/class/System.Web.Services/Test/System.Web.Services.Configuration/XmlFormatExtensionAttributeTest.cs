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
	public class XmlFormatExtensionAttributeTest {


		[Test]
		public void TestConstructors ()
		{
			XmlFormatExtensionAttribute attribute;

		/*	attribute = new XmlFormatExtensionAttribute ();
			Assertion.AssertEquals (String.Empty, attribute.ElementName);
			Assertion.AssertEquals (null, attribute.ExtensionPoints);
			Assertion.AssertEquals (String.Empty, attribute.Namespace);

			string elementName = "binding";
			string ns = "http://schemas.xmlsoap.org/wsdl/http/";
			Type[] types = new Type[4] {typeof (Binding), typeof (Binding), typeof (Binding), typeof (Binding)};

			attribute = new XmlFormatExtensionAttribute (elementName, ns, types[0]);
			Assertion.AssertEquals (elementName, attribute.ElementName);
			Assertion.AssertEquals (new Type[1] {types[0]}, attribute.ExtensionPoints);
			Assertion.AssertEquals (ns, attribute.Namespace);

			attribute = new XmlFormatExtensionAttribute (elementName, ns, types[0], types[1]);
			Assertion.AssertEquals (elementName, attribute.ElementName);
			Assertion.AssertEquals (new Type[2] {types[0], types[1]}, attribute.ExtensionPoints);
			Assertion.AssertEquals (ns, attribute.Namespace);

			attribute = new XmlFormatExtensionAttribute (elementName, ns, types[0], types[1], types[2]);
			Assertion.AssertEquals (elementName, attribute.ElementName);
			Assertion.AssertEquals (new Type[3] {types[0], types[1], types[2]}, attribute.ExtensionPoints);
			Assertion.AssertEquals (ns, attribute.Namespace);

			attribute = new XmlFormatExtensionAttribute (elementName, ns, types[0], types[1], types[2], types[3]);
			Assertion.AssertEquals (elementName, attribute.ElementName);
			Assertion.AssertEquals (new Type[4] {types[0], types[1], types[2], types[3]}, attribute.ExtensionPoints);
			Assertion.AssertEquals (ns, attribute.Namespace);

			attribute = new XmlFormatExtensionAttribute (elementName, ns, types);
			Assertion.AssertEquals (elementName, attribute.ElementName);
			Assertion.AssertEquals (types, attribute.ExtensionPoints);
			Assertion.AssertEquals (ns, attribute.Namespace);*/
		}
	}
}
