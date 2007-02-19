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

using NUnit.Framework;

using System;
using System.Globalization;
using System.IO;
using System.Web.Services;
using System.Web.Services.Description;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MonoTests.System.Web.Services.Description
{
	[TestFixture]
	public class ServiceDescriptionReflectorTest
	{
#if NET_2_0
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
#endif
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
#if NET_2_0
				"<wsdl:definitions xmlns:soap=\"http://schemas.xmlsoap.org/wsdl/soap/\" xmlns:tm=\"http://microsoft.com/wsdl/mime/textMatching/\""  +
				" xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:mime=\"http://schemas.xmlsoap.org/wsdl/mime/\"" +
				" xmlns:tns=\"http://tempuri.org/\" xmlns:s=\"http://www.w3.org/2001/XMLSchema\"" +
				" xmlns:soap12=\"http://schemas.xmlsoap.org/wsdl/soap12/\"" +
				" xmlns:http=\"http://schemas.xmlsoap.org/wsdl/http/\" targetNamespace=\"http://tempuri.org/\"" +
				" xmlns:wsdl=\"http://schemas.xmlsoap.org/wsdl/\">{0}" +
#else
				"<wsdl:definitions xmlns:http=\"http://schemas.xmlsoap.org/wsdl/http/\" xmlns:soap=\"http://schemas.xmlsoap.org/wsdl/soap/\"" +
				" xmlns:s=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\"" +
				" xmlns:tns=\"http://tempuri.org/\" xmlns:tm=\"http://microsoft.com/wsdl/mime/textMatching/\"" +
				" xmlns:mime=\"http://schemas.xmlsoap.org/wsdl/mime/\" targetNamespace=\"http://tempuri.org/\"" +
				" xmlns:wsdl=\"http://schemas.xmlsoap.org/wsdl/\">{0}" +
#endif
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
#if NET_2_0
				"    <soap:binding transport=\"http://schemas.xmlsoap.org/soap/http\" />{0}" +
#else
				"    <soap:binding transport=\"http://schemas.xmlsoap.org/soap/http\" style=\"document\" />{0}" +
#endif
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
#if NET_2_0
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
#endif
				"  <wsdl:service name=\"IncludeTestServices\">{0}" +
#if ONLY_1_1
				"    <documentation xmlns=\"http://schemas.xmlsoap.org/wsdl/\" />{0}" +
#endif
				"    <wsdl:port name=\"IncludeTestServicesSoap\" binding=\"tns:IncludeTestServicesSoap\">{0}" +
				"      <soap:address location=\"http://localhost/IncludeTestServices.asmx\" />{0}" +
				"    </wsdl:port>{0}" +
#if NET_2_0
				"    <wsdl:port name=\"IncludeTestServicesSoap12\" binding=\"tns:IncludeTestServicesSoap12\">{0}" +
				"      <soap12:address location=\"http://localhost/IncludeTestServices.asmx\" />{0}" +
				"    </wsdl:port>{0}" +
#endif
				"  </wsdl:service>{0}" +
				"</wsdl:definitions>", Environment.NewLine), sw.ToString (), "#5");
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
	}
}
