//
// TimestampTest.cs - NUnit Test Cases for Timestamp
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
	public class TimestampTest : Assertion {

		// compiler conflict between namespace Timestamp and class Timestamp
		private Microsoft.Web.Services.Timestamp.Timestamp ts;

		private static string empty = "<wsu:Timestamp xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\" />";
		private static string test1 = "<wsu:Timestamp xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\"><wsu:Created>2001-09-13T08:42:00Z</wsu:Created><wsu:Expires>2001-10-13T09:00:00Z</wsu:Expires></wsu:Timestamp>";
		private static string test2 = "<wsu:Timestamp xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\"><wsu:Created>2001-09-13T08:42:00Z</wsu:Created><wsu:Expires>2001-10-13T09:00:00Z</wsu:Expires><wsu:Received Actor=\"http://x.com/\" Delay=\"60000\">2001-09-13T08:44:00Z</wsu:Received></wsu:Timestamp>";
		private static string test3 = "<wsu:Timestamp xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\"><wsu:Created wsu:Id=\"createdId\">2001-09-13T08:42:00Z</wsu:Created><wsu:Expires wsu:Id=\"expiresId\">2001-10-13T09:00:00Z</wsu:Expires><wsu:Received Actor=\"http://x.com/\" Delay=\"60000\">2001-09-13T08:44:00Z</wsu:Received></wsu:Timestamp>";
		private static string test4 = "<wsu:Timestamp wsu:Id=\"timestampId\" xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\"><wsu:Created wsu:Id=\"createdId\">2001-09-13T08:42:00Z</wsu:Created><wsu:Expires wsu:Id=\"expiresId\">2001-10-13T09:00:00Z</wsu:Expires><wsu:Received Actor=\"http://x.com/\" Delay=\"60000\" wsu:Id=\"receivedId\">2001-09-13T08:44:00Z</wsu:Received></wsu:Timestamp>";

		[SetUp]
		void SetUp () 
		{
			ts = new Microsoft.Web.Services.Timestamp.Timestamp ();
		}

		[Test]
		public void Properties () 
		{
			// test values
			DateTime testTime = DateTime.UtcNow;
			long testTTL = 1;
			// default values
			AssertEquals ("Created (default)", DateTime.MinValue, ts.Created);
			AssertEquals ("Expires (default)", DateTime.MaxValue, ts.Expires);
			AllTests.AssertEquals ("TTL (default)", 300000, ts.Ttl);
			// new ttl values
			ts.Ttl = testTTL;
			AllTests.AssertEquals ("TTL (test)", 1, ts.Ttl);
			ts.Ttl = 0;
			AllTests.AssertEquals ("TTL (0)", 0, ts.Ttl);
			AssertEquals ("Expires (0)", DateTime.MaxValue, ts.Expires);
			AssertNotNull ("Receivers", ts.Receivers);
			AssertEquals ("Receivers.Count", 0, ts.Receivers.Count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Ttl_Negative () 
		{
			ts.Ttl = -1;
		}

		[Test]
		[ExpectedException (typeof (TimestampFormatException))]
		public void CheckInvalid () 
		{
			// default object is incomplete
			ts.CheckValid ();
		}

		// Note: All valid (no exception are thrown) but all EXPIRED !
		[Test]
		public void CheckValid () 
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test1);
			ts.LoadXml (doc.DocumentElement);
			ts.CheckValid ();

			doc.LoadXml (test2);
			ts.LoadXml (doc.DocumentElement);
			ts.CheckValid ();

			doc.LoadXml (test3);
			ts.LoadXml (doc.DocumentElement);
			ts.CheckValid ();

			doc.LoadXml (test4);
			ts.LoadXml (doc.DocumentElement);
			ts.CheckValid ();
		}

		[Test]
		public void GetXml () 
		{
			XmlDocument doc = new XmlDocument ();
			XmlElement xel = ts.GetXml (doc);
			// minimal XML
			AssertEquals ("Xml", empty, xel.OuterXml);

			ts.Ttl = 60000; // one minute
			xel = ts.GetXml (doc);
			// TTL has no change in XML
			AssertEquals ("Xml", empty, xel.OuterXml);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void LoadXml_Null () 
		{
			ts.LoadXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void LoadXml_BadElement () 
		{
			XmlDocument doc = new XmlDocument ();
			// bad element (Timestamp case is invalid)
			doc.LoadXml ("<wsu:timeStamp xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2002/07/utility\" />");
			ts.LoadXml (doc.DocumentElement);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void LoadXml_BadNamespace () 
		{
			XmlDocument doc = new XmlDocument ();
			// bad namespace (invalid year in URL)
			doc.LoadXml ("<wsu:Timestamp xmlns:wsu=\"http://schemas.xmlsoap.org/ws/2003/07/utility\" />");
			ts.LoadXml (doc.DocumentElement);
		}

		// sample taken from http://msdn.microsoft.com/library/en-us/dnglobspec/html/ws-security.asp
		[Test]
		public void LoadXml () 
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test1);
			ts.LoadXml (doc.DocumentElement);

			XmlElement xel = ts.GetXml (doc);
			AssertEquals ("Xml", test1, xel.OuterXml);
			AssertEquals ("Xml Actor", "", ts.Actor);
			AssertEquals ("Xml Created", "2001-09-13T08:42:00Z", ts.Created.ToString (WSTimestamp.TimeFormat));
			AssertEquals ("Xml Expires", "2001-10-13T09:00:00Z", ts.Expires.ToString (WSTimestamp.TimeFormat));
			AssertEquals ("Xml Receivers.Count", 0, ts.Receivers.Count);
			AllTests.AssertEquals ("Xml Ttl", 300000, ts.Ttl);
		}

		// sample taken from http://msdn.microsoft.com/library/en-us/dnglobspec/html/ws-security.asp
		[Test]
		public void LoadXmlWithOneIntermediary () 
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test2);
			ts.LoadXml (doc.DocumentElement);

			XmlElement xel = ts.GetXml (doc);
			AssertEquals ("Xml", test2, xel.OuterXml);
			AssertEquals ("Xml Actor", "", ts.Actor); // Actor is part of <Received> element
			AssertEquals ("Xml Created", "2001-09-13T08:42:00Z", ts.Created.ToString (WSTimestamp.TimeFormat));
			AssertEquals ("Xml Expires", "2001-10-13T09:00:00Z", ts.Expires.ToString (WSTimestamp.TimeFormat));
			AssertEquals ("Xml Receivers.Count", 1, ts.Receivers.Count);
			AllTests.AssertEquals ("Xml Ttl", 300000, ts.Ttl);
		}

		// WSE supports wsu:Id for Created and Expires elements
		[Test]
		public void LoadXmlWithCreatedAndExpiresIds () 
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test3);
			ts.LoadXml (doc.DocumentElement);

			XmlElement xel = ts.GetXml (doc);
			AssertEquals ("Xml", test3, xel.OuterXml);
			AssertEquals ("Xml Actor", "", ts.Actor); // Actor is part of <Received> element
			AssertEquals ("Xml Created", "2001-09-13T08:42:00Z", ts.Created.ToString (WSTimestamp.TimeFormat));
			AssertEquals ("Xml Expires", "2001-10-13T09:00:00Z", ts.Expires.ToString (WSTimestamp.TimeFormat));
			AssertEquals ("Xml Receivers.Count", 1, ts.Receivers.Count);
			AllTests.AssertEquals ("Xml Ttl", 300000, ts.Ttl);
		}

		// WSE _doesn't_ support wsu:Id for Timestamp and Received elements
		[Test]
		public void LoadXmlWithAllIds () 
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (test4);
			ts.LoadXml (doc.DocumentElement);

			XmlElement xel = ts.GetXml (doc);
			// FIXME (WSE) AssertEquals ("Xml", test4, xel.OuterXml);
			AssertEquals ("Xml Actor", "", ts.Actor); // Actor is part of <Received> element
			AssertEquals ("Xml Created", "2001-09-13T08:42:00Z", ts.Created.ToString (WSTimestamp.TimeFormat));
			AssertEquals ("Xml Expires", "2001-10-13T09:00:00Z", ts.Expires.ToString (WSTimestamp.TimeFormat));
			AssertEquals ("Xml Receivers.Count", 1, ts.Receivers.Count);
			AllTests.AssertEquals ("Xml Ttl", 300000, ts.Ttl);
		}
	}
}