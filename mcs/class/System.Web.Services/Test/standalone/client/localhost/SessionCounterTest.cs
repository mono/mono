// Web service test for WSDL document:
// http://localhost:8080/SessionCounter.asmx?wsdl

using System;
using System.Net;
using NUnit.Framework;
using SessionCounterTests.Soap;

namespace Localhost.SessionCounterTests
{
	[TestFixture]
	public class SessionCounterTest: WebServiceTest
	{
		[Test]
		public void TestService ()
		{
			SessionCounter s = new SessionCounter ();
			s.CookieContainer = new CookieContainer ();
			s.Reset ();
			for (int n=1; n<10; n++)
				Assert.AreEqual (n, s.AddOne (), "t"+n);
		}
	}
}
