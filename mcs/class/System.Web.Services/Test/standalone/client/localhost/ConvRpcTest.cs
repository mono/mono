// Web service test for WSDL document:
// http://localhost:8080/ConvRpc.asmx?wsdl

using System;
using System.Web.Services;
using System.Xml.Serialization;
using System.Xml;
using NUnit.Framework;
using ConvRpcTests.Soap;

namespace Localhost.ConvRpcTests
{
	[TestFixture]
	public class ConvRpcTest: WebServiceTest
	{
		[Test]
		public void TestService ()
		{
			ConverterService cs = new ConverterService ();
			cs.Login ("lluis");
			cs.SetCurrencyRate ("EUR", 0.5);
			AssertEquals ("#1", 0.5, cs.GetCurrencyRate ("EUR"));
			
			double res = cs.Convert ("EUR","USD",6);
			AssertEquals ("#2", (int)res, (int)12);
			
			CurrencyInfo[] infos = cs.GetCurrencyInfo ();
			foreach (CurrencyInfo info in infos)
			{
				double val = 0;
				switch (info.Name)
				{
					case "USD": val = 1; break;
					case "EUR": val = 0.5; break;
					case "GBP": val = 0.611817; break;
					case "JPY": val = 118.271; break;
					case "CAD": val = 1.36338; break;
					case "AUD": val = 1.51485; break;
					case "CHF": val = 1.36915; break;
					case "RUR": val = 30.4300; break;
					case "CNY": val = 8.27740; break;
					case "ZAR": val = 7.62645; break;
					case "MXN": val = 10.5025; break;
				}
				AssertEquals ("#3 " + info.Name, val, info.Rate);
			}
		}
		
		[Test]
		public void TestObjectReturn ()
		{
			ConverterServiceExtraTest et = new ConverterServiceExtraTest ();
			string d;
			object res = et.GetTestInfo ("hi", out d);
			
			AssertEquals ("t1", "iii", d);
			AssertNotNull ("t2", res);
			Assert ("t3", res is XmlNode[]);
			XmlNode[] nods = res as XmlNode[];
			AssertEquals ("t4", 5, nods.Length);
			
			Assert ("t5", nods[0] is XmlAttribute);
			XmlAttribute at = nods[0] as XmlAttribute;
			AssertEquals ("t6", "id", at.LocalName);
			
			Assert ("t7", nods[1] is XmlAttribute);
			at = nods[1] as XmlAttribute;
			AssertEquals ("t8", "type", at.LocalName);
			
			Assert ("t9", nods[2] is XmlAttribute);
			at = nods[2] as XmlAttribute;
			
			Assert ("t10", nods[3] is XmlElement);
			XmlElement el = nods[3] as XmlElement;
			AssertEquals ("t11", "a", el.Name);
			
			Assert ("t12", nods[4] is XmlElement);
			el = nods[4] as XmlElement;
			AssertEquals ("t13", "b", el.Name);
		}
	}
	
	[System.Web.Services.WebServiceBindingAttribute(Name="ConverterServiceSoap", Namespace="urn:mono-ws-tests")]
	public class ConverterServiceExtraTest : System.Web.Services.Protocols.SoapHttpClientProtocol
	{
		public ConverterServiceExtraTest() {
			this.Url = "http://192.168.1.3:8080/ConvRpc.asmx";
		}
	
		[System.Web.Services.Protocols.SoapRpcMethodAttribute("urn:mono-ws-tests/GetTestInfo", RequestNamespace="urn:mono-ws-tests", ResponseNamespace="urn:mono-ws-tests")]
		public object GetTestInfo(string s, out string d) {
			object[] results = this.Invoke("GetTestInfo", new object[] {s});
			d = (string) results[1];
	        return ((object)(results[0]));
		}
	}
}
