// Web service test for WSDL document:
// http://appserver.pepperzak.net/bankcode/BankCodeEJBHome/wsdl.jsp

using System;
using NUnit.Framework;
using BankCodeEJBHomeTests.Soap;

namespace External.BankCodeEJBHomeTests
{
	[TestFixture]
	public class BankCodeEJBHomeTest: WebServiceTest
	{
		[Test]
		public void TestService ()
		{
			// BEA Weblogic / RPC
			
			BankCode bc = new BankCode ();
			
			Assert.AreEqual ("10020000  ", bc.getCodeByName ("Berlin"), "#1");
			string s = bc.getNameByCode ("10020000  ");
			Assert.AreEqual ("10020000  ", bc.getCodeByName (s), "#2");
		}
	}
}
