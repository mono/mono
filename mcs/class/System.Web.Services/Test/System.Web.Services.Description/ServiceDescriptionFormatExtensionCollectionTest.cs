//
// ServiceDescriptionFormatExtensionCollectionTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C) 2006 Novell, Inc.
//

using NUnit.Framework;

using System;
using System.Web.Services.Description;
using System.Xml;

namespace MonoTests.System.Web.Services.Description
{
	[TestFixture]
	public class ServiceDescriptionFormatExtensionCollectionTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Add ()
		{
			ServiceDescriptionFormatExtensionCollection c =
				new ServiceDescriptionFormatExtensionCollection (new ServiceDescription ());

			c.Add (0);
		}

		[Test]
		public void Add2 ()
		{
			ServiceDescriptionFormatExtensionCollection c =
				new ServiceDescriptionFormatExtensionCollection (new ServiceDescription ());

			c.Add (new XmlDocument ().CreateElement ("foo"));
		}

		class MySoapBinding : SoapBinding
		{
		}

		[Test]
		public void Find ()
		{
			ServiceDescriptionFormatExtensionCollection c =
				new ServiceDescriptionFormatExtensionCollection (new ServiceDescription ());
			c.Add (new MySoapBinding ());
			Assert.IsNotNull (c.Find (typeof (SoapBinding)));
		}
	}
}
