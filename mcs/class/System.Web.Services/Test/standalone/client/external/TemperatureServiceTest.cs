// Web service test for WSDL document:
// http://www.xmethods.net/sd/2001/TemperatureService.wsdl

using System;
using NUnit.Framework;
using TemperatureServiceTests.Soap;

namespace External.TemperatureServiceTests
{
	[TestFixture]
	public class TemperatureServiceTest: WebServiceTest
	{
		[Test]
		public void TestService ()
		{
			// Apache SOAP / RPC
		
			TemperatureService ts = new TemperatureService ();
			float temp = ts.getTemp ("95110");
			Assert.IsTrue (temp < 140 && temp > -60);
			
			temp = ts.getTemp ("hola");
			Assert.IsTrue (temp == -999);
		}
	}
}
