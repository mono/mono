//
// TimestampInputFilterTest.cs - NUnit Test Cases for TimestampInputFilter
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using Microsoft.Web.Services;
using Microsoft.Web.Services.Timestamp;
using System;
using System.Web.Services.Protocols;
using System.Xml;

// note: due to compiler confusion between classes and namespace (like Timestamp)
// I renamed the test namespace from "MonoTests.Microsoft.Web.Services.Timestamp"
// to "MonoTests.MS.Web.Services.Timestamp".
namespace MonoTests.MS.Web.Services.Timestamp {

	// Reference:
	// 1.	Inside the Web Services Enhancements Pipeline
	//	http://msdn.microsoft.com/library/en-us/dnwebsrv/html/insidewsepipe.asp

	[TestFixture]
	public class TimestampInputFilterTest : Assertion {

		static string soapMinimal = "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body /></soap:Envelope>";
		static string soapExpiredEnvelope = "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Header><wsu:Timestamp xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\"><wsu:Created>2002-11-14T19:03:27Z</wsu:Created><wsu:Expires>2002-11-14T19:08:27Z</wsu:Expires></wsu:Timestamp></soap:Header><soap:Body /></soap:Envelope>";
		static string soapTimestampEnvelope = "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Header><wsu:Timestamp xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\"><wsu:Created>2002-11-14T19:03:27Z</wsu:Created><wsu:Expires>2020-11-14T19:08:27Z</wsu:Expires></wsu:Timestamp></soap:Header><soap:Body /></soap:Envelope>";

		[Test]
		public void NoTimestamp () 
		{
			SoapEnvelope env = new SoapEnvelope ();
			env.LoadXml (soapMinimal);

			TimestampInputFilter tsInput = new TimestampInputFilter ();
			tsInput.ProcessMessage (env);
			AssertEquals ("Envelope", soapMinimal, env.Envelope.OuterXml);
			AllTests.AssertEquals ("Created", DateTime.MinValue, env.Context.Timestamp.Created);
			AllTests.AssertEquals ("Expires", DateTime.MaxValue, env.Context.Timestamp.Expires);
		}

		[Test]
		public void ExpiredMessage () 
		{
			SoapEnvelope env = new SoapEnvelope ();
			env.LoadXml (soapExpiredEnvelope);

			TimestampInputFilter tsInput = new TimestampInputFilter ();
			try {
				tsInput.ProcessMessage (env);
				Fail ("Expected TimestampFault but got none");
			}
			catch (SoapHeaderException she) {
				// TimestampFault isn't public so we catch it's ancestor
				if (she.ToString ().StartsWith ("Microsoft.Web.Services.Timestamp.TimestampFault")) {
					// this is expected
				}
				else
					Fail ("Expected TimestampFault but got " + she.ToString ());
			}
			catch (Exception e) {
				Fail ("Expected TimestampFault but got " + e.ToString ());
			}
		}

		[Test]
		public void ValidMessage () 
		{
			SoapEnvelope env = new SoapEnvelope ();
			// valid until 2020
			env.LoadXml (soapTimestampEnvelope);

			TimestampInputFilter tsInput = new TimestampInputFilter ();
			tsInput.ProcessMessage (env);
			AssertEquals ("Envelope", "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Header></soap:Header><soap:Body /></soap:Envelope>", env.Envelope.OuterXml);
			AssertEquals ("Created", "2002-11-14T19:03:27Z", env.Context.Timestamp.Created.ToString (WSTimestamp.TimeFormat));
			AssertEquals ("Expires", "2020-11-14T19:08:27Z", env.Context.Timestamp.Expires.ToString (WSTimestamp.TimeFormat));
		}
	}
}