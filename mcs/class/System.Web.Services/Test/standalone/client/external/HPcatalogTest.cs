// Web service test for WSDL document:
// http://www.lixusnet.com/lixusnet/HPcatalog.jws?wsdl

using System;
using NUnit.Framework;
using HPcatalogTests.Soap;

namespace HPcatalogTests
{
	[TestFixture]
	public class HPcatalogTest: WebServiceTest
	{
		[Test]
		public void TestService ()
		{
			// AXIS / rpc
			
			HPcatalogService ser = new HPcatalogService ();
			string[][] list = ser.getList ("F2444KG");
			
			AssertNotNull (list);
			AssertEquals (121, list.Length);
			
			list = ser.getList ("D5319A");
			AssertNotNull (list);
			AssertEquals (2, list.Length);
			
			AssertNotNull (list[0]);
			AssertEquals ("8120-8382", list[0][0]);
			AssertEquals ("Power cord (Flint Gray) - 18 AWG, 1.8m (6.0ft) long - Has straight (F) receptacle (For 120V in the USA and Canada)", list[0][1]);
			
			AssertNotNull (list[1]);
			AssertEquals ("5182-8895", list[1][0]);
			AssertEquals ("Heat sink support base / retention base - For Pentium II (Klamath) processor", list[1][1]);
			
		}
	}
}
