// Web service test for WSDL document:
// http://localhost:8080/ConvDocLitBar.asmx?wsdl

using System;
using NUnit.Framework;
using ConvDocLitBarTests.Soap;

namespace ConvDocLitBarTests
{
	[TestFixture]
	public class ConvDocLitBarTest: WebServiceTest
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
	}
}
