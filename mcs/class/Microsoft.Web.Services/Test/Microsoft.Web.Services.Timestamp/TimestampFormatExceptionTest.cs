//
// TimestampFormatExceptionTest.cs - NUnit Test Cases for TimestampFormatException
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
	public class TimestampFormatExceptionTest : Assertion {

		[Test]
		public void Constructor () 
		{
			string msg = "message";
			TimestampFormatException e = new TimestampFormatException (msg);
			AssertEquals ("Message", msg, e.Message);
		}
	}
}