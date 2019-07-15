//
// ServiceDescriptionReflectorTest.cs
//
// Author:
//	Gert Driesen  <drieseng@users.sourceforge.net>
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Gert Driesen
// Copyright (C) 2006 Novell, Inc.
//


#if !MOBILE && !XAMMAC_4_5
using NUnit.Framework;

using System;
using System.Globalization;
using System.IO;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MonoTests.System.Web.Services.Description
{
	[TestFixture]
	public class ServiceDescriptionReflectorTest
	{
		[Test]
		public void ReflectNullableInt ()
		{
			ServiceDescriptionReflector r =
				new ServiceDescriptionReflector ();
			r.Reflect (typeof (NullableContainer), null);
			ServiceDescription sd = r.ServiceDescriptions [0];
			XmlSchema xs = sd.Types.Schemas [0];
			XmlSchemaElement el = null;
			foreach (XmlSchemaElement e in xs.Items) {
				if (e.Name != "GetNullResponse")
					continue;
				el = e;
				break;
			}
			XmlSchemaComplexType ct =
				el.SchemaType as XmlSchemaComplexType;
			XmlSchemaSequence s = ct.Particle as XmlSchemaSequence;
			XmlSchemaElement e2 = s.Items [0] as XmlSchemaElement;
			Assert.IsTrue (e2.IsNillable);
		}
		[Test]
		[Category ("NotWorking")]
		public void IncludeTest ()
		{
			ServiceDescriptionReflector reflector = new ServiceDescriptionReflector ();
			reflector.Reflect (typeof (IncludeTestServices), "http://localhost/IncludeTestServices.asmx");

			Assert.AreEqual (0, reflector.Schemas.Count, "#1");
			Assert.AreEqual (1, reflector.ServiceDescriptions.Count, "#2");

			ServiceDescription sd = reflector.ServiceDescriptions[0];

			Assert.IsNull (sd.Name, "#3");
			Assert.AreEqual (1, sd.Types.Schemas.Count, "#4");

			StringWriter sw = new StringWriter ();
			sd.Write (sw);

			Assert.AreEqual (string.Format(CultureInfo.InvariantCulture,
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>{0}" +
				"<wsdl:definitions xmlns:soap=\"http://schemas.xmlsoap.org/wsdl/soap/\" xmlns:tm=\"http://microsoft.com/wsdl/mime/textMatching/\""  +
				" xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:mime=\"http://schemas.xmlsoap.org/wsdl/mime/\"" +
				" xmlns:tns=\"http://tempuri.org/\" xmlns:s=\"http://www.w3.org/2001/XMLSchema\"" +
				" xmlns:soap12=\"http://schemas.xmlsoap.org/wsdl/soap12/\"" +
				" xmlns:http=\"http://schemas.xmlsoap.org/wsdl/http/\" targetNamespace=\"http://tempuri.org/\"" +
				" xmlns:wsdl=\"http://schemas.xmlsoap.org/wsdl/\">{0}" +
				"  <wsdl:types>{0}" +
				"    <s:schema elementFormDefault=\"qualified\" targetNamespace=\"http://tempuri.org/\">{0}" +
				"      <s:element name=\"EchoString\">{0}" +
				"        <s:complexType>{0}" +
				"          <s:sequence>{0}" +
				"            <s:element minOccurs=\"0\" maxOccurs=\"1\" name=\"strval\" type=\"s:string\" />{0}" +
				"          </s:sequence>{0}" +
				"        </s:complexType>{0}" +
				"      </s:element>{0}" +
				"      <s:element name=\"EchoStringResponse\">{0}" +
				"        <s:complexType>{0}" +
				"          <s:sequence>{0}" +
				"            <s:element minOccurs=\"1\" maxOccurs=\"1\" name=\"MyTime\" type=\"s:time\" />{0}" +
				"          </s:sequence>{0}" +
				"        </s:complexType>{0}" +
				"      </s:element>{0}" +
				"      <s:element name=\"Vehicle\">{0}" +
				"        <s:complexType>{0}" +
				"          <s:sequence>{0}" +
				"            <s:element minOccurs=\"0\" maxOccurs=\"1\" name=\"licenseNumber\" type=\"s:string\" />{0}" +
				"          </s:sequence>{0}" +
				"        </s:complexType>{0}" +
				"      </s:element>{0}" +
				"      <s:element name=\"VehicleResponse\">{0}" +
				"        <s:complexType>{0}" +
				"          <s:sequence>{0}" +
				"            <s:element minOccurs=\"1\" maxOccurs=\"1\" name=\"NewVehicle\" nillable=\"true\" type=\"tns:Vehicle\" />{0}" +
				"          </s:sequence>{0}" +
				"        </s:complexType>{0}" +
				"      </s:element>{0}" +
				"      <s:complexType name=\"Vehicle\" abstract=\"true\">{0}" +
				"        <s:sequence>{0}" +
				"          <s:element minOccurs=\"0\" maxOccurs=\"1\" name=\"licenseNumber\" type=\"s:string\" />{0}" +
				"          <s:element minOccurs=\"1\" maxOccurs=\"1\" name=\"make\" type=\"s:dateTime\" />{0}" +
				"          <s:element minOccurs=\"1\" maxOccurs=\"1\" name=\"age\" type=\"tns:TimeSpan\" />{0}" +
				"        </s:sequence>{0}" +
				"      </s:complexType>{0}" +
				"      <s:complexType name=\"TimeSpan\" />{0}" +
				"      <s:complexType name=\"Car\">{0}" +
				"        <s:complexContent mixed=\"false\">{0}" +
				"          <s:extension base=\"tns:Vehicle\" />{0}" +
				"        </s:complexContent>{0}" +
				"      </s:complexType>{0}" +
				"    </s:schema>{0}" +
				"  </wsdl:types>{0}" +
				"  <wsdl:message name=\"EchoStringSoapIn\">{0}" +
				"    <wsdl:part name=\"parameters\" element=\"tns:EchoString\" />{0}" +
				"  </wsdl:message>{0}" +
				"  <wsdl:message name=\"EchoStringSoapOut\">{0}" +
				"    <wsdl:part name=\"parameters\" element=\"tns:EchoStringResponse\" />{0}" +
				"  </wsdl:message>{0}" +
				"  <wsdl:message name=\"VehicleSoapIn\">{0}" +
				"    <wsdl:part name=\"parameters\" element=\"tns:Vehicle\" />{0}" +
				"  </wsdl:message>{0}" +
				"  <wsdl:message name=\"VehicleSoapOut\">{0}" +
				"    <wsdl:part name=\"parameters\" element=\"tns:VehicleResponse\" />{0}" +
				"  </wsdl:message>{0}" +
				"  <wsdl:portType name=\"IncludeTestServicesSoap\">{0}" +
				"    <wsdl:operation name=\"EchoString\">{0}" +
				"      <wsdl:input message=\"tns:EchoStringSoapIn\" />{0}" +
				"      <wsdl:output message=\"tns:EchoStringSoapOut\" />{0}" +
				"    </wsdl:operation>{0}" +
				"    <wsdl:operation name=\"Vehicle\">{0}" +
				"      <wsdl:input message=\"tns:VehicleSoapIn\" />{0}" +
				"      <wsdl:output message=\"tns:VehicleSoapOut\" />{0}" +
				"    </wsdl:operation>{0}" +
				"  </wsdl:portType>{0}" +
				"  <wsdl:binding name=\"IncludeTestServicesSoap\" type=\"tns:IncludeTestServicesSoap\">{0}" +
				"    <soap:binding transport=\"http://schemas.xmlsoap.org/soap/http\" />{0}" +
				"    <wsdl:operation name=\"EchoString\">{0}" +
				"      <soap:operation soapAction=\"http://tempuri.org/EchoString\" style=\"document\" />{0}" +
				"      <wsdl:input>{0}" +
				"        <soap:body use=\"literal\" />{0}" +
				"      </wsdl:input>{0}" +
				"      <wsdl:output>{0}" +
				"        <soap:body use=\"literal\" />{0}" +
				"      </wsdl:output>{0}" +
				"    </wsdl:operation>{0}" +
				"    <wsdl:operation name=\"Vehicle\">{0}" +
				"      <soap:operation soapAction=\"http://tempuri.org/Vehicle\" style=\"document\" />{0}" +
				"      <wsdl:input>{0}" +
				"        <soap:body use=\"literal\" />{0}" +
				"      </wsdl:input>{0}" +
				"      <wsdl:output>{0}" +
				"        <soap:body use=\"literal\" />{0}" +
				"      </wsdl:output>{0}" +
				"    </wsdl:operation>{0}" +
				"  </wsdl:binding>{0}" +
				"  <wsdl:binding name=\"IncludeTestServicesSoap12\" type=\"tns:IncludeTestServicesSoap\">{0}" +
				"    <soap12:binding transport=\"http://schemas.xmlsoap.org/soap/http\" />{0}" +
				"    <wsdl:operation name=\"EchoString\">{0}" +
				"      <soap12:operation soapAction=\"http://tempuri.org/EchoString\" style=\"document\" />{0}" +
				"      <wsdl:input>{0}" +
				"        <soap12:body use=\"literal\" />{0}" +
				"      </wsdl:input>{0}" +
				"      <wsdl:output>{0}" +
				"        <soap12:body use=\"literal\" />{0}" +
				"      </wsdl:output>{0}" +
				"    </wsdl:operation>{0}" +
				"    <wsdl:operation name=\"Vehicle\">{0}" +
				"      <soap12:operation soapAction=\"http://tempuri.org/Vehicle\" style=\"document\" />{0}" +
				"      <wsdl:input>{0}" +
				"        <soap12:body use=\"literal\" />{0}" +
				"      </wsdl:input>{0}" +
				"      <wsdl:output>{0}" +
				"        <soap12:body use=\"literal\" />{0}" +
				"      </wsdl:output>{0}" +
				"    </wsdl:operation>{0}" +
				"  </wsdl:binding>{0}" +
				"  <wsdl:service name=\"IncludeTestServices\">{0}" +
				"    <wsdl:port name=\"IncludeTestServicesSoap\" binding=\"tns:IncludeTestServicesSoap\">{0}" +
				"      <soap:address location=\"http://localhost/IncludeTestServices.asmx\" />{0}" +
				"    </wsdl:port>{0}" +
				"    <wsdl:port name=\"IncludeTestServicesSoap12\" binding=\"tns:IncludeTestServicesSoap12\">{0}" +
				"      <soap12:address location=\"http://localhost/IncludeTestServices.asmx\" />{0}" +
				"    </wsdl:port>{0}" +
				"  </wsdl:service>{0}" +
				"</wsdl:definitions>", Environment.NewLine), sw.ToString (), "#5");
		}

		[Test]
		[Category ("NotWorking")]
		public void ReflectTypeNonDefaultBinding ()
		{
			// bug #78953
			ServiceDescriptionReflector r =
				new ServiceDescriptionReflector ();
			r.Reflect (typeof (EdaInterface), "urn:foo");
//foreach (ServiceDescription sss in r.ServiceDescriptions) sss.Write (Console.Out);
			// It should create two wsdls, one for www.DefaultNamespace.org and
			// another for urn:localBinding:local .
			Assert.AreEqual (2, r.ServiceDescriptions.Count, "#1");
			Assert.IsNotNull (r.ServiceDescriptions ["www.DefaultNamespace.org"], "#1-1");
			ServiceDescription sd = r.ServiceDescriptions ["urn:localBinding:local"];
			Assert.IsNotNull (sd, "#1-2");
			// Soap and Soap12
			Assert.AreEqual (2, sd.Bindings.Count, "#2-2.0");
			Binding b = sd.Bindings [0];
			Assert.AreEqual ("Local", b.Name, "#3");
		}

		[Test]
		public void Bug79087 ()
		{
			ServiceDescriptionReflector r =
				new ServiceDescriptionReflector ();
			r.Reflect (typeof (Bug79807Service), "urn:foo");
			StringWriter sw = new StringWriter ();
			r.ServiceDescriptions [0].Write (sw);
			ServiceDescription.Read (new StringReader (sw.ToString ()));
		}

		[Test]
		public void EmptyAction ()
		{
			ServiceDescriptionReflector r =
				new ServiceDescriptionReflector ();
			r.Reflect (typeof (EmptyActionService), "urn:foo");
			Binding b = r.ServiceDescriptions [0].Bindings ["EmptyActionServiceSoap"];
			OperationBinding o = b.Operations [0];
			SoapOperationBinding sob = o.Extensions [0] as SoapOperationBinding;
			Assert.AreEqual (String.Empty, sob.SoapAction);
		}

		[Test]
		public void Bug332150 ()
		{
			ServiceDescriptionReflector r =
				new ServiceDescriptionReflector ();
			r.Reflect (typeof (Bug332150Service), "urn:foo");
			StringWriter sw = new StringWriter ();
			r.ServiceDescriptions [0].Write (sw);
			ServiceDescription.Read (new StringReader (sw.ToString ()));
		}

		[Test]
		public void Bug345448 ()
		{
			ServiceDescriptionReflector r =
				new ServiceDescriptionReflector ();
			r.Reflect (typeof (Bug345448Service), "urn:foo");

			ServiceDescription sd = r.ServiceDescriptions [0];

			Assert.AreEqual("Bug345448ServiceSoap", sd.Bindings [0].Name, "sd #1");
			Assert.AreEqual("Bug345448ServiceSoap12", sd.Bindings [1].Name, "sd #2");
		}

		[Test]
		public void Bug345449 ()
		{
			ServiceDescriptionReflector r =
				new ServiceDescriptionReflector ();
			r.Reflect (typeof (Bug345448Service), "urn:foo");
			ServiceDescription sd = r.ServiceDescriptions [0];

			Assert.AreEqual("Bug345448ServiceSoap", sd.Services [0].Ports [0].Name, "sd #3");
			Assert.AreEqual("Bug345448ServiceSoap12", sd.Services [0].Ports [1].Name, "sd #4");
		}

		[Test]
		public void Bug360241 ()
		{
			// Make sure the map for service client is properly created
			new Bug360241SoapHttpClientProtocol ();
		}

		public class IncludeTestServices : WebService
		{
			[WebMethod ()]
			[return: XmlElement ("MyTime", DataType = "time")]
			public DateTime EchoString ([XmlElement (DataType = "string")] string strval)
			{
				return DateTime.Now;
			}

			[WebMethod ()]
			[XmlInclude (typeof (Car))]
			public Vehicle Vehicle (string licenseNumber)
			{
				if (licenseNumber == "0") {
					Vehicle v = new Car ();
					v.licenseNumber = licenseNumber;
					return v;
				} else {
					return null;
				}
			}
		}
		[XmlRoot ("NewVehicle")]
		public abstract class Vehicle
		{
			public string licenseNumber;
			public DateTime make;
			public TimeSpan age;
		}

		public class Car : Vehicle
		{
		}

		public class NullableContainer
		{
			[WebMethod (Description="Test nullables")]
			public int? GetNull ()
			{
				return null;
			}
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
			public void Test2 ()
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

		// bug #79807
		public class Bug79807Item
		{
			public string stringOne;
			public string stringTwo;
		}

		public class Bug79807AnotherItem
		{
			public string stringOne;
			public string stringTwo;
		}

		[WebService]
		[SoapRpcService]
		public class Bug79807Service : WebService
		{
			[WebMethod]
			public Bug79807Item [] Method1 (int count)
			{
				Bug79807Item [] arr = new Bug79807Item [count];
				for (int i = 0;i < count;i++) {
					arr [i].stringOne = "one";
					arr [i].stringTwo = "two";
				}
				return arr;
			}

			[WebMethod]
			public Bug79807AnotherItem [] Method2 (int count)
			{
				Bug79807AnotherItem [] arr = new Bug79807AnotherItem [count];
				for (int i = 0;i < count;i++) {
					arr [i].stringOne = "one";
					arr [i].stringTwo = "two";
				}
				return arr;
			}
		}

		[WebService (Namespace = "http://tempuri.org/")]
		public class EmptyActionService : WebService
		{
			[WebMethod]
			[SoapDocumentMethod ("")]
			public string HelloWorld () {
				return "Hello World";
			}
		}

		[WebService (Namespace = "http://tempuri.org/")]
		[WebServiceBinding (ConformsTo = WsiProfiles.BasicProfile1_1)]
		public abstract class Bug332150SecureWebService : WebService
		{
			public Bug332150SecureWebService ()
			{ 
			}

			[WebMethod]
			public bool Login (string userName, string password)
			{
				return true;
			}
		}

		[WebService (Namespace = "http://tempuri.org/")]
		[WebServiceBinding (ConformsTo = WsiProfiles.BasicProfile1_1)]
		public class Bug332150Service : Bug332150SecureWebService
		{
			public Bug332150Service ()
			{
			}

			[WebMethod]
			public string HelloWorld ()
			{
				return "Hello World";
			}
		}

		[WebService (Namespace = "http://tempuri.org/")]
		[WebServiceBindingAttribute (Name = "AnotherBinding", Namespace = "http://tempuri.org/")]
		public class Bug345448Service : WebService
		{
			[WebMethod]
			//[SoapDocumentMethodAttribute (Binding="AnotherBinding")]
			public string HelloWorld ()
			{
				return "Hello World";
			}
		}

		[WebServiceBindingAttribute (Name = "AnotherBinding", Namespace = "http://tempuri.org/")]
		public class Bug360241SoapHttpClientProtocol : SoapHttpClientProtocol
		{
		}
	}
}

#endif
