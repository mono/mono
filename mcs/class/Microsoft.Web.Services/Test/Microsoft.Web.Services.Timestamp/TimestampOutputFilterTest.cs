//
// TimestampOutputFilterTest.cs - NUnit Test Cases for TimestampOutputFilter
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
	public class TimestampOutputFilterTest : Assertion {

		[Test]
		public void TimestampWithoutHeader () 
		{
			SoapEnvelope env = new SoapEnvelope ();
			XmlElement body = env.CreateBody ();
			env.Envelope.AppendChild (body);

			string xml = env.Envelope.OuterXml;
			Assert ("Envelope.Header", xml.IndexOf ("<soap:Header>") < 0);

			TimestampOutputFilter tsOutput = new TimestampOutputFilter ();
			tsOutput.ProcessMessage (env);
			xml = env.Envelope.OuterXml;
			Assert ("Envelope.Header", xml.IndexOf ("<soap:Header>") > 0);

			// not static - more difficult to check
			Assert ("Envelope.Timestamp.Created", xml.IndexOf ("<wsu:Created>") > 0);
			Assert ("Envelope.Timestamp.Expired", xml.IndexOf ("<wsu:Expires>") > 0);
		}

		[Test]
		public void TimestampWithHeader () 
		{
			SoapEnvelope env = new SoapEnvelope ();
			XmlElement header = env.CreateHeader ();
			env.Envelope.AppendChild (header);
			XmlElement body = env.CreateBody ();
			env.Envelope.AppendChild (body);

			TimestampOutputFilter tsOutput = new TimestampOutputFilter ();
			tsOutput.ProcessMessage (env);
			// not static - more difficult to check
			string xml = env.Envelope.OuterXml;
			Assert ("Envelope.Timestamp.Created", xml.IndexOf ("<wsu:Created>") > 0);
			Assert ("Envelope.Timestamp.Expired", xml.IndexOf ("<wsu:Expires>") > 0);
		}
	}
}