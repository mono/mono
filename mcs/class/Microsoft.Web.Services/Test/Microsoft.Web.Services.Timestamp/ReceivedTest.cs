//
// ReceivedTest.cs - NUnit Test Cases for Received
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using Microsoft.Web.Services.Timestamp;
using System;
using System.Xml;

// note: due to compiler confusion between classes and namespace (like Timestamp)
// I renamed the test namespace from "MonoTests.Microsoft.Web.Services.Timestamp"
// to "MonoTests.MS.Web.Services.Timestamp".
namespace MonoTests.MS.Web.Services.Timestamp {

	[TestFixture]
	public class ReceivedTest : Assertion {

		[Test]
		public void Constructor_Uri () 
		{
			Uri uri = new Uri ("http://www.go-mono.com/");
			Received recv = new Received (uri);
			AssertNotNull ("Received (uri)", recv);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_UriNull () 
		{
			Uri nullUri = null;
			Received recv = new Received (nullUri);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_XmlElementNull () 
		{
			XmlElement nullXml = null;
			Received recv = new Received (nullXml);
		}

		[Test]
		public void Properties () 
		{
			string actor = "http://www.go-mono.com/";
			Uri uri = new Uri (actor);
			Received recv = new Received (uri);

			AssertEquals ("Actor", actor, recv.Actor.AbsoluteUri);
			// compare longs not objects !!!
			AllTests.AssertEquals ("Delay", 0, recv.Delay);
			// we dont check default value because it's UtcNow (too late ;-)
			DateTime testDate = DateTime.FromFileTime (0xDEADC0DE); // 12/31/1600
			recv.Value = testDate;
			AssertEquals ("Value", testDate, recv.Value);
		}

		[Test]
		public void Roundtrips () 
		{
			string actor = "http://www.go-mono.com/";
			Uri uri = new Uri (actor);
			Received recv = new Received (uri);
			recv.Value = DateTime.FromFileTime (0xDEADC0DE); // 12/31/1600

			XmlDocument doc = new XmlDocument ();
			XmlElement xel = recv.GetXml (doc);
			Assertion.AssertEquals ("Xml", "<wsu:Received xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\">1600-12-31T19:06:13Z</wsu:Received>", xel.OuterXml);

			try {
				Received recv2 = new Received (xel);
				Assertion.Fail ("Expected TimestampFormatException but got none");
			}
			catch (TimestampFormatException e) {
				// not logical but as documented
				if (e.Message != TimestampFormatException.MissingActorAttributeInReceivedElement)
					Assertion.Fail ("Invalid TimestampFormatException.Message -> MissingActorAttributeInReceivedElement");
			}
			catch (Exception e) {
				Assertion.Fail ("Expected TimestampFormatException but got: " + e.ToString ());
			}

			try {
				Received recv3 = new Received (uri);
				recv3.LoadXml (xel);
				Assertion.Fail ("Expected TimestampFormatException but got none");
			}
			catch (TimestampFormatException e) {
				// not logical but as documented
				if (e.Message != TimestampFormatException.MissingActorAttributeInReceivedElement)
					Assertion.Fail ("Invalid TimestampFormatException.Message -> MissingActorAttributeInReceivedElement");
			}
			catch (Exception e) {
				Assertion.Fail ("Expected TimestampFormatException but got: " + e.ToString ());
			}
		}

		public void TestDelay () 
		{
			string actor = "http://www.go-mono.com/";
			Uri uri = new Uri (actor);
			Received recv = new Received (uri);
			recv.Delay = 60;
			recv.Value = DateTime.FromFileTime (0xDEADC0DE); // 12/31/1600

			XmlDocument doc = new XmlDocument ();
			XmlElement xel = recv.GetXml (doc);
			// Actor isn't present
			AssertEquals ("Xml", "<wsu:Received Delay=\"60\" xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\">1600-12-31T19:06:13Z</wsu:Received>", xel.OuterXml);

			string xml = "<wsu:Received Actor=\"http://www.go-mono.com/\" Delay=\"60\" xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\">1600-12-31T19:06:13Z</wsu:Received>";
			doc.LoadXml (xml);

			// FIXME: Shouldn't need this - Bug in WSE ?
			xel.SetAttribute (WSTimestamp.AttributeNames.Actor, actor);
			Received recv2 = new Received (xel);
			AssertEquals ("Actor", recv.Actor.AbsoluteUri, recv2.Actor.AbsoluteUri);
			AssertEquals ("Delay", recv.Delay, recv2.Delay);
			// compare DateTime not objects !!!
			AllTests.AssertEquals ("Value", recv.Value, recv2.Value);
			
			Received recv3 = new Received (uri);
			recv3.LoadXml (doc.DocumentElement);
			AssertEquals ("Actor", recv.Actor.AbsoluteUri, recv3.Actor.AbsoluteUri);
			AssertEquals ("Delay", recv.Delay, recv3.Delay);
			// compare DateTime not objects !!!
			AllTests.AssertEquals ("Value", recv.Value, recv3.Value);
		}
	}
}