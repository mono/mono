// Web service test for WSDL document:
// http://appserver.pepperzak.net/bankcode/BankCodeEJBHome/wsdl.jsp

using System;
using NUnit.Framework;
using BankCodeEJBHomeTests.Soap;

namespace BankCodeEJBHomeTests
{
	[TestFixture]
	public class BankCodeEJBHomeTest: WebServiceTest
	{
		[Test]
		public void TestService ()
		{
			BankCode bc = new BankCode ();
			
			AssertEquals ("#1", "10020000  ", bc.getCodeByName ("Berlin"));
			string s = bc.getNameByCode ("10020000  ");
			AssertEquals ("#2", "10020000  ", bc.getCodeByName (s));
		}
	}
}
