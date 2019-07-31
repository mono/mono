//
// SoapServerTypeTest.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.
//
#if !MOBILE && !XAMMAC_4_5
using NUnit.Framework;

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Web.Services;
using System.Web.Services.Configuration;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MonoTests.System.Web.Services.Protocol
{
	[TestFixture]
	public class SoapServerTypeTest
	{
		[Test]
		public void NamedServiceBinding ()
		{
			SoapServerType sst = new SoapServerType (typeof (EdaInterface), WebServiceProtocols.HttpSoap);
			new ServerType (typeof (EdaInterface));

			SoapServerMethod m = sst.GetMethod ("urn:localBinding:local:LocalBindingMethod");
			Assert.IsNotNull (m, "#1");
			m = sst.GetMethod ("somethingFoo");
			Assert.IsNull (m, "#2");

			MethodInfo mi = typeof (EdaInterface).GetMethod ("BindingMethod");
			Assert.IsNotNull ("#3-1");
			m = sst.GetMethod (mi);
			// ... so, MethodInfo does not work as a key here.
			Assert.IsNull (m, "#3-2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetMethodNullKey ()
		{
			SoapServerType sst = new SoapServerType (typeof (EdaInterface), WebServiceProtocols.HttpSoap);
			sst.GetMethod (null);
		}

		[Test]
		public void ConstructorHttpGet ()
		{
			SoapServerType st = new SoapServerType (typeof (EdaInterface), WebServiceProtocols.HttpGet);
			// I wonder if this property makes sense here ...
			Assert.IsTrue (st.ServiceRoutingOnSoapAction, "#1");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void DuplicateBindingAttribute ()
		{
			new SoapServerType (typeof (DuplicateService), WebServiceProtocols.HttpSoap);
		}

		[Test]
		public void DuplicateBindingAttribute2 ()
		{
			// ServerType, not SoapServerType ... no failure.
			new ServerType (typeof (DuplicateService));
		}

		[Test]
		public void DuplicateBindingAttributeButNotInUse ()
		{
			new SoapServerType (typeof (DuplicateButUnusedService), WebServiceProtocols.AnyHttpSoap);
		}

		[Test]
		[ExpectedException (typeof (SoapException))]
		[Category ("NotWorking")]
		public void DuplicateMethodsWithSoapAction ()
		{
			new SoapServerType (typeof (WebService1), WebServiceProtocols.HttpSoap);
		}

		[Test]
		// still error because both methods have the same name (though
		// the error message seems saying that the element name 
		// conflicts with other "type" which I guess is the one built
		// from another conflicting method).
		[ExpectedException (typeof (InvalidOperationException))]
		[Category ("NotWorking")]
		public void DuplicateMethodsWithRequestElement1 ()
		{
			new SoapServerType (typeof (WebService2), WebServiceProtocols.HttpSoap);
		}

		[Test]
		[Category ("NotWorking")]
		public void DuplicateMethodsWithRequestElement2 ()
		{
			new SoapServerType (typeof (WebService3), WebServiceProtocols.HttpSoap);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void WrongNamedServiceBinding ()
		{
			new SoapServerType (typeof (WrongBindingNameClass), WebServiceProtocols.HttpSoap);
		}

		[Test]
		public void SimpleRpcType ()
		{
			new SoapServerType (typeof (SimpleRpcService), WebServiceProtocols.HttpSoap);
		}

		[WebService]
		[SoapRpcService]
		public class SimpleRpcService : WebService
		{
			[WebMethod]
			public void Hello ()
			{
			}
		}

		// bug #78953
		[WebServiceAttribute (Namespace = "www.DefaultNamespace.org")]
		[WebServiceBindingAttribute (Name = "Local", Namespace = "urn:localBinding:local")]
		[WebServiceBindingAttribute (Name = "Local2", Namespace = "urn:localBinding:local2")]
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

		[WebServiceAttribute (Namespace = "www.DefaultNamespace.org")]
		[WebServiceBindingAttribute (Name = "Duplicate", Namespace = "urn:localBinding:local")]
		[WebServiceBindingAttribute (Name = "Duplicate", Namespace = "urn:localBinding:local")]
		public class DuplicateService : WebService
		{
			[WebMethod]
			public void Test ()
			{
			}

			[WebMethod]
			[SoapDocumentMethodAttribute ("urn:localBinding:local:LocalBindingMethod",
				RequestNamespace = "urn:localBinding:local",
				Binding = "Duplicate",
				Use = SoapBindingUse.Literal, 
				ParameterStyle = SoapParameterStyle.Bare)]
			public void Foo ()
			{
			}
		}

		[WebServiceAttribute (Namespace = "www.DefaultNamespace.org")]
		[WebServiceBindingAttribute (Name = "Duplicate", Namespace = "urn:localBinding:local")]
		[WebServiceBindingAttribute (Name = "Duplicate", Namespace = "urn:localBinding:local")]
		[WebServiceBindingAttribute (Name = "Local", Namespace = "urn:localBinding:local")]
		public class DuplicateButUnusedService : WebService
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
			public void Foo ()
			{
			}
		}

		[WebService]
		public class WebService1 : WebService
		{
			[WebMethod]
			public void Test ()
			{
			}

			[WebMethod]
			[SoapDocumentMethodAttribute ("urn:foo:method1",
				RequestNamespace = "urn:foo:method1",
				Use = SoapBindingUse.Literal)]
			public void BindingMethod ()
			{
			}

			[WebMethod]
			[SoapDocumentMethodAttribute ("urn:foo:method1",
				RequestNamespace = "urn:foo:method1",
				Use = SoapBindingUse.Literal)]
			public void BindingMethod2 ()
			{
			}
		}

		[WebService]
		[SoapDocumentService (RoutingStyle = SoapServiceRoutingStyle.RequestElement)]
		public class WebService2 : WebService
		{
			[WebMethod]
			public void Test ()
			{
			}

			[WebMethod]
			[SoapDocumentMethodAttribute ("urn:foo:method1",
				RequestNamespace = "urn:foo:method1",
				RequestElementName = "Element1",
				Use = SoapBindingUse.Literal)]
			public void BindingMethod ()
			{
			}

			[WebMethod]
			[SoapDocumentMethodAttribute ("urn:foo:method1",
				RequestNamespace = "urn:foo:method1",
				RequestElementName = "Element1",
				Use = SoapBindingUse.Literal)]
			public void BindingMethod2 ()
			{
			}
		}

		[WebService]
		[SoapDocumentService (RoutingStyle = SoapServiceRoutingStyle.RequestElement)]
		public class WebService3 : WebService
		{
			[WebMethod]
			public void Test ()
			{
			}

			[WebMethod]
			[SoapDocumentMethodAttribute ("urn:foo:method1",
				RequestNamespace = "urn:foo:method1",
				RequestElementName = "Element1",
				Use = SoapBindingUse.Literal)]
			public void BindingMethod ()
			{
			}

			[WebMethod]
			[SoapDocumentMethodAttribute ("urn:foo:method1",
				RequestNamespace = "urn:foo:method1",
				RequestElementName = "Element2",
				Use = SoapBindingUse.Literal)]
			public void BindingMethod2 ()
			{
			}
		}
	}
}
#endif
