// Web service test for WSDL document:
// http://www.lixusnet.com/lixusnet/HPcatalog.jws?wsdl

using System;
using NUnit.Framework;
using HPcatalogTests.Soap;

namespace External.HPcatalogTests
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
			
			Assert.IsNotNull (list);
			Assert.AreEqual (121, list.Length);
			
			list = ser.getList ("D5319A");
			Assert.IsNotNull (list);
			Assert.AreEqual (2, list.Length);
			
			Assert.IsNotNull (list[0]);
			Assert.AreEqual ("8120-8382", list[0][0]);
			Assert.AreEqual ("Power cord (Flint Gray) - 18 AWG, 1.8m (6.0ft) long - Has straight (F) receptacle (For 120V in the USA and Canada)", list[0][1]);
			
			Assert.IsNotNull (list[1]);
			Assert.AreEqual ("5182-8895", list[1][0]);
			Assert.AreEqual ("Heat sink support base / retention base - For Pentium II (Klamath) processor", list[1][1]);
			
		}
	}
}
