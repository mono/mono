//
// SoapServerTypeTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.
//
#if NET_2_0
using NUnit.Framework;

using System;
using System.Globalization;
using System.IO;
using System.Web.Services;
using System.Web.Services.Configuration;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MonoTests.System.Web.Services.Description
{
	[TestFixture]
	public class SoapServerTypeTest
	{
		[Test]
		public void NamedServiceBinding ()
		{
			new SoapServerType (typeof (EdaInterface), WebServiceProtocols.HttpSoap);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void WrongNamedServiceBinding ()
		{
			new SoapServerType (typeof (WrongBindingNameClass), WebServiceProtocols.HttpSoap);
		}

		// bug #78953
		[WebServiceAttribute (Namespace = "www.DefaultNamespace.org")]
		[WebServiceBindingAttribute (Name = "Local", Namespace = "urn:localBinding:local")]
		public class EdaInterface : WebService
		{
			[WebMethod]
			public void Test ()
			{
			}

			[WebMethod]
			[SoapDocumentMethodAttribute ("urn:localBinding:local:LocalBindingMethod",
				RequestNamespace = "urn:localBinding:local",
				Binding = "Local",
				Use = SoapBindingUse.Literal, 
				ParameterStyle = SoapParameterStyle.Bare)]
			public void BindingMethod ()
			{
			}
		}

		[WebServiceBindingAttribute (Name = "Wrong", Namespace = "urn:localBinding:local")]
		public class WrongBindingNameClass : WebService
		{
			[WebMethod]
			public void Test ()
			{
			}

			[WebMethod]
			[SoapDocumentMethodAttribute ("urn:localBinding:local:LocalBindingMethod",
				RequestNamespace = "urn:localBinding:local",
				Binding = "Local",
				Use = SoapBindingUse.Literal, 
				ParameterStyle = SoapParameterStyle.Bare)]
			public void BindingMethod ()
			{
			}
		}
	}
}
#endif
