//
// WSTimestampTest.cs - NUnit Test Cases for WSTimestamp
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using Microsoft.Web.Services.Timestamp;
using System;

// note: due to compiler confusion between classes and namespace (like Timestamp)
// I renamed the test namespace from "MonoTests.Microsoft.Web.Services.Timestamp"
// to "MonoTests.MS.Web.Services.Timestamp".
namespace MonoTests.MS.Web.Services.Timestamp {

	[TestFixture]
	public class WSTimestampTest : Assertion {

		[Test]
		public void Constructor ()
		{
			WSTimestamp wsts = new WSTimestamp ();
			AssertNotNull ("Constructor", wsts);
		}

		[Test]
		public void PublicConstStrings ()
		{
			AssertEquals ("NamespaceURI", "http://schemas.xmlsoap.org/ws/2002/07/utility", WSTimestamp.NamespaceURI);
			AssertEquals ("Prefix", "wsu", WSTimestamp.Prefix);
			AssertEquals ("TimeFormat", "yyyy-MM-ddTHH:mm:ssZ", WSTimestamp.TimeFormat);
		}

		[Test]
		public void AttributeNamesConstructor ()
		{
			// test constructor
			WSTimestamp.AttributeNames an = new WSTimestamp.AttributeNames ();
			AssertNotNull ("AttributeNames Constructor", an);
		}

		[Test]
		public void AttributeNames ()
		{
			// test public const strings
			AssertEquals ("Actor", "Actor", WSTimestamp.AttributeNames.Actor);
			AssertEquals ("Delay", "Delay", WSTimestamp.AttributeNames.Delay);
			AssertEquals ("Id", "Id", WSTimestamp.AttributeNames.Id);
			AssertEquals ("ValueType", "ValueType", WSTimestamp.AttributeNames.ValueType);
		}

		[Test]
		public void ElementNamesConstructor ()
		{
			// test constructor
			WSTimestamp.ElementNames en = new WSTimestamp.ElementNames ();
			AssertNotNull ("ElementNames Constructor", en);
		}

		[Test]
		public void ElementNames ()
		{
			// test public const strings
			AssertEquals ("Created", "Created", WSTimestamp.ElementNames.Created);
			AssertEquals ("Expires", "Expires", WSTimestamp.ElementNames.Expires);
			AssertEquals ("Received", "Received", WSTimestamp.ElementNames.Received);
			AssertEquals ("Timestamp", "Timestamp", WSTimestamp.ElementNames.Timestamp);
		}
	}
}