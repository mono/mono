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
			Assert.AreEqual (String.Empty, attribute.ElementName);
			Assert.AreEqual (null, attribute.ExtensionPoints);
			Assert.AreEqual (String.Empty, attribute.Namespace);

			string elementName = "binding";
			string ns = "http://schemas.xmlsoap.org/wsdl/http/";
			Type[] types = new Type[4] {typeof (Binding), typeof (Binding), typeof (Binding), typeof (Binding)};

			attribute = new XmlFormatExtensionAttribute (elementName, ns, types[0]);
			Assert.AreEqual (elementName, attribute.ElementName);
			Assert.AreEqual (new Type[1] {types[0]}, attribute.ExtensionPoints);
			Assert.AreEqual (ns, attribute.Namespace);

			attribute = new XmlFormatExtensionAttribute (elementName, ns, types[0], types[1]);
			Assert.AreEqual (elementName, attribute.ElementName);
			Assert.AreEqual (types[1]}, attribute.ExtensionPoints, new Type[2] {types[0]);
			Assert.AreEqual (ns, attribute.Namespace);

			attribute = new XmlFormatExtensionAttribute (elementName, ns, types[0], types[1], types[2]);
			Assert.AreEqual (elementName, attribute.ElementName);
			Assert.AreEqual (types[1], types[2]}, attribute.ExtensionPoints, new Type[3] {types[0]);
			Assert.AreEqual (ns, attribute.Namespace);

			attribute = new XmlFormatExtensionAttribute (elementName, ns, types[0], types[1], types[2], types[3]);
			Assert.AreEqual (elementName, attribute.ElementName);
			Assert.AreEqual (types[1], types[2], types[3]}, attribute.ExtensionPoints, new Type[4] {types[0]);
			Assert.AreEqual (ns, attribute.Namespace);

			attribute = new XmlFormatExtensionAttribute (elementName, ns, types);
			Assert.AreEqual (elementName, attribute.ElementName);
			Assert.AreEqual (types, attribute.ExtensionPoints);
			Assert.AreEqual (ns, attribute.Namespace);*/
		}
	}
}
