//
// MonoTests.System.Web.Services.Configuration.XmlFormatExtensionAttributeTest.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using NUnit.Framework;
using System;
using System.Web.Services.Configuration;
using System.Web.Services.Description;

namespace MonoTests.System.Web.Services.Configuration {

	public class XmlFormatExtensionAttributeTest : TestCase {

		public XmlFormatExtensionAttributeTest () :
			base ("[MonoTests.System.Web.Services.Configuration.XmlFormatExtensionAttributeTest]") 
		{
		}

		public XmlFormatExtensionAttributeTest (string name) :
			base (name) 
		{
		}

		protected override void SetUp ()
		{
		}

		protected override void TearDown ()
		{
		}

		public static ITest Suite {
			get { return new TestSuite (typeof (XmlFormatExtensionAttributeTest)); }
		}

		public void TestConstructors ()
		{
			XmlFormatExtensionAttribute attribute;

			attribute = new XmlFormatExtensionAttribute ();
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
			AssertEquals (ns, attribute.Namespace);
		}
	}
}
