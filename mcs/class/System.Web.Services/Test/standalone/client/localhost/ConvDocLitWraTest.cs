// Web service test for WSDL document:
// http://localhost:8080/ConvDocLitWra.asmx?wsdl

using System;
using System.Threading;
using NUnit.Framework;
using ConvDocLitWraTests.Soap;
using System.Web.Services.Protocols;

namespace ConvDocLitWraTests
{
	[TestFixture]
	public class ConvDocLitWraTest: WebServiceTest
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
			cs.SetCurrencyRate ("EUR", 0.9);
		}
		
		// Async tests
		
		ConverterService acs;
		bool a1;
		bool a2;
		bool a3;
		AutoResetEvent eve = new AutoResetEvent (false);
		
		[Test]
		public void AsyncTestService ()
		{
			IAsyncResult ar;
			acs = new ConverterService ();
			
			ar = acs.BeginLogin ("lluis", null, null);
			acs.EndLogin (ar);
			
			acs.BeginSetCurrencyRate ("EUR", 0.5, new AsyncCallback(Callback1), null);
			
			Assert ("#0", eve.WaitOne (5000, false));
			Assert ("#1",a1);
			
			Assert ("#2", eve.WaitOne (5000, false));
			Assert ("#3",a2);
			
			Assert ("#4", eve.WaitOne (5000, false));
			Assert ("#5",a3);
		}
		
		void Callback1 (IAsyncResult ar)
		{
			acs.EndSetCurrencyRate (ar);
			acs.BeginGetCurrencyRate ("EUR", new AsyncCallback(Callback2), null);
		}
		
		void Callback2 (IAsyncResult ar)
		{
			double res = acs.EndGetCurrencyRate (ar);
			a1 = (res == 0.5);
			eve.Set ();
			
			acs.BeginConvert ("EUR","USD",6, new AsyncCallback(Callback3), null);
		}
		
		void Callback3 (IAsyncResult ar)
		{
			double res = acs.EndConvert (ar);
			a2 = (res == 12);
			eve.Set ();
			
			acs.BeginGetCurrencyInfo (new AsyncCallback(Callback4),null);
		}
		
		void Callback4 (IAsyncResult ar)
		{
			CurrencyInfo[] infos = acs.EndGetCurrencyInfo (ar);
			
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
				a3 = (val == info.Rate);
				if (!a3) break;
			}
			eve.Set ();
		}
		
		[Test]
		public void TestException ()
		{
			ConverterService cs = new ConverterService ();
			try
			{
				cs.SetCurrencyRate ("EUR", 0.5);
				Assert ("#0",false);
			}
			catch (SoapException ex)
			{
				Assert ("#1", ex.Message.IndexOf ("User not logged") != -1);
				AssertEquals ("#2", SoapException.ServerFaultCode, ex.Code);
			}
		}
		
		[Test]
		public void AsyncTestException ()
		{
			ConverterService cs = new ConverterService ();
			IAsyncResult ar = cs.BeginSetCurrencyRate ("EUR", 0.5, null, null);
			try
			{
				cs.EndSetCurrencyRate (ar);
				Assert ("#0",false);
			}
			catch (SoapException ex)
			{
				Assert ("#1", ex.Message.IndexOf ("User not logged") != -1);
				AssertEquals ("#2", SoapException.ServerFaultCode, ex.Code);
			}
		}
	}
}
